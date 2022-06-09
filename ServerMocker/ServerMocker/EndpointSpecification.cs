using Microsoft.AspNetCore.Routing.Patterns;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ServerMocker
{
    /// <summary>
    /// defines a single endpoint, with behavior defined as either branching on query strings,
    /// a canned response, or a sequence of responses
    /// </summary>
    public class EndpointSpecification:AbstractEndpointResponseSpec
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonRequired]
        public string ApiRoute { get; set; }
        [JsonRequired]
        public SerializableHttpMethodEnum Method { get; set; }
#pragma warning restore CS8618 // handled via attribute

        [JsonProperty("$Seq")]
        public ResponseSequence? Sequence { get; set; }
        [JsonProperty("$QueryMap")]
        public QueryStringMap? ByQueryString { get; set; }

        public override void Validate(string path)
        {
            var populateds = GetPopulatedSimpleResponseComponents();
            bool isSimple = populateds != null;
            bool isSequence = Sequence != null;
            bool isMap = ByQueryString != null;
            var errorMessage = (isSimple, isSequence, isMap) switch
            {
                (false, false, false) => "Either status code for simple response, $Seq, or $QueryMap must be set",
                (false, true, true) => "$Seq and $QueryMap are mutually exclusive",
                (true, false, true) => populateds+" are set; simple response and $QueryMap are mutually exclusive",
                (true, true, false) => populateds+" are set; simple response and $Seq are mutually exclusive",
                (true, true, true) => populateds+" are set; simple response, $Seq, and $QueryMap are mutually exclusive",
                _ => null
            };
            if (errorMessage != null)
            {
                throw new JsonSerializationException(errorMessage,path,0,0,null);
            }
            if (ByQueryString != null)
            {
                ByQueryString.Validate($"{path}.$QueryMap");
            }
            else if(Sequence != null)
            {
                Sequence.Validate($"{path}.$Seq");
            }
            else
            {
                ValidateStatusCode(path);
            }
        }
        public RequestDelegate ToRequestDelegate()
        {

            return new RequestDelegate(async httpCtx =>
            {
                AbstractEndpointResponseSpec endpointResp = this;
                var query = httpCtx.Request.QueryString.Value;
                if(Sequence != null)
                {
                    endpointResp=Sequence.GetNext(query);
                }
                else if(ByQueryString != null )
                {
                    endpointResp=ByQueryString.MatchOrDefault(query).GetNext(query);
                }
                await WriteResponseFromSpec(httpCtx, endpointResp);
            });
        }


        private async Task WriteResponseFromSpec(HttpContext ctx, AbstractEndpointResponseSpec spec)
        {
            await Task.Delay(TimeSpan.FromSeconds(spec.DelayInSeconds));
            if(spec.Headers != null && spec.Headers.Any())
            {
                foreach(var header in spec.Headers)
                {
                    if (!ctx.Response.Headers.ContainsKey(header.Key))
                    {
                        ctx.Response.Headers.Add(header.Key, header.Value);
                    }
                }
            }
            ctx.Response.StatusCode = (int)spec.StatusCode;
            if (spec.Body != null)
            {
                await ctx.Response.WriteAsync(spec.Body.ToString());
            }
        }

    }
}
