using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AiApp;

public class QueryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(string));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
                return reader.Value.ToString();
            case JsonToken.StartObject:
                JObject obj = JObject.Load(reader);
                return obj.ToString(Formatting.None);
            default:
                throw new JsonSerializationException("Expected string or object for 'query' property.");
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}