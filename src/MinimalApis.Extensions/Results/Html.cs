
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;
public class Html : Text, IProvideEndpointResponseMetadata
{
    private const string HtmlMediaType = "text/html";

    public Html(string html)
        : base(html, HtmlMediaType)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesAttribute(HtmlMediaType);
    }
}
