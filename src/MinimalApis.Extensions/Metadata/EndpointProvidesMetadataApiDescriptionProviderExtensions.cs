#if NET6_0
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalApis.Extensions.Metadata;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding an <see cref="IApiDescriptionProvider"/> so that <see cref="Endpoint"/> metadata
/// provided by <see cref="IEndpointParameterMetadataProvider"/> and <see cref="IEndpointMetadataProvider"/>
/// appear in ApiExplorer and thus OpenAPI/Swagger documents and UI.
/// </summary>
public static class EndpointProvidesMetadataApiDescriptionProviderExtensions
{
    /// <summary>
    /// Configures ApiExplorer using <see cref="Endpoint"/> metadata, <see cref="IEndpointParameterMetadataProvider"/>
    /// and <see cref="IEndpointMetadataProvider"/>. 
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEndpointsProvidesMetadataApiExplorer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddEndpointsApiExplorer();

        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, EndpointProvidesMetadataApiDescriptionProvider>());

        return services;
    }
}
#endif
