using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results;

using MinimalApis.Extensions.Metadata;

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
