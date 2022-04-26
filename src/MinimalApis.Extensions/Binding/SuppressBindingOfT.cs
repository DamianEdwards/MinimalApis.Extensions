using System.Reflection;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// Suprresses the default binding logic of RequestDelegateFactory when accepted as a parameter to a route handler.
/// </summary>
/// <typeparam name="TValue">The <see cref="Type"/> of the parameter to suppress binding for.</typeparam>
public class SuppressBinding<TValue> : IEndpointParameterMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuppressBinding{TValue}"/> class.
    /// </summary>
    public SuppressBinding()
    {

    }

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="SuppressBinding{TValue}"/>.</returns>
    public static ValueTask<SuppressBinding<TValue?>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        return ValueTask.FromResult(new SuppressBinding<TValue?>());
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services) =>
        IEndpointParameterMetadataProvider.GetDefaultMetadataForWrapperType<TValue>(parameter, services);
}
