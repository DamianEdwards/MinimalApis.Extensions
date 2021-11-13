using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response that serializes an object to JSON content in the response body.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class Ok<TResult> : Json, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// The <see cref="StatusCodes.Status200OK"/> response status code.
    /// </summary>
    protected const int ResponseStatusCode = StatusCodes.Status200OK;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ok{TResult}"/> class.
    /// </summary>
    /// <param name="result">The object to serialize to the response body.</param>
    public Ok(TResult result)
        : base(result)
    {
        Result = result;
        StatusCode = ResponseStatusCode;
    }

    /// <summary>
    /// Gets the object that will be serialized to the response body.
    /// </summary>
    public TResult Result { get; init; }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), ResponseStatusCode, JsonContentType);
    }
}
