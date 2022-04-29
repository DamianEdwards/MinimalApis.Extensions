using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// An <see cref="IResult"/> that returns a Gone (410) status code.
/// </summary>
public sealed class Gone : IResult, IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gone"/> class.
    /// </summary>
    internal Gone()
    {

    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status410Gone"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status410Gone;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/> to provide metadata for.</param>
    /// <returns>The metadata.</returns>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status410Gone));
    }
}
