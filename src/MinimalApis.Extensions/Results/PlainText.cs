
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;
public class PlainText : StatusCode, IProvideEndpointResponseMetadata
{
    private const string PlainTextMediaType = "text/plain";

    public PlainText(string text)
        : base(StatusCodes.Status200OK, text, PlainTextMediaType)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesAttribute(PlainTextMediaType);
    }
}
