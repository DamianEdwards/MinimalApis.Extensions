#if NET7_0_OR_GREATER
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniValidation;
using static System.Net.Mime.MediaTypeNames;

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
        endpoint.Add(builder =>
        {
            var loggerFactory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("MinimalApis.Extensions.Filters.ValidationRouteHandlerFilter");

            var methodInfo = builder.Metadata.OfType<MethodInfo>().FirstOrDefault();

            if (methodInfo is null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Cannot add parameter validation filter to endpoint with no MethodInfo metadata.");
                }
                return;
            }

            var isService = builder.ApplicationServices.GetService<IServiceProviderIsService>();

            if (!IsValidatable(methodInfo, isService))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Route handler method '{methodName}' does not contain any validatable parameters, skipping adding validation filter.", methodInfo.Name);
                }
                return;
            }

            builder.FilterFactories.Add((EndpointFilterFactoryContext context, EndpointFilterDelegate next) =>
            {
                // PERF: Compute details during endpoint building about which args to validate, rather than just looping over all
                // arguments and calling TryValidate() on every request.

                var validationDetails = new ValidationFilterDetails();
                foreach (var parameter in methodInfo.GetParameters())
                {
                    if (IsValidatable(parameter.ParameterType, isService))
                    {
                        validationDetails.Parameters.Add(parameter.ParameterType);
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Parameter '{parameterType} {parameterName}' has a validatable type and will be validated by the filter.", parameter.ParameterType, parameter.Name);
                        }
                    }
                    else
                    {
                        validationDetails.Parameters.Add(null);
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Parameter '{parameterType} {parameterName}' does not have validatable type and will not be validated by the filter.", parameter.ParameterType, parameter.Name);
                        }
                    }
                }

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Validation filter will be added as route handler method '{methodName}' has validatable parameters.", context.MethodInfo.Name);
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Updating endpoint metadata to indicate it can produce a validation problem result ({StatusCode}).", statusCode);
                    }
                }

                builder.Metadata.Add(new ProducesResponseTypeMetadata(typeof(HttpValidationProblemDetails), statusCode, "application/problem+json"));

                return (EndpointFilterInvocationContext efic) =>
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Validation filter running on {argumentCount} argument(s).", efic.Arguments.Count);
                    }

                    Debug.Assert(validationDetails is not null);
                    Debug.Assert(validationDetails.Parameters.Count == efic.Arguments.Count);

                    var useParameterValidationDetails = validationDetails is not null && validationDetails.Parameters.Count == efic.Arguments.Count;
                    if (!useParameterValidationDetails && logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Falling back to validating all arguments as computed validation details are invalid: ParameterCount={parameterCount}, ArgumentCount={argumentCount}", validationDetails?.Parameters.Count ?? 0, efic.Arguments.Count);
                    }

                    for (var i = 0; i < efic.Arguments.Count; i++)
                    {
                        var argument = efic.Arguments[i];

                        if (argument is null)
                        {
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Argument skipped as it is null.");
                            }
                            continue;
                        }

                        if (useParameterValidationDetails)
                        {
                            var parameterType = validationDetails!.Parameters[i];
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

                    return next(efic);
                };
            });
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

internal class ValidationFilterDetails
{
    public List<Type?> Parameters { get; } = new();
}
#endif
