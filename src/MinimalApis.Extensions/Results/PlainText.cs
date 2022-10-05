using Microsoft.AspNetCore.Builder;
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// An <see cref="IResult"/> that returns an Ok (200) status code response with a plain text response body.
/// </summary>
public sealed class PlainText : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IContentTypeHttpResult
{
    private const int ResponseStatusCode = StatusCodes.Status200OK;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainText"/> class.
    /// </summary>
    /// <param name="text">The text to write to the response body.</param>
    internal PlainText(string? text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status200OK"/>
    /// </summary>
    public int StatusCode => ResponseStatusCode;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <summary>
    /// Gets the value that will be set on the <c>Content-Type</c> header.
    /// </summary>
    public string ContentType => "text/plain";

    /// <summary>
    /// Gets the text that will be written to the response.
    /// </summary>
    public string? Text { get; }

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = ContentType;

        if (!string.IsNullOrEmpty(Text))
        {
            await httpContext.Response.WriteAsync(Text);
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
        metadata.Add(new ProducesResponseTypeMetadata(ResponseStatusCode, "text/plain"));
    }
}
