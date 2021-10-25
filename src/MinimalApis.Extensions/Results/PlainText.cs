using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results;

using MinimalApis.Extensions.Metadata;

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
