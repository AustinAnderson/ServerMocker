using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ServerMocker.Models
{
    public class MapOrResponse:AbstractEndpointResponseSpec
    {

        [JsonProperty(QueryStringMap.Id)]
        public QueryStringMap? ByQueryString { get; set; }

        public override void Validate(string path)
        {
            var populateds = GetPopulatedSimpleResponseComponents();
            if(populateds != null && ByQueryString != null)
            {
                throw new JsonSerializationException(populateds + $" are set, simple requests and {QueryStringMap.Id} are mutually exclusive for this section");
            }
            if(ByQueryString != null)
            {
                ByQueryString.Validate(path+"."+QueryStringMap.Id);
            }
            else
            {
                ValidateStatusCode(path);
            }
        }
    }
}
