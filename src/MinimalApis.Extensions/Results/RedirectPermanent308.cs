using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status308PermanentRedirect"/> redirect response.
/// </summary>
public class RedirectPermanent308 : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status308PermanentRedirect;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectPermanent"/> class.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    public RedirectPermanent308(string url)
        : base(ResponseStatusCode, null)
    {
        Url = url;
    }

    /// <summary>
    /// The URL to redirect to.
    /// </summary>
    public string Url { get; init; }

    /// <inheritdoc />
    public override Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Location = Url;

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
