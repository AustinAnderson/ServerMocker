using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerMocker.Models
{
    [JsonConverter(typeof(JsonSerializer))]
    public class SerializableHttpMethodEnum
    {
        public string Verb { get; private set; }
        private static readonly Dictionary<string,SerializableHttpMethodEnum> mapFromString = new Dictionary<string, SerializableHttpMethodEnum>();
        public SerializableHttpMethodEnum(string method)
        {
            Verb = method;
            mapFromString.Add(method.ToLower(), this);
        }
        public static SerializableHttpMethodEnum Delete = new SerializableHttpMethodEnum(HttpMethods.Delete);
        public static SerializableHttpMethodEnum Get = new SerializableHttpMethodEnum(HttpMethods.Get);
        public static SerializableHttpMethodEnum Head = new SerializableHttpMethodEnum(HttpMethods.Head);
        public static SerializableHttpMethodEnum Options = new SerializableHttpMethodEnum(HttpMethods.Options);
        public static SerializableHttpMethodEnum Patch = new SerializableHttpMethodEnum(HttpMethods.Patch);
        public static SerializableHttpMethodEnum Post = new SerializableHttpMethodEnum(HttpMethods.Post);
        public static SerializableHttpMethodEnum Put = new SerializableHttpMethodEnum(HttpMethods.Put);
        public static SerializableHttpMethodEnum Trace = new SerializableHttpMethodEnum(HttpMethods.Trace);

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
