#if NET6_0
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Defines a contract that represents the result of an HTTP endpoint
/// that contains a <see cref="ContentType"/>.
/// </summary>
public interface IContentTypeHttpResult
{
    /// <summary>
    /// Gets the Content-Type header for the response.
    /// </summary>
    string? ContentType { get; }
}
#endif
