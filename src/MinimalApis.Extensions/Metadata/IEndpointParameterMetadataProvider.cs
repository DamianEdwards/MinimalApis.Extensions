using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Http.Metadata;

#if NET6_0
/// <summary>
/// Marker interface that indicates a type provides a static method that populates <see cref="Endpoint"/> metadata for a
/// parameter on a route handler delegate. The method must be of the form:
/// <code>public static void PopulateMetadata(<see cref="ParameterInfo"/> parameter, <see cref="EndpointBuilder"/> builder)</code>
/// </summary>
public interface IEndpointParameterMetadataProvider
{
    //static abstract void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder);
}
#endif

internal static class EndpointParameterMetadataHelpers
{
    internal static readonly string PopulateMetadataMethodName = "PopulateMetadata";
    internal static readonly string[] DefaultAcceptsContentTypes = new[] { "application/json" };

    public static void PopulateDefaultMetadataForWrapperType<TValue>(ParameterInfo parameter, IList<object> metadata, IServiceProvider services)
    {
        if (typeof(TValue).IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
        {
            PopulateMetadataLateBound(parameter, metadata, services);
            return;
        }

        metadata.Add(new MinimalApis.Extensions.Metadata.AcceptsMetadata(typeof(TValue), false, DefaultAcceptsContentTypes));
    }

#if NET7_0_OR_GREATER
    public static void PopulateMetadata<T>(ParameterInfo parameter, EndpointBuilder builder)
        where T : IEndpointParameterMetadataProvider
    {
        T.PopulateMetadata(parameter, builder);
    }
#endif

    public static void PopulateMetadataLateBound(ParameterInfo parameter, IList<object> metadata, IServiceProvider services)
    {
        var targetType = parameter.ParameterType;

        if (!targetType.IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
        {
            throw new ArgumentException($"Target type {targetType.FullName} must implement {nameof(IEndpointParameterMetadataProvider)}", nameof(parameter));
        }

        // TODO: Cache the method lookup and delegate creation? This is only called during first calls to ApiExplorer.
        var method = targetType.GetMethod(PopulateMetadataMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (method == null)
        {
            return;
        }

        // void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
        var populateMetadata = method.CreateDelegate<Action<ParameterInfo, IList<object>, IServiceProvider>>();
        populateMetadata(parameter, metadata, services);
    }
}
