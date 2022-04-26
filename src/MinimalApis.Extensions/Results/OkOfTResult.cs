using System.Text;
using System.Text.Json;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response that serializes an object to JSON content
/// in the response body.
/// </summary>
/// <typeparam name="TResult">The type object to be JSON serialized to the response body.</typeparam>
public class Ok<TResult> : IResult, IProvideEndpointResponseMetadata
{
    private const string JsonContentType = "application/json";

    /// <summary>
    /// Initializes a new instance of the <see cref="Ok{TResult}"/> class.
    /// </summary>
    /// <param name="value">The object to serialize to the response body.</param>
    public Ok(TResult value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets the value to be JSON serialized to the response body.
    /// </summary>
    public TResult Value { get; init; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int StatusCode => StatusCodes.Status200OK;

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        var response = httpContext.Response;

        response.StatusCode = StatusCode;

        if (Value is not null)
        {
            await httpContext.Response.WriteAsJsonAsync(Value, (JsonSerializerOptions?)null, JsonContentType);
        }
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), StatusCodes.Status200OK, JsonContentType);
    }
}
