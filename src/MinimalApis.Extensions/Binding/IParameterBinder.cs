namespace MinimalApis.Extensions.Binding;

using System.Reflection;

/// <summary>
/// Represents a type that can bind parameters of route handler delegates.
/// </summary>
/// <typeparam name="TValue">The <see cref="Type"/> this binder can perform parameter binding for.</typeparam>
public interface IParameterBinder<TValue>
{
    /// <summary>
    /// Returns the bound value for the specified <paramref name="parameter"/>.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="parameter">The <see cref="ParameterInfo"/> for the parameter to bind a value for.</param>
    /// <returns>The value to populate the target parameter with.</returns>
    ValueTask<TValue?> BindAsync(HttpContext context, ParameterInfo parameter);
}
