using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalApis.Extensions.Metadata;

namespace Microsoft.Extensions.DependencyInjection;

public static class EndpointProvidesMetadataApiDescriptionProviderExtensions
{
    public static IServiceCollection AddEndpointsProvidesMetadataApiExplorer(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApiDescriptionProvider, EndpointProvidesMetadataApiDescriptionProvider>());

        return services;
    }
}
