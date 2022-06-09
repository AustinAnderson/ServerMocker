using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ServerMocker
{
    public class MapOrResponse:AbstractEndpointResponseSpec
    {

        [JsonProperty("$QueryMap")]
        public QueryStringMap? ByQueryString { get; set; }

        public override void Validate(string path)
        {
            var populateds = GetPopulatedSimpleResponseComponents();
            if(populateds != null && ByQueryString != null)
            {
                throw new JsonSerializationException(populateds + " are set, simple requests and $QueryMap are mutually exclusive for this section");
            }
            if(ByQueryString != null)
            {
                ByQueryString.Validate(path+".$QueryMap");
            }
            else
            {
                ValidateStatusCode(path);
            }
        }
    }
}
