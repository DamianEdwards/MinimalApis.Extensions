using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status422UnprocessableEntity"/> response.
/// </summary>
public class UnprocessableEntity : ResultBase, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status422UnprocessableEntity;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntity"/> class.
    /// </summary>
    /// <param name="message">An optional message to return in the response body.</param>
    public UnprocessableEntity(string? message = null)
    {
        ResponseContent = message;
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
