using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that returns HTML content in the response body.
/// </summary>
public sealed class Html : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IContentTypeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Html"/> class.
    /// </summary>
    /// <param name="html">The HTML to return in the response body.</param>
    internal Html(string? html)
    {
        Content = html;
    }

    /// <summary>
    /// Gets the status code: <see cref="StatusCodes.Status200OK"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status200OK;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <summary>
    /// Gets the content type: <c>text/html</c>
    /// </summary>
    public string ContentType => "text/html";

    /// <summary>
    /// Gets the HTML content to render in the response body.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = ContentType;
        if (Content is not null)
        {
            await httpContext.Response.WriteAsync(Content);
        }
    }

    /// <summary>
    /// Populates metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status200OK));
        context.EndpointMetadata.Add(new Mvc.ProducesAttribute("text/html"));
    }
}
