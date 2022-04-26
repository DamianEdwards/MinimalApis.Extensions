﻿#if NET6_0
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that returns a Found (302), Moved Permanently (301), Temporary Redirect (307),
/// or Permanent Redirect (308) response with a Location header.
/// Targets a registered route.
/// </summary>
public sealed partial class RedirectToRouteHttpResult : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeValues">The parameters for the route.</param>
    internal RedirectToRouteHttpResult(object? routeValues)
        : this(routeName: null, routeValues: routeValues)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues)
        : this(routeName, routeValues, permanent: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301).
    /// Otherwise a temporary redirect is used (302).</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues,
        bool permanent)
        : this(routeName, routeValues, permanent, fragment: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301).
    /// Otherwise a temporary redirect is used (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307)
    /// or permanent redirect (308) preserve the initial request method.</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        bool preserveMethod)
        : this(routeName, routeValues, permanent, preserveMethod, fragment: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues,
        string? fragment)
        : this(routeName, routeValues, permanent: false, fragment: fragment)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301).
    /// Otherwise a temporary redirect is used (302).</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        string? fragment)
        : this(routeName, routeValues, permanent, preserveMethod: false, fragment: fragment)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToRouteHttpResult"/> with the values
    /// provided.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="routeValues">The parameters for the route.</param>
    /// <param name="permanent">If set to true, makes the redirect permanent (301).
    /// Otherwise a temporary redirect is used (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307)
    /// or permanent redirect (308) preserve the initial request method.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    internal RedirectToRouteHttpResult(
        string? routeName,
        object? routeValues,
        bool permanent,
        bool preserveMethod,
        string? fragment)
    {
        RouteName = routeName;
        RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
        PreserveMethod = preserveMethod;
        Permanent = permanent;
        Fragment = fragment;
    }

    /// <summary>
    /// Gets the name of the route to use for generating the URL.
    /// </summary>
    public string? RouteName { get; }

    /// <summary>
    /// Gets the route data to use for generating the URL.
    /// </summary>
    public RouteValueDictionary? RouteValues { get; }

    /// <summary>
    /// Gets the value that specifies that the redirect should be permanent if true or temporary if false.
    /// </summary>
    public bool Permanent { get; }

    /// <summary>
    /// Gets an indication that the redirect preserves the initial request method.
    /// </summary>
    public bool PreserveMethod { get; }

    /// <summary>
    /// Gets the fragment to add to the URL.
    /// </summary>
    public string? Fragment { get; }

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

        var destinationUrl = linkGenerator.GetUriByRouteValues(
            httpContext,
            RouteName,
            RouteValues,
            fragment: Fragment == null ? FragmentString.Empty : new FragmentString("#" + Fragment));

        if (string.IsNullOrEmpty(destinationUrl))
        {
            throw new InvalidOperationException("No route matches the supplied values.");
        }

        if (PreserveMethod)
        {
            httpContext.Response.StatusCode = Permanent ?
                StatusCodes.Status308PermanentRedirect : StatusCodes.Status307TemporaryRedirect;
            httpContext.Response.Headers.Location = destinationUrl;
        }
        else
        {
            httpContext.Response.Redirect(destinationUrl, Permanent);
        }

        return Task.CompletedTask;
    }
}
#endif
