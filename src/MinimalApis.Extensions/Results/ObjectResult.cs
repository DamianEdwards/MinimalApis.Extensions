using System.Text;

namespace MinimalApis.Extensions.Results;

public abstract class ObjectResult : IResult
{
    //protected const string DefaultContentType = "application/json; charset=utf-8";
    protected static readonly Encoding DefaultEncoding = Encoding.UTF8;

    public ObjectResult(object? value)
    {
        Value = value;
    }

    public object? Value { get; }

    public abstract string DefaultContentType { get; }

    public string? ContentType { get; init; }

    public int? StatusCode { get; init; }

    public virtual async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;

        ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
            ContentType,
            response.ContentType,
            (DefaultContentType, DefaultEncoding),
            ResponseContentTypeHelper.GetEncoding,
            out var resolvedContentType,
            out var resolvedContentTypeEncoding);

        response.ContentType = resolvedContentType;

        if (StatusCode != null)
        {
            response.StatusCode = StatusCode.Value;
        }

        await WriteResult(httpContext, resolvedContentTypeEncoding);
    }

    protected abstract Task WriteResult(HttpContext httpContext, Encoding contentTypeEncoding);
}
