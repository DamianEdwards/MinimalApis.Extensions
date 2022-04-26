#if NET6_0
using System.Reflection;
using MinimalApis.Extensions.Infrastructure;

namespace Microsoft.AspNetCore.Http.Metadata;

/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for the
/// returned value from a given <see cref="Endpoint"/> route handler delegate. The method must be of the form:
/// <code>public static <see cref="IEnumerable{Object}"/> GetMetadata(<see cref="Endpoint"/> endpoint, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IEndpointMetadataProvider
{
    private static readonly string PopulateMetadataMethodName = "PopulateMetadata";
    //static abstract IEnumerable<object> PopulateMetadata(Endpoint endpoint, IServiceProvider services);

    internal static void PopulateMetadataLateBound(Type type, IList<object> metadata, IServiceProvider services)
    {
        var routeHandlerMethod = metadata.OfType<MethodInfo>().SingleOrDefault();
        if (routeHandlerMethod is null || type is null)
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

        // IEnumerable<object> PopulateMetadata(Endpoint endpoint, IServiceProvider services)
        var populateMetadata = method.CreateDelegate<Action<EndpointMetadataContext>>();
        var context = new EndpointMetadataContext(routeHandlerMethod, metadata, services);
        populateMetadata(context);
    }
}
#endif
