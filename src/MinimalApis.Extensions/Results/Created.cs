using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class Created : Json, IProvideEndpointResponseMetadata
{
    protected const int ResponseStatusCode = StatusCodes.Status201Created;

    public Created(string uri, object? value)
        : base(value)
    {
        Uri = uri;
        StatusCode = ResponseStatusCode;
    }

    public string Uri { get; }

    public override Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Headers.Location = Uri;
        return base.ExecuteAsync(httpContext);
    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
    }
}
