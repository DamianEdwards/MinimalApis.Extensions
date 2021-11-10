using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Bind{TValue}"/> class.
    /// </summary>
    /// <param name="modelValue">The value bound.</param>
    public Bind(TValue? modelValue)
    {
        _value = modelValue;
    }

    /// <summary>
    /// The value bound.
    /// </summary>
    public TValue? Value => _value;

    private static Bind<TValue?> WrapResult(TValue? value) => new(value);

    public static implicit operator TValue?(Bind<TValue> model) => model.Value;

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="Bind{TValue}"/>.</returns>
    /// <exception cref="BadHttpRequestException">Thrown if the default binding logic results in a status code other than <see cref="StatusCodes.Status200OK"/>.</exception>
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

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Bind<TValue>>>();
        var binder = LookupBinder(services, logger);

        return binder switch
        {
            IProvideEndpointParameterMetadata => IProvideEndpointParameterMetadata.GetMetadataLateBound(parameter, services),
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
