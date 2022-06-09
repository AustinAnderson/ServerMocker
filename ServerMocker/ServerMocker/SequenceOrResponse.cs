using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerMocker
{
    public class SequenceOrResponse : AbstractEndpointResponseSpec
    {

        public AbstractEndpointResponseSpec GetNext(string queryString)
        {
            if (GetPopulatedSimpleResponseComponents() != null)
            {
                return this;
            }
            else
            {
                return Sequence.GetNext(queryString);
            }
        }
        [JsonProperty("$Seq")]
        public ResponseSequence? Sequence { get; set; }

        public override void Validate(string path)
        {
            var populateds=GetPopulatedSimpleResponseComponents();
            if(populateds!=null && Sequence != null)
            {
                throw new JsonSerializationException(
                    populateds + " are set; simple response and $Seq are mutual exclusive",
                    path,0,0,null
                );
            }
            if(Sequence != null)
            {
                Sequence.Validate($"{path}.$Seq");
            }
            else
            {
                ValidateStatusCode(path);
            }
        }
    }
}
