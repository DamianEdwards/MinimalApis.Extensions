using System.Reflection;
using MinimalApis.Extensions.Infrastructure;

namespace MinimalApis.Extensions.Metadata;

/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for the
/// returned value from a given <see cref="Endpoint"/> route handler delegate. The method must be of the form:
/// <code>public static <see cref="IEnumerable{object}"/> GetMetadata(<see cref="Endpoint"/> endpoint, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IProvideEndpointResponseMetadata
{
    static readonly string GetMetadataMethodName = "GetMetadata";
    //static abstract IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services);

    internal static IEnumerable<object> GetMetadataLateBound(Type? type, Endpoint endpoint, IServiceProvider services)
    {
        var routeHandlerMethod = endpoint.Metadata.FirstOrDefault(m => m.GetType().IsAssignableTo(typeof(MethodInfo))) as MethodInfo;
        if (routeHandlerMethod is null && type is null)
        {
            return Enumerable.Empty<object>();
        }

        var targetType = type ?? AwaitableInfo.GetMethodReturnType(routeHandlerMethod);
        if (!targetType.IsAssignableTo(typeof(IProvideEndpointResponseMetadata)))
        {
            throw new ArgumentException($"Target type {targetType.FullName} must implement {nameof(IProvideEndpointResponseMetadata)}", type != null ? nameof(type) : nameof(endpoint));
        }

        // TODO: Cache the method lookup and delegate creation? This is only called during first calls to ApiExplorer.
        var method = targetType.GetMethod(GetMetadataMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (method == null)
        {
            return Enumerable.Empty<object>();
        }

        // IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
        var getMetadata = method.CreateDelegate<Func<Endpoint, IServiceProvider, IEnumerable<object>>>();
        var metadata = getMetadata(endpoint, services);

        return metadata ?? Enumerable.Empty<object>();
    }
}
