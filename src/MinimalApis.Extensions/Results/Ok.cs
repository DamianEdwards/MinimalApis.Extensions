#if NET6_0
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Ok (200) status code.
/// </summary>
public sealed class Ok : IResult, IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ok"/> class with the values.
    /// </summary>
    internal Ok()
    {
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status200OK"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status200OK;

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
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status200OK));
    }
}
#endif
