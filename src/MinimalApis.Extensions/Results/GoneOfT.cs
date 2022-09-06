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

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(typeof(TValue), StatusCodes.Status410Gone, "application/json"));
    }
}
