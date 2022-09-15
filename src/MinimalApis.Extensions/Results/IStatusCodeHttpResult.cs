#if NET6_0
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Defines a contract that represents the result of an HTTP endpoint that contains a <see cref="StatusCode"/>.
/// </summary>
public interface IStatusCodeHttpResult
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    int? StatusCode { get; }
}
#endif
