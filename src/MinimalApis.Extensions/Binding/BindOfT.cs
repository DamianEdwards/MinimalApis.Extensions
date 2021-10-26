using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mvc = Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// Represents a type that will use a registered <see cref="IParameterBinder{TValue}"/> to populate a
/// parameter of type <typeparamref name="TValue"/> of a route handler delegate.
/// </summary>
/// <typeparam name="TValue">The parameter type.</typeparam>
public struct Bind<TValue> : IProvideEndpointParameterMetadata
{
    private readonly TValue? _value;

    public Bind(TValue? modelValue)
    {
        _value = modelValue;
    }

    public TValue? Value => _value;

    private static Bind<TValue?> WrapResult(TValue? value) => new(value);

    public static implicit operator TValue?(Bind<TValue> model) => model.Value;

    // RequestDelegateFactory discovers this method via reflection and code-gens calls to it to populate
    // parameter values for declared route handler delegates.
    public static async ValueTask<Bind<TValue?>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Bind<TValue>>>();

        var binder = LookupBinder(context.RequestServices, logger);

        if (binder != null)
        {
            var value = await binder.BindAsync(context, parameter);
            return WrapResult(value);
        }

        var (defaultBinderResult, statusCode) = await DefaultBinder<TValue>.GetValueAsync(context, parameter);

        if (statusCode != StatusCodes.Status200OK)
        {
            // Binding issue
            throw new BadHttpRequestException("Bad request", statusCode);
        }

        return WrapResult(defaultBinderResult);
    }

    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Bind<TValue>>>();
        var binder = LookupBinder(services, logger);

        return binder switch
        {
            IProvideEndpointResponseMetadata => IProvideEndpointParameterMetadata.GetMetadataLateBound(parameter, services),
            _ => new[] { new Mvc.ConsumesAttribute(typeof(TValue), "application/json") }
        };
    }

    private const string Template_ResolvedFromDI = nameof(IParameterBinder<object>) + "<{ParameterBinderTargetTypeName}> resolved from DI container.";
    private const string Template_NotResolvedFromDI = nameof(IParameterBinder<object>) + "<{ParameterBinderTargetTypeName}> could not be resovled from DI container, using default binder.";

    private static IParameterBinder<TValue>? LookupBinder(IServiceProvider services, ILogger logger)
    {
        var binder = services.GetService<IParameterBinder<TValue>>();

        if (binder is not null)
        {
            logger.LogDebug(Template_ResolvedFromDI, typeof(TValue).Name);

            return binder;
        }

        logger.LogDebug(Template_NotResolvedFromDI, typeof(TValue).Name);

        return null;
    }
}
