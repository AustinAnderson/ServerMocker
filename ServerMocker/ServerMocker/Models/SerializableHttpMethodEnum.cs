using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerMocker.Models
{
    [JsonConverter(typeof(JsonSerializer))]
    public class SerializableHttpMethodEnum
    {
        public string Verb { get; private set; }
        private static readonly Dictionary<string,SerializableHttpMethodEnum> mapFromString = new Dictionary<string, SerializableHttpMethodEnum>();
        private static readonly Dictionary<HttpMethod, SerializableHttpMethodEnum> mapFrom = new Dictionary<HttpMethod, SerializableHttpMethodEnum>();
        private static readonly Dictionary<SerializableHttpMethodEnum, HttpMethod> mapTo = new Dictionary<SerializableHttpMethodEnum, HttpMethod>();
        public SerializableHttpMethodEnum(HttpMethod method)
        {
            Verb = method.Method;
            mapFromString.Add(method.Method.ToLower(), this);
            mapFrom.Add(method, this);
            mapTo.Add(this,method);
        }
        public static implicit operator HttpMethod(SerializableHttpMethodEnum val)=>mapTo[val];
        public static explicit operator SerializableHttpMethodEnum(HttpMethod val)=>mapFrom[val];
        public static SerializableHttpMethodEnum Delete = new SerializableHttpMethodEnum(HttpMethod.Delete);
        public static SerializableHttpMethodEnum Get = new SerializableHttpMethodEnum(HttpMethod.Get);
        public static SerializableHttpMethodEnum Head = new SerializableHttpMethodEnum(HttpMethod.Head);
        public static SerializableHttpMethodEnum Options = new SerializableHttpMethodEnum(HttpMethod.Options);
        public static SerializableHttpMethodEnum Patch = new SerializableHttpMethodEnum(HttpMethod.Patch);
        public static SerializableHttpMethodEnum Post = new SerializableHttpMethodEnum(HttpMethod.Post);
        public static SerializableHttpMethodEnum Put = new SerializableHttpMethodEnum(HttpMethod.Put);
        public static SerializableHttpMethodEnum Trace = new SerializableHttpMethodEnum(HttpMethod.Trace);

        public class JsonSerializer : JsonConverter
        {
            public override bool CanConvert(Type objectType) => typeof(SerializableHttpMethodEnum) == objectType;
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                var str=reader.Value?.ToString()?.ToLower()??"(was C# null)";
                if(!mapFromString.TryGetValue(str,out var value))
                {
                    throw new JsonSerializationException($"Invalid HttpMethod '{str}'");
                }
                return value;
            }
            public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
            {
                if(value == null)
                {
                    writer.WriteNull();
                }
                else if(value is SerializableHttpMethodEnum e)
                {
                    ((JToken)e.Verb).WriteTo(writer);
                }
                else
                {
                    throw new NotImplementedException(
                        $"expected value to be SerializableHttpMethodEnum, but got {value?.GetType()} {value}"
                    );
                }
            }
        }
    }
}
