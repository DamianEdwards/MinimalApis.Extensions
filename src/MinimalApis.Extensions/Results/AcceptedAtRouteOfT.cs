#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with status code Accepted (202) and Location header.
/// Targets a registered route.
/// </summary>
/// <typeparam name="TValue">The type of object that will be JSON serialized to the response body.</typeparam>
public sealed class AcceptedAtRoute<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptedAtRoute"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="routeValues">The route data to use for generating the URL.</param>
    /// <param name="value">The value to format in the entity body.</param>
    internal AcceptedAtRoute(object? routeValues, TValue? value)
        : this(routeName: null, routeValues: routeValues, value: value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptedAtRoute"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route to use for generating the URL.</param>
    /// <param name="routeValues">The route data to use for generating the URL.</param>
    /// <param name="value">The value to format in the entity body.</param>
    internal AcceptedAtRoute(string? routeName, object? routeValues, TValue? value)
    {
        Value = value;
        RouteName = routeName;
        RouteValues = new RouteValueDictionary(routeValues);
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the name of the route to use for generating the URL.
    /// </summary>
    public string? RouteName { get; }

    /// <summary>
    /// Gets the route data to use for generating the URL.
    /// </summary>
    public RouteValueDictionary RouteValues { get; }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status202Accepted"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status202Accepted;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
        var url = linkGenerator.GetUriByAddress(
            httpContext,
            RouteName,
            RouteValues,
            fragment: FragmentString.Empty);

        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("No route matches the supplied values.");
        }

        httpContext.Response.Headers.Location = url;
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

        metadata.Add(new ProducesResponseTypeMetadata(typeof(TValue), StatusCodes.Status202Accepted, "application/json"));
    }
}
#endif
