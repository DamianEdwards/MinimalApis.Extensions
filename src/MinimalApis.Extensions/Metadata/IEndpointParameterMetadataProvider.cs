using System.Reflection;

namespace Microsoft.AspNetCore.Http.Metadata;

#if NET6_0
/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for a
/// parameter on a route handler delegate. The method must be of the form:
/// <code>public static void PopulateMetadata(<see cref="EndpointParameterMetadataContext"/> context)</code>
/// </summary>
public interface IEndpointParameterMetadataProvider
{
    //static abstract void PopulateMetadata(EndpointParameterMetadataContext context);
}
#endif

internal static class EndpointParameterMetadataHelpers
{
    internal static readonly string PopulateMetadataMethodName = "PopulateMetadata";

    public static void PopulateDefaultMetadataForWrapperType<TValue>(EndpointParameterMetadataContext context)
    {
        if (typeof(TValue).IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
        {
            PopulateMetadataLateBound(context);
            return;
        }

        context.EndpointMetadata.Add(new Mvc.ConsumesAttribute(typeof(TValue), "application/json"));
    }

    public static void PopulateMetadataLateBound(EndpointParameterMetadataContext context)
    {
        var targetType = context.Parameter.ParameterType;

        if (!targetType.IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
        {
            throw new ArgumentException($"Target type {targetType.FullName} must implement {nameof(IEndpointParameterMetadataProvider)}", nameof(context));
        }

        // TODO: Cache the method lookup and delegate creation? This is only called during first calls to ApiExplorer.
        var method = targetType.GetMethod(PopulateMetadataMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (method == null)
        {
            return;
        }

        // void PopulateMetadata(EndpointParameterMetadataContext context)
        var populateMetadata = method.CreateDelegate<Action<EndpointParameterMetadataContext>>();
        populateMetadata(context);
    }
}
