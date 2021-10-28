using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class BadRequest : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status400BadRequest;

    public BadRequest(string? message = null, int statusCode = ResponseStatusCode)
        : base(statusCode, message)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
