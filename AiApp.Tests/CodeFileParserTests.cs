using AiApp;
using FluentAssertions;

namespace Gptd.ChatLib.Tests;

public class CodeFileParserTests
{
    [Fact]
    public async void CodeFileParser()
    {
        var appModel = new CodeFileParserChat();
        var content = await appModel.Ask(CodeFile);
        // content.Should().Contain("did it!");
    }

    private string CodeFile = """
                              QueryConverter.cs:
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
                              """;
}