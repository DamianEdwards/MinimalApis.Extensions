#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Unprocessable Entity (422) status code.
/// </summary>
/// <typeparam name="TValue">The type of object that will be JSON serialized to the response body.</typeparam>
public sealed class UnprocessableEntity<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntity"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    internal UnprocessableEntity(TValue? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    object? IValueHttpResult.Value => Value;

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

        return httpContext.Response.WriteAsJsonAsync(Value);
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

        metadata.Add(new ProducesResponseTypeMetadata(typeof(TValue), StatusCodes.Status422UnprocessableEntity, "application/json"));
    }
}
#endif
