#if NET7_0_OR_GREATER
namespace MinimalApis.Extensions.Filters;

/// <summary>
/// Indicates a route handler delegate parameter should be validated before the route handler is invoked.
/// </summary>
/// <remarks>
/// Should be used in conjunction with <see cref="ValidationFilterRouteHandlerBuilderExtensions.WithParameterValidation{TBuilder}(TBuilder, bool, int)"/>
/// to enable route handler delegate parameter validation.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public class ValidateAttribute : Attribute
{

}
#endif
