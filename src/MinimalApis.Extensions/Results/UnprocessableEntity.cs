#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Unprocessable Entity (422) status code.
/// </summary>
public sealed class UnprocessableEntity : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntity"/> class with the values
    /// provided.
    /// </summary>
    internal UnprocessableEntity()
    {
    }

    internal static UnprocessableEntity Instance { get; } = new();

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status422UnprocessableEntity"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status422UnprocessableEntity;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

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

        metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status422UnprocessableEntity));
    }
}
#endif
