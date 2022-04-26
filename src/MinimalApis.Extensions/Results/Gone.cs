using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status410Gone"/> response.
/// </summary>
public sealed class Gone : IResult, IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gone"/> class.
    /// </summary>
    internal Gone()
    {

    }

    // <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status201Created"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status410Gone;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status410Gone);
    }
}
