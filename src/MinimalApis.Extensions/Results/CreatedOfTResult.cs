using Mvc = Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class Created<TResult> : Created, IProvideEndpointResponseMetadata
{
    public Created(string uri, TResult? value)
        : base(uri, value)
    {

    }

    public static new IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(TResult), ResponseStatusCode, JsonContentType);
    }
}
