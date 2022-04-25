using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MinimalApis.Extensions.Binding;

namespace MinimalApis.Extensions.UnitTests.Binding;

public class FormOfT
{
    [Fact]
    public void FormToJsonTransform()
    {
        using var ms = new MemoryStream();
        var jsonWriter = new Utf8JsonWriter(ms);
        var form = new FormCollection(new ()
        {
            { "Abc", "123" },
            { "Baz.Bar", "String value" },
            { "Cat", "String value" },
            { "Dog.Furp.Sit", "false" },
            { "Elk", "String value" },
            { "Fab.Boo", "456" },
            { "Fab.Cod", "true" },
            { "Gru.Dad", "String value" }
        });
        
        Form<object>.Transform(form, jsonWriter);
        ms.Position = 0;
        using var sr = new StreamReader(ms);
        var json = sr.ReadToEnd();

        Assert.Equal(
            "{" +
              "\"Abc\":123," +
              "\"Baz\":{" +
                "\"Bar\":\"String value\"" +
              "}," +
              "\"Cat\":\"String value\"," +
              "\"Dog\":{" +
                "\"Furp\":{" +
                  "\"Sit\":false" +
                "}" +
              "}," +
              "\"Elk\":\"String value\"," +
              "\"Fab\":{" +
                "\"Boo\":456," +
                "\"Cod\":true" +
              "}," +
              "\"Gru\":{" +
                "\"Dad\":\"String value\"" +
              "}" +
            "}",
            json);
    }
}
