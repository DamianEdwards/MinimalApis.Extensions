using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status201Created"/> response.
/// </summary>
public class Created : Json, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// The <see cref="StatusCodes.Status201Created"/> response status code.
    /// </summary>
    protected const int ResponseStatusCode = StatusCodes.Status201Created;

    /// <summary>
    /// Initializes a new instance of the <see cref="Created"/> class.
    /// </summary>
    /// <param name="uri">The URI the location response header will be set to.</param>
    /// <param name="value">An optional value representing the created entity.</param>
    public Created(string uri, object? value)
        : base(value)
    {
        Uri = uri;
        StatusCode = ResponseStatusCode;
    }

    /// <summary>
    /// Gets the URI that the location response header will be set to.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public override Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Location = Uri;
        return base.ExecuteAsync(httpContext);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
