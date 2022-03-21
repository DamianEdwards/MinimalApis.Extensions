using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status204NoContent"/> response.
/// </summary>
public class NoContent : ResultBase, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status204NoContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoContent"/> class.
    /// </summary>
    public NoContent()
    {
        StatusCode = ResponseStatusCode;
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
