using System.Text.Json;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that serializes an object to JSON content in the response body.
/// </summary>
public class Json : IResult, IProvideEndpointResponseMetadata
{
    private const string JsonContentType = "application/json";

    /// <summary>
    /// Initializes a new intance of the <see cref="Json"/> class.
    /// </summary>
    /// <param name="value">The value to serialize as JSON to the response body.</param>
    public Json(object? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value to be serialized to the response body.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets or sets the response status code.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use when writing the response body JSON content.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; init; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public virtual async Task ExecuteAsync(HttpContext httpContext)
    {
        if (StatusCode is { } code)
        {
            httpContext.Response.StatusCode = code;
        }

        await httpContext.Response.WriteAsJsonAsync(Value, JsonSerializerOptions, JsonContentType);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesAttribute(JsonContentType);
    }
}
