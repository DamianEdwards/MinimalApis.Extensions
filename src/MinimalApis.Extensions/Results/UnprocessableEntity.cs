using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class UnprocessableEntity : StatusCode, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status422UnprocessableEntity;

    public UnprocessableEntity(string? message = null)
        : base(ResponseStatusCode, message)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
