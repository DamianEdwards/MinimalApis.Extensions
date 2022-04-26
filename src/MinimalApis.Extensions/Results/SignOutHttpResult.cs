﻿#if NET6_0
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution invokes <see cref="M:HttpContext.SignOutAsync"/>.
/// </summary>
public sealed partial class SignOutHttpResult : IResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the default sign out scheme.
    /// </summary>
    internal SignOutHttpResult()
        : this(Array.Empty<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the default sign out scheme.
    /// specified authentication scheme and <paramref name="properties"/>.
    /// </summary>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    internal SignOutHttpResult(AuthenticationProperties properties)
        : this(Array.Empty<string>(), properties)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the
    /// specified authentication scheme.
    /// </summary>
    /// <param name="authenticationScheme">The authentication scheme to use when signing out the user.</param>
    internal SignOutHttpResult(string authenticationScheme)
        : this(new[] { authenticationScheme })
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the
    /// specified authentication schemes.
    /// </summary>
    /// <param name="authenticationSchemes">The authentication schemes to use when signing out the user.</param>
    internal SignOutHttpResult(IList<string> authenticationSchemes)
        : this(authenticationSchemes, properties: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the
    /// specified authentication scheme and <paramref name="properties"/>.
    /// </summary>
    /// <param name="authenticationScheme">The authentication schemes to use when signing out the user.</param>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    internal SignOutHttpResult(string authenticationScheme, AuthenticationProperties? properties)
        : this(new[] { authenticationScheme }, properties)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutHttpResult"/> with the
    /// specified authentication schemes and <paramref name="properties"/>.
    /// </summary>
    /// <param name="authenticationSchemes">The authentication scheme to use when signing out the user.</param>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    internal SignOutHttpResult(IList<string> authenticationSchemes, AuthenticationProperties? properties)
    {
        ArgumentNullException.ThrowIfNull(authenticationSchemes);

        AuthenticationSchemes = authenticationSchemes.ToList();
        Properties = properties;
    }

    /// <summary>
    /// Gets the authentication schemes that are challenged.
    /// </summary>
    public IReadOnlyList<string> AuthenticationSchemes { get; internal init; }

    /// <summary>
    /// Gets the <see cref="AuthenticationProperties"/> used to perform the sign-out operation.
    /// </summary>
    public AuthenticationProperties? Properties { get; internal init; }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (AuthenticationSchemes.Count == 0)
        {
            await httpContext.SignOutAsync(Properties);
        }
        else
        {
            for (var i = 0; i < AuthenticationSchemes.Count; i++)
            {
                await httpContext.SignOutAsync(AuthenticationSchemes[i], Properties);
            }
        }
    }
}
#endif
