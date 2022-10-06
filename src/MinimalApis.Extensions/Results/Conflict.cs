#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Conflict (409) status code.
/// </summary>
public sealed class Conflict : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Conflict"/> class with the values
    /// provided.
    /// </summary>
    internal Conflict()
    {
    }

    internal static readonly Conflict Instance = new();

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status409Conflict"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status409Conflict;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="metadata"></param>
    /// <param name="services"></param>
    public static void PopulateMetadata(MethodInfo methodInfo, IList<object> metadata, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(services);

        metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status409Conflict));
    }
}
#endif
