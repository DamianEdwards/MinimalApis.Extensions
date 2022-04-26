#if NET6_0
namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Represents an <see cref="IResult"/> that when executed will
/// do nothing.
/// </summary>
public sealed class EmptyHttpResult : IResult
{
    private EmptyHttpResult()
    {
    }

    /// <summary>
    /// Gets an instance of <see cref="EmptyHttpResult"/>.
    /// </summary>
    public static EmptyHttpResult Instance { get; } = new();

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return Task.CompletedTask;
    }
}
#endif
