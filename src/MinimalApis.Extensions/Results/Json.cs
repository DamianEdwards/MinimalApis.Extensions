using System.Text;
using System.Text.Json;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that serializes an object to JSON content in the response body.
/// </summary>
public class Json : ObjectResult
{
    /// <summary>
    /// The <c>application/json</c> media type.
    /// </summary>
    protected const string JsonContentType = "application/json";

    /// <summary>
    /// Initializes a new intance of the <see cref="Json"/> class.
    /// </summary>
    /// <param name="value">The value to serialize as JSON to the response body.</param>
    public Json(object? value)
        : base(value)
    {

    }

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use when writing the response body JSON content.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; init; }

    /// <summary>
    /// Gets the default content type to use for the response if one isn't specified via <see cref="ObjectResult.ContentType"/>.
    /// </summary>
    public override string DefaultContentType => $"{JsonContentType}; charset=utf-8";

    /// <summary>
    /// Writes the response body JSON content.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the response.</param>
    /// <param name="contentTypeEncoding">The <see cref="Encoding"/> to encode the response content with.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    protected override async Task WriteResult(HttpContext httpContext, Encoding contentTypeEncoding)
    {
        await httpContext.Response.WriteAsJsonAsync(Value, JsonSerializerOptions, httpContext.Response.ContentType);
    }
}
