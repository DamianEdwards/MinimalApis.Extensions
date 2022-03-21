using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status302Found"/> redirect response.
/// </summary>
public class RedirectTemporary : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status302Found;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectTemporary"/> class.
    /// </summary>
    /// <param name="uri">The URI to redirect to.</param>
    public RedirectTemporary(string uri)
        : base(ResponseStatusCode, null)
    {
        Uri = uri;
    }

    /// <summary>
    /// The URI to redirect to.
    /// </summary>
    public string Uri { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Location = Uri;

        return base.ExecuteAsync(httpContext);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
