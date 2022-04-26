using System.Reflection;
using MinimalApis.Extensions.Infrastructure;

namespace Microsoft.AspNetCore.Http.Metadata;

#if NET6_0
/// <summary>
/// Marker interface that indicates a type provides a static method that returns <see cref="Endpoint"/> metadata for the
/// returned value from a given <see cref="Endpoint"/> route handler delegate. The method must be of the form:
/// <code>public static <see cref="IEnumerable{Object}"/> GetMetadata(<see cref="Endpoint"/> endpoint, <see cref="IServiceProvider"/> services)</code>
/// </summary>
public interface IEndpointMetadataProvider
{
    //static abstract void PopulateMetadata(EndpointMetadataContext context);
}
#endif

internal static class EndpointMetadataHelpers
{
    private static readonly string PopulateMetadataMethodName = "PopulateMetadata";

    public static void PopulateMetadataLateBound(Type type, EndpointMetadataContext context)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(context);

        var routeHandlerMethod = context.EndpointMetadata.OfType<MethodInfo>().SingleOrDefault();
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

        // void PopulateMetadata(EndpointMetadataContext context)
        var populateMetadata = method.CreateDelegate<Action<EndpointMetadataContext>>();
        populateMetadata(context);
    }
}
