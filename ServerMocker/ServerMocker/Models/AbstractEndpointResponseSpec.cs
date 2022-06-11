using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerMocker
{
    public abstract class AbstractEndpointResponseSpec:IValidatable
    {
        public double DelayInSeconds { get; set; }
        public int StatusCode { get; set; }
        public JToken? Body { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        private const double ZeroEpsilon = 0.0000000001;
        public bool HasDelay => !(DelayInSeconds > -ZeroEpsilon && DelayInSeconds < ZeroEpsilon);
        protected string? GetPopulatedSimpleResponseComponents()
        {
            List<string> populateds = new List<string>();
            if(HasDelay) 
            {
                populateds.Add(nameof(DelayInSeconds));
            }
            if(StatusCode != 0) 
            {
                populateds.Add(nameof(StatusCode));
            }
            if(Body != null)
            {
                populateds.Add(nameof(Body));
            }
            if(Headers != null)
            {
                populateds.Add(nameof(Headers));
            }
            if (!populateds.Any())
            {
                return null;
            }
            if(populateds.Count == 1)
            {
                return populateds[0];
            }
            return string.Join(", ", populateds);
        }
        protected void ValidateStatusCode(string path)
        {
            if(StatusCode>599 || StatusCode < 100)
            {
                throw new JsonSerializationException(
                    $"invalid status {StatusCode}, status code must be in the range 100-599", path + "." + nameof(StatusCode),
                    1,1,null
                );
            }
        }
        public abstract void Validate(string path);
    }
}
