using System.Text;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that returns the result of serializing a supplied object as the content in the response body.
/// </summary>
public abstract class ObjectResult : IResult
{
    //protected const string DefaultContentType = "application/json; charset=utf-8";
    protected static readonly Encoding DefaultEncoding = Encoding.UTF8;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectResult"/> class.
    /// </summary>
    /// <param name="value">The value to be serialized to the response body.</param>
    public ObjectResult(object? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value to be serialized to the response body.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the default content type to use for the response if one is specified via <see cref="ContentType"/>.
    /// </summary>
    public abstract string DefaultContentType { get; }

    /// <summary>
    /// Gets or sets the content type to use for the response.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or sets the response status code.
    /// </summary>
    public int? StatusCode { get; init; }


    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
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

    /// <summary>
    /// Writes the response body content.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the response.</param>
    /// <param name="contentTypeEncoding">The <see cref="Encoding"/> to encode the response content with.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    protected abstract Task WriteResult(HttpContext httpContext, Encoding contentTypeEncoding);
}
