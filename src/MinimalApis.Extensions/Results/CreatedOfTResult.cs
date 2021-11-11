using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status201Created"/> response for the creation
/// of an entity represented by the <typeparamref name="TResult"/> type.
/// </summary>
public class Created<TResult> : Created, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Created{TResult}"/> class.
    /// </summary>
    /// <param name="uri">The URI the location response header will be set to.</param>
    /// <param name="value">An optional value representing the created entity.</param>
    public Created(string uri, TResult? value)
        : base(uri, value)
    {

    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static new IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), ResponseStatusCode, JsonContentType);
    }
}
