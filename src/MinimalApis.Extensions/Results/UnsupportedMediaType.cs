using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status415UnsupportedMediaType"/> response.
/// </summary>
public sealed class UnsupportedMediaType : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedMediaType"/> class.
    /// </summary>
    internal UnsupportedMediaType()
    {

    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status415UnsupportedMediaType"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status415UnsupportedMediaType;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status415UnsupportedMediaType));
    }
}
