#if NET6_0
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Defines a contract that represents the result of an HTTP endpoint that contains an object <see cref="Value"/>.
/// </summary>
public interface IValueHttpResult
{
    /// <summary>
    /// Gets the object result.
    /// </summary>
    object? Value { get; }
}

/// <summary>
/// Defines a contract that represents the result of an HTTP endpoint that contains a <see cref="Value"/>.
/// </summary>
public interface IValueHttpResult<out TValue>
{
    /// <summary>
    /// Gets the object result.
    /// </summary>
    TValue? Value { get; }
}
#endif
