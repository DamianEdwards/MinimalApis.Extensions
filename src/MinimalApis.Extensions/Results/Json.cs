using System.Text;
using System.Text.Json;

namespace MinimalApis.Extensions.Results;

public class Json : ObjectResult
{
    protected const string JsonContentType = "application/json";

    public Json(object? value)
        : base(value)
    {

    }

    public JsonSerializerOptions? JsonSerializerOptions { get; init; }

    public override string DefaultContentType => $"{JsonContentType}; charset=utf-8";

    protected override async Task WriteResult(HttpContext httpContext, Encoding contentTypeEncoding)
    {
        await httpContext.Response.WriteAsJsonAsync(Value, JsonSerializerOptions, ContentType);
    }
}
