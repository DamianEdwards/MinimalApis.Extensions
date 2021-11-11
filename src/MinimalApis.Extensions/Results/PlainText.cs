using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response with a plain text response body.
/// </summary>
public class PlainText : TextResult, IProvideEndpointResponseMetadata
{
    private const string PlainTextMediaType = "text/plain";
    private const int ResponseStatusCode = StatusCodes.Status200OK;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainText"/> class.
    /// </summary>
    /// <param name="text">The text to write to the response body.</param>
    public PlainText(string text)
        : base(text, PlainTextMediaType)
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
        yield return new Mvc.ProducesAttribute(PlainTextMediaType);
    }
}
