using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

namespace AiApp;

public static class SchemaLookup
{
    private static readonly Dictionary<Type, JObject> _schemaMappings = new Dictionary<Type, JObject>();

    public static JObject GetSchemaForType(Type t)
    {
        if (!_schemaMappings.ContainsKey(t))
        {
            _schemaMappings[t] = GetParams(t);
        }

        return _schemaMappings[t];
    }

    private static JObject GetParams(Type type)
    {
        var generator = new JSchemaGenerator();
        generator.SchemaLocationHandling = SchemaLocationHandling.Inline;
        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        var functionParamSchema = generator.Generate(type);
        var schemaJson = functionParamSchema.ToString();
        return JObject.Parse(schemaJson);
    }
}