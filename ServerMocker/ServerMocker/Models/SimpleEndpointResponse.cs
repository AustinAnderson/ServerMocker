using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace ServerMocker.Models
{
    public class SimpleEndpointResponse : AbstractEndpointResponseSpec
    {
        public override void Validate(string path) 
        {
            ValidateStatusCode(path);
        }
    }
}
