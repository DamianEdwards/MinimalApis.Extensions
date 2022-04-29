﻿#if NET6_0
using Microsoft.AspNetCore.Http.Metadata;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with status code Accepted (202) and Location header.
/// Targets a registered route.
/// </summary>
public sealed class Accepted : IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Accepted"/> class.
    /// </summary>
    /// <param name="location">The location at which the status of requested content can be monitored.</param>
    internal Accepted(string? location = null)
    {
        Location = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Accepted"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="locationUri">The location at which the status of requested content can be monitored.</param>
    internal Accepted(Uri locationUri)
    {
        ArgumentNullException.ThrowIfNull(locationUri);

        if (locationUri.IsAbsoluteUri)
        {
            Location = locationUri.AbsoluteUri;
        }
        else
        {
            Location = locationUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
        }
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status202Accepted"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status202Accepted;

    /// <summary>
    /// Gets the location at which the status of the requested content can be monitored.
    /// </summary>
    public string? Location { get; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (!string.IsNullOrEmpty(Location))
        {
            httpContext.Response.Headers.Location = Location;
        }

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/>.
    /// </summary>
    /// <param name="context">The <see cref="EndpointMetadataContext"/>.</param>
    public static void PopulateMetadata(EndpointMetadataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.EndpointMetadata.Add(new Mvc.ProducesResponseTypeAttribute(StatusCodes.Status202Accepted));
    }
}
#endif
