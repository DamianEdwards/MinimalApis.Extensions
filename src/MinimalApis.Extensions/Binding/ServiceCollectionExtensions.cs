using MinimalApis.Extensions.Binding;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a type to use as a parameter binder for route handler delegates.
    /// </summary>
    /// <typeparam name="TBinder">The type to register as a parameter binder.</typeparam>
    /// <typeparam name="TModel">The parameter type to register the binder for.</typeparam>
    /// <param name="services">The IServiceCollection.</param>
    /// <returns>The IServiceCollection.</returns>
    public static IServiceCollection AddParameterBinder<TBinder, TModel>(this IServiceCollection services)
        where TBinder : class, IParameterBinder<TModel> =>
            services.AddSingleton<IParameterBinder<TModel>, TBinder>();
}
