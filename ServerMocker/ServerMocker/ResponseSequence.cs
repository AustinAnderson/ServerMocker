namespace ServerMocker
{
    public class ResponseSequence : List<MapOrResponse>, IValidatable
    {
        private int current = 0;
        public AbstractEndpointResponseSpec GetNext(string queryString)
        {
            AbstractEndpointResponseSpec ret=this[current];
            current++;
            var map = (ret as MapOrResponse)?.ByQueryString;
            if(map != null)
            {
                ret = map.MatchOrDefault(queryString).GetNext(queryString);
            }
            return ret;
        }

        public void Validate(string path)
        {
            for(int i = 0; i < Count; i++)
            {
                this[i].Validate($"{path}[{i}]");
            }
        }
    }
}
