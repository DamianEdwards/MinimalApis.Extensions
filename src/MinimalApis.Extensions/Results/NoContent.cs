#if NET6_0
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Represents an <see cref="IResult"/> that when executed will
/// produce an HTTP response with the No Content (204) status code.
/// </summary>
public class NoContent : IResult, IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoContent"/> class.
    /// </summary>
    internal NoContent()
    {
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status204NoContent"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status204NoContent;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void IEndpointMetadataProviderPopulateMetadata(EndpointMetadataContext context)
    {
        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
    }
}
#endif
