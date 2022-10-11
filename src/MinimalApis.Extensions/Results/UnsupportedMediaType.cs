using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status415UnsupportedMediaType"/> response.
/// </summary>
public sealed class UnsupportedMediaType : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    private const int ResponseStatusCode = StatusCodes.Status415UnsupportedMediaType;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedMediaType"/> class.
    /// </summary>
    internal UnsupportedMediaType()
    {

    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status415UnsupportedMediaType"/>
    /// </summary>
    public int StatusCode => ResponseStatusCode;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
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
        metadata.Add(new ProducesResponseTypeMetadata(ResponseStatusCode));
    }
}
