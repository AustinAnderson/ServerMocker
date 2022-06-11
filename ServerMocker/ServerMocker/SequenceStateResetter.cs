using ServerMocker.Models;

namespace ServerMocker
{
    public class SequenceStateResetter
    {
        //should reference equal the spec list used to build the endpoints
        private readonly EndpointSpecification[] specs;
        public SequenceStateResetter(EndpointSpecification[] specs)
        {
            this.specs = specs;
        }
        public void ResetSequences()
        {
            foreach (var spec in specs)
            {
                if(spec.Sequence != null)
                {
                    ResetSequence(spec.Sequence);
                }
                else if(spec.ByQueryString != null)
                {
                    ResetMap(spec.ByQueryString);
                }
            }
        }
        private void ResetSequence(ResponseSequence sequence)
        {
            sequence.Reset();
            foreach(var mapOrSimple in sequence)
            {
                if(mapOrSimple.ByQueryString != null)
                {
                    ResetMap(mapOrSimple.ByQueryString);
                }
            }
        }
        private void ResetMap(QueryStringMap map)
        {
            foreach(var seqOrSimple in map)
            {
                if(seqOrSimple.Value.Sequence != null)
                {
                    ResetSequence(seqOrSimple.Value.Sequence);
                }
            }
        }
    }
}
