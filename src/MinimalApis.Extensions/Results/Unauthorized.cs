#if NET6_0

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Represents an <see cref="IResult"/> that when executed will
/// produce an HTTP response with the No Unauthorized (401) status code.
/// </summary>
public sealed class UnauthorizedHttpResult : IResult, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedHttpResult"/> class.
    /// </summary>
    internal UnauthorizedHttpResult()
    {
    }

    internal static UnauthorizedHttpResult Instance { get; } = new();

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status401Unauthorized"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status401Unauthorized;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return Task.CompletedTask;
    }
}
#endif
