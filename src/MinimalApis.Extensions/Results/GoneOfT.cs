using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with a Gone (410) status code.
/// </summary>
/// <typeparam name="TValue">The type of value object that will be JSON serialized to the response body.</typeparam>
public sealed class Gone<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gone{TValue}"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    internal Gone(TValue? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status410Gone"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status410Gone;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;


    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return httpContext.Response.WriteAsJsonAsync(Value);
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
        metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status410Gone));
    }
}
