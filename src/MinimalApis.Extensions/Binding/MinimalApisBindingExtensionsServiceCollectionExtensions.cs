using MinimalApis.Extensions.Binding;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for adding binding-related Minimal APIs extensions to the application <see cref="IServiceCollection"/>.
/// </summary>
public static class MinimalApisBindingExtensionsServiceCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="Type"/> to use as a parameter binder for route handler delegates.
    /// </summary>
    /// <typeparam name="TBinder">The <see cref="Type"/> to register as a parameter binder.</typeparam>
    /// <typeparam name="TModel">The parameter type to register the binder for.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddParameterBinder<TBinder, TModel>(this IServiceCollection services)
        where TBinder : class, IParameterBinder<TModel> =>
            services.AddSingleton<IParameterBinder<TModel>, TBinder>();
}
