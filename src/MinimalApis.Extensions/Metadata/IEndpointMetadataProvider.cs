using System.Reflection;
using Microsoft.AspNetCore.Builder;
using MinimalApis.Extensions.Infrastructure;

namespace Microsoft.AspNetCore.Http.Metadata;

#if NET6_0
/// <summary>
/// Marker interface that indicates a type provides a static method that populates <see cref="Endpoint"/> metadata for the
/// returned value from a given <see cref="Endpoint"/> route handler delegate. The method must be of the form:
/// <code>public static void PopulateMetadata(<see cref="MethodInfo"/> method, <see cref="IList{Object}"/> metadata, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IEndpointMetadataProvider
{
    //static abstract void PopulateMetadata(EndpointBuilder builder);
}
#endif

internal static class EndpointMetadataHelpers
{
    private static readonly string PopulateMetadataMethodName = "PopulateMetadata";

#if NET7_0_OR_GREATER
    public static void PopulateMetadata<T>(MethodInfo method, EndpointBuilder builder)
        where T : IEndpointMetadataProvider
    {
        T.PopulateMetadata(method, builder);
    }
#endif

    public static void PopulateMetadataLateBound(Type type, IList<object> metadata, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(services);

        var routeHandlerMethod = metadata.OfType<MethodInfo>().SingleOrDefault();
        if (routeHandlerMethod is null)
        {
            return;
        }

        var targetType = type ?? AwaitableInfo.GetMethodReturnType(routeHandlerMethod);
        if (!targetType.IsAssignableTo(typeof(IEndpointMetadataProvider)))
        {
            throw new ArgumentException($"Target type {targetType.FullName} must implement {nameof(IEndpointMetadataProvider)}", nameof(type));
        }

        // TODO: Cache the method lookup and delegate creation? This is only called during first calls to ApiExplorer.
        var method = targetType.GetMethod(PopulateMetadataMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (method == null)
        {
            return;
        }

        // void PopulateMetadata(MethodInfo method, IList<object> metadata, IServiceProvider services)
        var populateMetadata = method.CreateDelegate<Action<MethodInfo, IList<object>, IServiceProvider>>();
        populateMetadata(method, metadata, services);
    }
}
