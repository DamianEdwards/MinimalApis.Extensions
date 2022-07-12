#if NET7_0_OR_GREATER
using System.Diagnostics;
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
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Route handler method '{methodName}' does not contain any validatable parameters, skipping adding validation filter.", context.MethodInfo.Name);
                }
                return next;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Validation filter will be added as route handler method '{methodName}' has validatable parameters.", context.MethodInfo.Name);
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Updating endpoint metadata to indicate it can produce a validation problem result (400 Bad Request).", context.MethodInfo.Name);
                }
            }
            context.EndpointMetadata.Add(new ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), statusCode, "application/problem+json"));

            // PERF: Add metadata that details which args to validate, rather than just looping over all arguments and calling TryValidate().
            var validationMetadata = new ValidationFilterMetadata();
            foreach (var parameter in context.MethodInfo.GetParameters())
            {
                if (IsValidatable(parameter.ParameterType, isService))
                {
                    validationMetadata.Parameters.Add(parameter.ParameterType);
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Parameter '{parameterType} {parameterName}' has a validatable type and will be validated by the filter.", parameter.ParameterType, parameter.Name);
                    }
                }
                else
                {
                    validationMetadata.Parameters.Add(null);
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Parameter '{parameterType} {parameterName}' does not have validatable type and will not be validated by the filter.", parameter.ParameterType, parameter.Name);
                    }
                }
            }
            context.EndpointMetadata.Add(validationMetadata);

            return (RouteHandlerInvocationContext rhic) =>
            {
                var endpoint = rhic.HttpContext.GetEndpoint();
                if (endpoint is null) return next(rhic);

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Validation filter running on {argumentCount} argument(s).", rhic.Arguments.Count);
                }

                var validationMetadata = endpoint.Metadata.GetMetadata<ValidationFilterMetadata>();

                Debug.Assert(validationMetadata is not null);
                Debug.Assert(validationMetadata.Parameters.Count == rhic.Arguments.Count);

                var useParameterValidationMetadata = validationMetadata is not null && validationMetadata.Parameters.Count == rhic.Arguments.Count;
                if (!useParameterValidationMetadata && logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Falling back to validating all arguments as parameter metadata is invalid: ParameterCount={parameterCount}, ArgumentCount={argumentCount}", validationMetadata?.Parameters.Count ?? 0, rhic.Arguments.Count);
                }

                for (var i = 0; i < rhic.Arguments.Count; i++)
                {
                    var argument = rhic.Arguments[i];

                    if (argument is null)
                    {
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Argument skipped as it is null.");
                        }
                        continue;
                    }

                    if (useParameterValidationMetadata)
                    {
                        var parameterType = validationMetadata!.Parameters[i];
                        if (parameterType is null)
                        {
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Argument with type '{argumentType}' skipped as the parameter it maps to is declared with a type that is not validatable.", argument.GetType());
                            }
                            continue;
                        }
                        else if (!parameterType.IsAssignableFrom(argument.GetType()))
                        {
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Argument with type '{argumentType}' skipped as it doesn't match the parameter type {parameterType}.", argument.GetType(), parameterType);
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (!IsValidatable(argument.GetType()))
                        {
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Argument with type '{argumentType}' skipped as the type is not validatable.", argument.GetType());
                            }
                            continue;
                        }
                    }

                    if (!MiniValidator.TryValidate(argument, out var errors))
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug("Argument for parameter '{parameterName}' failed validation. Will not validate any more parameters.", errors.Keys.FirstOrDefault());
                        }
                        return new(TypedResults.ValidationProblem(errors));
                    }

                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Argument with type '{argumentType}' was validated and no errors were found.", argument.GetType());
                    }
                }

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Validation is complete.");
                }

                return next(rhic);
            };
        });

        return endpoint;
    }

    private static bool IsValidatable(MethodInfo methodInfo, IServiceProviderIsService? isService) =>
        methodInfo.GetParameters().Any(p => IsValidatable(p.ParameterType, isService));

    private static bool IsValidatable(Type type, IServiceProviderIsService? isService = null) =>
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

internal class ValidationFilterMetadata
{
    public List<Type?> Parameters { get; } = new();
}
#endif
