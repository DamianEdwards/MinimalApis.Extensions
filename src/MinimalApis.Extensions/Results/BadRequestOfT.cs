#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Bad Request (400) status code.
/// </summary>
/// <typeparam name="TValue">The type of error object that will be JSON serialized to the response body.</typeparam>
public sealed class BadRequest<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequest"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="error">The error content to format in the entity body.</param>
    internal BadRequest(TValue? error)
    {
        Value = error;
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status400BadRequest"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status400BadRequest;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return httpContext.Response.WriteAsJsonAsync(Value);
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

        metadata.Add(new ProducesResponseTypeMetadata(typeof(TValue), StatusCodes.Status400BadRequest, "application/json"));
    }
}
#endif
