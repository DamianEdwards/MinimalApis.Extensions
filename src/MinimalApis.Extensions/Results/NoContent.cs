
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;
public class NoContent : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status204NoContent;

    public NoContent(string? message = null)
        : base(ResponseStatusCode, message)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
