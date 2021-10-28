using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class UnsupportedMediaType : StatusCode, IProvideEndpointResponseMetadata
{
    public UnsupportedMediaType(string? message = null)
        : base(StatusCodes.Status415UnsupportedMediaType, message)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status415UnsupportedMediaType);
    }
}
