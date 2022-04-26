#if NET6_0
using System.Reflection;

namespace Microsoft.AspNetCore.Http.Metadata;

/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for a
/// parameter on a route handler delegate. The method must be of the form:
/// <code>public static <see cref="IEnumerable{Object}"/> GetMetadata(<see cref="ParameterInfo"/> parameter, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IEndpointParameterMetadataProvider
{
    internal static readonly string PopulateMetadataMethodName = "PopulateMetadata";
    //static abstract IEnumerable<object> PopulateMetadataMetadata(ParameterInfo parameter, IServiceProvider services);

    internal static IEnumerable<object> GetDefaultMetadataForWrapperType<TValue>(ParameterInfo parameter, IServiceProvider services)
    {
        if (typeof(TValue).IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
        {
            var metadata = new List<object>();
            PopulateMetadataLateBound(parameter, metadata, services);
            return metadata;
        }

        return new[] { new Mvc.ConsumesAttribute(typeof(TValue), "application/json") };
    }

    internal static void PopulateMetadataLateBound(ParameterInfo parameter, IList<object> metadata, IServiceProvider services)
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

        // IEnumerable<object> PopulateMetadata(ParameterInfo parameter, IServiceProvider services)
        var populateMetadata = method.CreateDelegate<Action<EndpointParameterMetadataContext>>();
        var context = new EndpointParameterMetadataContext(parameter, metadata, services);
        populateMetadata(context);
    }
}
#endif
