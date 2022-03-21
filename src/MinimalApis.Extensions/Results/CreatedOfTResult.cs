using System.Text;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status201Created"/> response for the creation
/// of an entity represented by the <typeparamref name="TResult"/> type.
/// </summary>
public class Created<TResult> : IResult, IProvideEndpointResponseMetadata
{
    private const string JsonContentType = "application/json";
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;

    /// <summary>
    /// Initializes a new instance of the <see cref="Created{TResult}"/> class.
    /// </summary>
    /// <param name="uri">The URI the location response header will be set to.</param>
    /// <param name="value">An optional value representing the created entity.</param>
    public Created(string uri, TResult? value)
    {
        Uri = uri;
        Value = value;
    }

    /// <summary>
    /// Gets the URI that the location response header will be set to.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// Gets the value to be serialized to the response body.
    /// </summary>
    public TResult? Value { get; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int StatusCode => StatusCodes.Status201Created;

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;

        response.StatusCode = StatusCode;

        if (Value is not null)
        {
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                null,
                response.ContentType,
                (JsonContentType, DefaultEncoding),
                ResponseContentTypeHelper.GetEncoding,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            await httpContext.Response.WriteAsJsonAsync(Value, null, resolvedContentType);
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
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), StatusCodes.Status201Created, JsonContentType);
    }
}
