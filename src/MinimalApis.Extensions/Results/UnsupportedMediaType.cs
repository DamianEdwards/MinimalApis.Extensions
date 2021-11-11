using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status415UnsupportedMediaType"/> response.
/// </summary>
public class UnsupportedMediaType : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status415UnsupportedMediaType;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedMediaType"/> class.
    /// </summary>
    /// <param name="message">An optional message to return in the response body.</param>
    public UnsupportedMediaType(string? message = null)
        : base(ResponseStatusCode, message)
    {

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
