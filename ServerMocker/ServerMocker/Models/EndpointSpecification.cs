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

        [JsonProperty(ResponseSequence.Id)]
        public ResponseSequence? Sequence { get; set; }
        [JsonProperty(QueryStringMap.Id)]
        public QueryStringMap? ByQueryString { get; set; }

        public override void Validate(string path)
        {
            var populateds = GetPopulatedSimpleResponseComponents();
            bool isSimple = populateds != null;
            bool isSequence = Sequence != null;
            bool isMap = ByQueryString != null;
            var errorMessage = (isSimple, isSequence, isMap) switch
            {
                (false, false, false) => $"Either status code for simple response, {ResponseSequence.Id}, or {QueryStringMap.Id} must be set",
                (false, true, true) => $"{ResponseSequence.Id} and {QueryStringMap.Id} are mutually exclusive",
                (true, false, true) => populateds+$" and {QueryStringMap.Id} are set; simple response and {QueryStringMap.Id} are mutually exclusive",
                (true, true, false) => populateds+$" and {ResponseSequence.Id} are set; simple response and {ResponseSequence.Id} are mutually exclusive",
                (true, true, true) => populateds+$", {QueryStringMap.Id}, and {ResponseSequence.Id} are set; simple response, {ResponseSequence.Id}, and {QueryStringMap.Id} are mutually exclusive",
                _ => null
            };
            if (errorMessage != null)
            {
                throw new JsonSerializationException(errorMessage,path,0,0,null);
            }
            if (ByQueryString != null)
            {
                ByQueryString.Validate($"{path}.{QueryStringMap.Id}");
            }
            else if(Sequence != null)
            {
                Sequence.Validate($"{path}.{ResponseSequence.Id}");
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
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ffff tt") + ":   ");
            Console.WriteLine(ctx.Request.Method + " " + ctx.Request.Path+ctx.Request.QueryString);
            var bodyStr=await new StreamReader(ctx.Request.Body).ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(bodyStr))
            {
                Console.WriteLine(
                    JsonConvert.DeserializeObject<JToken>(bodyStr)?.ToString()
                );
            }
            if (spec.HasDelay)
            {
                Console.Write($"Pausing {spec.DelayInSeconds}s... ");
                await Task.Delay(TimeSpan.FromSeconds(spec.DelayInSeconds));
            }
            Console.WriteLine("Returning "+spec.StatusCode);
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
            ctx.Response.StatusCode = spec.StatusCode;
            if (spec.Body != null)
            {
                await ctx.Response.WriteAsync(spec.Body.ToString());
            }
        }

    }
}
