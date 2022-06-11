namespace ServerMocker
{
    public class ResponseSequence : List<MapOrResponse>, IValidatable
    {
        public const string Id = "$Sequence";
        private static List<ResponseSequence> Sequences=new List<ResponseSequence>();
        public ResponseSequence()
        {
            Sequences.Add(this);
        }
        public static void ResetSequences()
        {
            foreach(var sequence in Sequences)
            {
                sequence.current = 0;
            }
            Console.WriteLine("Sequences Reset");
        }
        private int current = 0;
        public AbstractEndpointResponseSpec GetNext(string queryString)
        {
            AbstractEndpointResponseSpec ret=this[current];
            if (current < this.Count-1)
            {
                current++;
            }
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
