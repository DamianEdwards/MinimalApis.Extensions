using System.Xml.Serialization;

using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;
public class CreatedJsonOrXml<T> : IResult, IProvideEndpointResponseMetadata
{
    private readonly T _responseBody;
    private readonly string _contentType;

    public CreatedJsonOrXml(T responseBody, string contentType)
    {
        ThrowIfUnsupportedContentType(contentType);

        _responseBody = responseBody;
        _contentType = contentType;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        // Likely should honor Accpets header, etc.
        httpContext.Response.StatusCode = StatusCodes.Status201Created;
        httpContext.Response.ContentType = _contentType;

        switch (_contentType)
        {
            case "application/xml":
                // This is terrible code, don't do this
                var xml = new XmlSerializer(typeof(T));
                using (var ms = new MemoryStream())
                {
                    xml.Serialize(ms, _responseBody);
                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(httpContext.Response.Body);
                }
                break;

            case "application/json":
            default:
                await httpContext.Response.WriteAsJsonAsync(_responseBody);
                break;
        }
    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(T), StatusCodes.Status201Created, "application/json", "application/xml");
    }

    internal static void ThrowIfUnsupportedContentType(string contentType)
    {
        if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(contentType, "application/xml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Value provided for {contentType} must be either 'application/json' or 'application/xml'.", nameof(contentType));
        }
    }
}
