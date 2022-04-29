﻿#if NET6_0
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that returns a Found (302), Moved Permanently (301), Temporary Redirect (307),
/// or Permanent Redirect (308) response with a Location header to the supplied URL.
/// </summary>
public sealed partial class RedirectHttpResult : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectHttpResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    internal RedirectHttpResult(string url)
         : this(url, permanent: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectHttpResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    internal RedirectHttpResult(string url, bool permanent)
        : this(url, permanent, preserveMethod: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectHttpResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307)
    /// or permanent redirect (308) preserve the initial request method.</param>
    internal RedirectHttpResult(string url, bool permanent, bool preserveMethod)
        : this(url, acceptLocalUrlOnly: false, permanent, preserveMethod)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectHttpResult"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
    /// <param name="preserveMethod">If set to true, make the temporary redirect (307)
    /// or permanent redirect (308) preserve the initial request method.</param>
    /// <param name="acceptLocalUrlOnly">If set to true, only local URLs are accepted
    /// and will throw an exception when the supplied URL is not considered local.</param>
    internal RedirectHttpResult(string url, bool acceptLocalUrlOnly, bool permanent, bool preserveMethod)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("Argument cannot be null or empty", nameof(url));
        }

        Url = url;
        Permanent = permanent;
        PreserveMethod = preserveMethod;
        AcceptLocalUrlOnly = acceptLocalUrlOnly;
    }

    /// <summary>
    /// Gets the value that specifies that the redirect should be permanent if true or temporary if false.
    /// </summary>
    public bool Permanent { get; }

    /// <summary>
    /// Gets an indication that the redirect preserves the initial request method.
    /// </summary>
    public bool PreserveMethod { get; }

    /// <summary>
    /// Gets the URL to redirect to.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets an indication that only local URLs are accepted.
    /// </summary>
    public bool AcceptLocalUrlOnly { get; }

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var isLocalUrl = SharedUrlHelper.IsLocalUrl(Url);

        if (AcceptLocalUrlOnly && !isLocalUrl)
        {
            throw new InvalidOperationException("The supplied URL is not local. A URL with an absolute path is considered local if it does not have a host/authority part. URLs using virtual paths ('~/') are also local.");
        }

        // IsLocalUrl is called to handle URLs starting with '~/'.
        var destinationUrl = isLocalUrl ? SharedUrlHelper.Content(httpContext, contentPath: Url) : Url;

        if (PreserveMethod)
        {
            httpContext.Response.StatusCode = Permanent
                ? StatusCodes.Status308PermanentRedirect
                : StatusCodes.Status307TemporaryRedirect;
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
