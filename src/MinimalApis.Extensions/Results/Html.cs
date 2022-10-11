using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that returns HTML content in the response body.
/// </summary>
public sealed class Html : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IContentTypeHttpResult
{
    private const int ResponseStatusCode = StatusCodes.Status200OK;

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
    public int StatusCode => ResponseStatusCode;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <summary>
    /// Gets the content type: <c>text/html</c>
    /// </summary>
    public string ContentType => "text/html";

    /// <summary>
    /// Gets the HTML content to render in the response body.
    /// </summary>
    public string? Content { get; }

    /// <inheritdoc />
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

#if NET7_0_OR_GREATER
    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="builder"></param>
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        PopulateMetadataImpl(method, builder.Metadata, builder.ApplicationServices);
    }
#else
    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="metadata"></param>
    /// <param name="services"></param>
    public static void PopulateMetadata(MethodInfo method, IList<object> metadata, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(services);

        PopulateMetadataImpl(method, metadata, services);
    }
#endif

    private static void PopulateMetadataImpl(MethodInfo method, IList<object> metadata, IServiceProvider services)
    {
        metadata.Add(new ProducesResponseTypeMetadata(ResponseStatusCode, "text/html"));
    }
}
