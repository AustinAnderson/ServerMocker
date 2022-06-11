using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace ServerMocker
{
    public class QueryStringMap : Dictionary<string, SequenceOrResponse>, IValidatable
    {
        public const string Id = "$QueryMap";
        public SequenceOrResponse MatchOrDefault(string toMatch)
        {
            foreach(var kvp in this)
            {
                if (Regex.IsMatch(toMatch,kvp.Key))
                {
                    return kvp.Value;
                }
            }
            return Default;
        }
        public SequenceOrResponse Default
        {
            get
            {
                foreach(var kvp in this)
                {
                    if (kvp.Key.ToString() == "$Default")
                    {
                        return kvp.Value;
                    }
                }
                return null;
            }
        }
        public void Validate(string path)
        {
            if (Default == null)
            {
                throw new JsonSerializationException(
                    "Querystring maps must contain a default keyed as $Default",
                    path, 0, 0, null
                );
            }
            foreach(var kvp in this)
            {
                if(kvp.Value == null)
                {
                    throw new JsonSerializationException(
                        "response section for query filter can't be null",
                        $"{path}[{kvp.Key}]", 0, 0, null
                    );
                }
                else
                {
                    kvp.Value.Validate($"{path}[{kvp.Key}]");
                }
            }
        }
    }
}
