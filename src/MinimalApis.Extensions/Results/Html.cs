using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that returns HTML content in the response body.
/// </summary>
public class Html : ResultBase, IProvideEndpointResponseMetadata
{
    private const string HtmlMediaType = "text/html";
    private const int ResponseStatusCode = StatusCodes.Status200OK;

    /// <summary>
    /// Initializes a new instance of the <see cref="Html"/> class.
    /// </summary>
    /// <param name="html">The HTML to return in the response body.</param>
    public Html(string? html)
    {
        ResponseContent = html;
        ContentType = HtmlMediaType;
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
        yield return new Mvc.ProducesAttribute(HtmlMediaType);
    }
}
