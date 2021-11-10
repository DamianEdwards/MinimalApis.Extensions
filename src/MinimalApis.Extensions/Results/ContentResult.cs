using System.Text;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that returns non-streamed content in the response body.
/// </summary>
public abstract class ContentResult : IResult
{
    private const string DefaultContentType = "text/plain; charset=utf-8";
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;

    /// <summary>
    /// The content type of the response body content.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// The response body content.
    /// </summary>
    public string? ResponseContent { get; init; }

    /// <summary>
    /// The status code to return.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;

        if (StatusCode != null)
        {
            response.StatusCode = StatusCode.Value;
        }

        if (ResponseContent != null)
        {
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                ContentType,
                response.ContentType,
                (DefaultContentType, DefaultEncoding),
                ResponseContentTypeHelper.GetEncoding,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;
            response.ContentLength = resolvedContentTypeEncoding.GetByteCount(ResponseContent);
            await response.WriteAsync(ResponseContent, resolvedContentTypeEncoding);
        }
    }
}
