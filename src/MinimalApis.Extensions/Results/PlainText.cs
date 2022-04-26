using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response with a plain text response body.
/// </summary>
public sealed class PlainText : IResult, IEndpointMetadataProvider
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
    public int StatusCode => StatusCodes.Status200OK;

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
        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = ContentType;

        if (!string.IsNullOrEmpty(Text))
        {
            await httpContext.Response.WriteAsync(Text);
        }
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode));
        context.EndpointMetadata.Add(new Mvc.ProducesAttribute("text/plain"));
    }
}
