using System.Reflection;

namespace MinimalApis.Extensions.Metadata;

public interface IProvideEndpointParameterMetadata
{
    static readonly string GetMetadataMethodName = "GetMetadata";
    //static abstract IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services);

    internal static IEnumerable<object> GetMetadataLateBound(ParameterInfo parameter, IServiceProvider services)
    {
        var targetType = parameter.ParameterType;

        if (!targetType.IsAssignableTo(typeof(IProvideEndpointParameterMetadata)))
        {
            throw new ArgumentException($"Target type {targetType.FullName} must implement {nameof(IProvideEndpointParameterMetadata)}", nameof(parameter));
        }

        // TODO: Cache the method lookup and delegate creation? This is only called during first calls to ApiExplorer.
        var method = targetType.GetMethod(GetMetadataMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (method == null)
        {
            return Enumerable.Empty<object>();
        }

        // IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services)
        var getMetadata = method.CreateDelegate<Func<ParameterInfo, IServiceProvider, IEnumerable<object>>>();
        var metadata = getMetadata(parameter, services);

        return metadata ?? Enumerable.Empty<object>();
    }
}
