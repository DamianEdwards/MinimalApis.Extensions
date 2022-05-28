#if NET7_0_OR_GREATER
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="statusCode">The status code to return on validation failure. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <returns></returns>
    public static RouteHandlerBuilder WithParameterValidation(this RouteHandlerBuilder builder, int statusCode = StatusCodes.Status400BadRequest)
    {
        builder.Add(eb =>
        {
            var methodInfo = eb.Metadata.OfType<MethodInfo>().SingleOrDefault();
            if (methodInfo is not null && IsValidatable(methodInfo))
            {
                eb.Metadata.Add(new ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), statusCode, "application/problem+json"));
            }
        });
        builder.AddFilter((RouteHandlerContext context, RouteHandlerFilterDelegate next) =>
        {
            // TODO: Blocked by https://github.com/dotnet/aspnetcore/issues/41900
            //var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            //var logger = loggerFactory.Create("MinimalApis.Extensions.Filters.ValidationRouteHandlerFilterFactory");

            if (!IsValidatable(context.MethodInfo))
            {
                return next;
            }

            return rhic =>
            {
                foreach (var parameter in rhic.Parameters)
                {
                    if (parameter is not null && !MiniValidator.TryValidate(parameter, out var errors))
                    {
                        return new(Results.ValidationProblem(errors));
                    }
                }

                return next(rhic);
            };
        });

        return builder;
    }

    private static bool IsValidatable(MethodInfo methodInfo) => methodInfo.GetParameters().Any(IsValidatable);

    private static bool IsValidatable(ParameterInfo parameter) => MiniValidator.RequiresValidation(parameter.ParameterType);
}
#endif
