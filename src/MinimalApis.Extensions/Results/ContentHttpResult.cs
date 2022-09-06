#if NET6_0

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="ContentHttpResult"/> that when executed
/// will produce a response with content.
/// </summary>
public sealed partial class ContentHttpResult : IResult, IStatusCodeHttpResult, IContentTypeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentHttpResult"/> class with the values.
    /// </summary>
    /// <param name="content">The value to format in the entity body.</param>
    /// <param name="contentType">The Content-Type header for the response</param>
    internal ContentHttpResult(string? content, string? contentType)
        : this(content, contentType, statusCode: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentHttpResult"/> class with the values
    /// </summary>
    /// <param name="content">The value to format in the entity body.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="contentType">The Content-Type header for the response</param>
    internal ContentHttpResult(string? content, string? contentType, int? statusCode)
    {
        ResponseContent = content;
        StatusCode = statusCode;
        ContentType = contentType;
    }

    /// <summary>
    /// Gets the content representing the body of the response.
    /// </summary>
    public string? ResponseContent { get; internal init; }

    /// <summary>
    /// Gets the Content-Type header for the response.
    /// </summary>
    public string? ContentType { get; internal init; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; internal init; }

    /// <summary>
    /// Writes the content to the HTTP response.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (StatusCode is { } statusCode)
        {
            httpContext.Response.StatusCode = statusCode;
        }

        if (ContentType is { } contentType)
        {
            httpContext.Response.ContentType = contentType;
        }

        if (ResponseContent is { } responseContent)
        {
            await httpContext.Response.WriteAsync(responseContent);
        }
    }
}
#endif
