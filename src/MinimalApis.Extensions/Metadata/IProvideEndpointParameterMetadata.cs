using System.Reflection;

namespace MinimalApis.Extensions.Metadata;

/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for a
/// parameter on a route handler delegate. The method must be of the form:
/// <code>public static <see cref="IEnumerable{Object}"/> GetMetadata(<see cref="ParameterInfo"/> parameter, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IProvideEndpointParameterMetadata
{
    internal static readonly string GetMetadataMethodName = "GetMetadata";
    //static abstract IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services);

    internal static IEnumerable<object> GetDefaultMetadataForWrapperType<TValue>(ParameterInfo parameter, IServiceProvider services)
    {
        if (typeof(TValue).IsAssignableTo(typeof(IProvideEndpointParameterMetadata)))
        {
            return GetMetadataLateBound(parameter, services);
        }

        return new[] { new Mvc.ConsumesAttribute(typeof(TValue), "application/json") };
    }

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
