using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class Ok<TResult> : Json, IProvideEndpointResponseMetadata
{
    public Ok(TResult result)
        : base(result)
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), StatusCodes.Status200OK, JsonContentType);
    }
}
