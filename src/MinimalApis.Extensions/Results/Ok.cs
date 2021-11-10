using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response.
/// </summary>
public class Ok : StatusCode, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// The <see cref="StatusCodes.Status200OK"/> status code.
    /// </summary>
    protected const int ResponseStatusCode = StatusCodes.Status200OK;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ok"/> class.
    /// </summary>
    /// <param name="message">An optional message to return in the response body.</param>
    public Ok(string? message = null)
        : base(ResponseStatusCode, message)
    {

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
