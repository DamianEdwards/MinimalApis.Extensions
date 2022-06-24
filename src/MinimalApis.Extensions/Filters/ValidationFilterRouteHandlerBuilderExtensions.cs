#if NET7_0_OR_GREATER
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniValidation;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Validation extension methods for <see cref="RouteHandlerBuilder"/>.
/// </summary>
public static class ValidationFilterRouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a filter that validates route handler parameters using <see cref="MiniValidator"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// The filter will not be added if the route handler does not have any validatable parameters.
    /// </remarks>
    /// <param name="endpoint">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="statusCode">The status code to return on validation failure. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <returns></returns>
    public static TBuilder WithParameterValidation<TBuilder>(this TBuilder endpoint, int statusCode = StatusCodes.Status400BadRequest)
        where TBuilder : IEndpointConventionBuilder
    {
        endpoint.AddRouteHandlerFilter((RouteHandlerContext context, RouteHandlerFilterDelegate next) =>
        {
            var loggerFactory = context.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("MinimalApis.Extensions.Filters.ValidationRouteHandlerFilterFactory");

            var isService = context.ApplicationServices.GetService<IServiceProviderIsService>();
            if (!IsValidatable(context.MethodInfo, isService))
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Route handler method does not contain any validatable parameters, skipping adding validation filter.");
                }
                return next;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Adding metadata to endpoint to document that it can produce a validation problem result.");
            }

            // PERF: Add metadata that details which args to validate, e.g. index, type, etc.?
            //       Rather than just looping over all arguments and calling TryValidate().

            context.EndpointMetadata.Add(new ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), statusCode, "application/problem+json"));

            return (RouteHandlerInvocationContext rhic) =>
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Validation filter running on {argumentCount} argument(s).", rhic.Arguments.Count);
                }

                foreach (var parameter in rhic.Arguments)
                {
                    if (parameter is not null && IsValidatable(parameter.GetType(), isService) && !MiniValidator.TryValidate(parameter, out var errors))
                    {
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Parameter '{parameterName}' failed validation.", errors.Keys.FirstOrDefault());
                        }
                        return new(TypedResults.ValidationProblem(errors));
                    }
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Validation filter completed.");
                }

                return next(rhic);
            };
        });

        return endpoint;
    }

    private static bool IsValidatable(MethodInfo methodInfo, IServiceProviderIsService? isService) =>
        methodInfo.GetParameters().Any(p => IsValidatable(p.ParameterType, isService));

    private static bool IsValidatable(Type type, IServiceProviderIsService? isService) =>
        !IsRequestDelegateFactorySpecialBoundType(type, isService)
        && MiniValidator.RequiresValidation(type);

    private static bool IsRequestDelegateFactorySpecialBoundType(Type type, IServiceProviderIsService? isService) =>
        typeof(HttpContext) == type
        || typeof(HttpRequest) == type
        || typeof(HttpResponse) == type
        || typeof(ClaimsPrincipal) == type
        || typeof(CancellationToken) == type
        || typeof(IFormFileCollection) == type
        || typeof(IFormFile) == type
        || typeof(Stream) == type
        || typeof(PipeReader) == type
        || isService?.IsService(type) == true;
}
#endif
