#if NET7_0_OR_GREATER
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinimalApis.Extensions.Filters;
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
    /// The filter will not be added if the route handler does not have any validatable parameters.
    /// </remarks>
    /// <param name="endpoint">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="requireParameterAttribute">If <c>true</c>, only parameters decorated with <see cref="ValidateAttribute"/> will be validated. Default is <c>false</c>.</param>
    /// <param name="statusCode">The status code to return on validation failure. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <returns></returns>
    public static TBuilder WithParameterValidation<TBuilder>(this TBuilder endpoint, bool requireParameterAttribute = false, int statusCode = StatusCodes.Status400BadRequest)
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

            if (!IsValidatable(requireParameterAttribute, methodInfo, isService))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Route handler method '{methodName}' does not contain any validatable parameters, skipping adding validation filter.", methodInfo.Name);
                }
                return;
            }

            // We're going to add the filter so add metadata as well
            builder.Metadata.Add(new ProducesResponseTypeMetadata(typeof(HttpValidationProblemDetails), statusCode, "application/problem+json"));

            builder.FilterFactories.Add((EndpointFilterFactoryContext context, EndpointFilterDelegate next) =>
            {
                // PERF: Compute details during endpoint building about which args to validate, rather than just looping over all
                // arguments and calling TryValidate() on every request.

                var validationDetails = new ValidationFilterDetails();
                foreach (var parameter in methodInfo.GetParameters())
                {
                    if ((requireParameterAttribute && parameter.GetCustomAttribute<ValidateAttribute>() is not null)
                        || IsValidatable(parameter.ParameterType, isService))
                    {
                        validationDetails.Parameters.Add(parameter.ParameterType);
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Parameter '{parameterType} {parameterName}' was marked for validation or has a validatable type and will be validated by the filter.", parameter.ParameterType, parameter.Name);
                        }
                    }
                    else
                    {
                        validationDetails.Parameters.Add(null);
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace("Parameter '{parameterType} {parameterName}' was not marked for validation or does not have validatable type and will not be validated by the filter.", parameter.ParameterType, parameter.Name);
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

                return async (EndpointFilterInvocationContext efic) =>
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Validation filter running on {argumentCount} argument(s).", efic.Arguments.Count);
                    }

                    Debug.Assert(validationDetails is not null);
                    Debug.Assert(validationDetails.Parameters.Count == efic.Arguments.Count);

                    var useParameterValidationDetails = validationDetails is not null && validationDetails.Parameters.Count == efic.Arguments.Count;
                    if (!useParameterValidationDetails)
                    {
                        if (requireParameterAttribute && logger.IsEnabled(LogLevel.Warning))
                        {
                            logger.LogWarning("Validation filter will not run for this request as 'requireParameterAttribute' was specified and computed validation details are invalid: ParameterCount={parameterCount}, ArgumentCount={argumentCount}", validationDetails?.Parameters.Count ?? 0, efic.Arguments.Count);
                        }
                        else if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug("Falling back to validating all arguments as computed validation details are invalid: ParameterCount={parameterCount}, ArgumentCount={argumentCount}", validationDetails?.Parameters.Count ?? 0, efic.Arguments.Count);
                        }
                    }

                    for (var i = 0; i < validationDetails!.Parameters.Count; i++)
                    {
                        Type? parameterType = null;
                        if (useParameterValidationDetails)
                        {
                            parameterType = validationDetails!.Parameters[i];

                            if (parameterType is null)
                            {
                                if (logger.IsEnabled(LogLevel.Trace))
                                {
                                    logger.LogTrace("Argument skipped as the parameter it maps to is declared with a type that is not validatable.");
                                }
                                continue;
                            }
                        }

                        var argument = efic.Arguments[i];

                        if (argument is null)
                        {
                            if (logger.IsEnabled(LogLevel.Trace))
                            {
                                logger.LogTrace("Argument skipped because it is null.");
                            }
                            continue;
                        }

                        if (useParameterValidationDetails)
                        {
                            if (parameterType is not null && !parameterType.IsAssignableFrom(argument.GetType()))
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

                        var (isValid, errors) = await MiniValidator.TryValidateAsync(argument, recurse: true);

                        if (!isValid)
                        {
                            if (logger.IsEnabled(LogLevel.Debug))
                            {
                                logger.LogDebug("Argument for parameter '{parameterName}' failed validation. Will not validate any more parameters.", errors.Keys.FirstOrDefault());
                            }
                            return TypedResults.ValidationProblem(errors);
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

                    return await next(efic);
                };
            });
        });

        return endpoint;
    }

    private static bool IsValidatable(bool requireParameterAttribute, MethodInfo methodInfo, IServiceProviderIsService? isService) =>
        methodInfo.GetParameters().Any(p => requireParameterAttribute
            ? p.GetCustomAttribute<ValidateAttribute>() is not null
            : IsValidatable(p.ParameterType, isService));

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
