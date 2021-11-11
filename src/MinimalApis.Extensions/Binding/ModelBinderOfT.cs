using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// <para>A bridge to support MVC's model binders in minimal APIs. Specify <see cref="ModelBinder{T}"/> as a paramter type
/// of your method to trigger the model binding system.
/// </para>
/// <para>
/// This requires registering the model binding services by calling
/// <see cref="MvcCoreServiceCollectionExtensions.AddMvcCore(IServiceCollection)"/> or <see cref="MvcServiceCollectionExtensions.AddControllers(IServiceCollection)"/>.
/// </para>
/// </summary>
/// <typeparam name="TValue">The type to model bind</typeparam>
public class ModelBinder<TValue> : IProvideEndpointParameterMetadata
{
    // This caches the model binding information so we don't need to create one from a factory every time
    private static readonly ConcurrentDictionary<(ParameterInfo, IModelBinderFactory, IModelMetadataProvider),
                                                 (IModelBinder, BindingInfo, ModelMetadata)> _cache = new();

    /// <summary>
    /// Creates a new <see cref="ModelBinder{TValue}"/>.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="modelState">The <see cref="ModelStateDictionary"/>.</param>
    public ModelBinder(TValue? model, ModelStateDictionary modelState)
    {
        Model = model;
        ModelState = modelState;
    }

    /// <summary>
    /// The model being bound.
    /// </summary>
    public TValue? Model { get; }

    /// <summary>
    /// The validation information.
    /// </summary>
    public ModelStateDictionary ModelState { get; }

    /// <summary>
    /// Supports deconstructing the <see cref="Model"/> and <see cref="ModelState"/>.
    /// </summary>
    /// <param name="model">The <see cref="Model"/>.</param>
    /// <param name="modelState">The <see cref="ModelState"/>.</param>
    public void Deconstruct(out TValue? model, out ModelStateDictionary modelState)
    {
        model = Model;
        modelState = ModelState;
    }

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="ModelBinder{TValue}"/>.</returns>
    public static async ValueTask<ModelBinder<TValue>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var modelBinderFactory = context.RequestServices.GetRequiredService<IModelBinderFactory>();
        var modelMetadataProvider = context.RequestServices.GetRequiredService<IModelMetadataProvider>();
        var parameterBinder = context.RequestServices.GetRequiredService<ParameterBinder>();

        var (binder, bindingInfo, metadata) = _cache.GetOrAdd((parameter, modelBinderFactory, modelMetadataProvider), static arg =>
        {
            var (parameter, modelBinderFactory, modelMetadataProvider) = arg;

            ModelMetadata metadata = modelMetadataProvider.GetMetadataForType(typeof(TValue));

            var bindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes(), metadata);

            var binder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
            {
                BindingInfo = bindingInfo,
                Metadata = metadata,
                CacheToken = parameter
            });

            return (binder!, bindingInfo!, metadata!);
        });

        // Resolve the value provider factories from MVC options
        var valueProviderFactories = context.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value.ValueProviderFactories;

        // We don't have an action descriptor, so just make up a fake one. Custom binders that rely on 
        // a specific action descriptor (like ControllerActionDescriptor, won't work).
        var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());

        var valueProvider = await CompositeValueProvider.CreateAsync(actionContext, valueProviderFactories);
        var paramterDescriptor = new ParameterDescriptor
        {
            BindingInfo = bindingInfo,
            Name = parameter.Name!,
            ParameterType = parameter.ParameterType
        };

        var result = await parameterBinder.BindModelAsync(actionContext, binder, valueProvider, paramterDescriptor, metadata, value: null, container: null);

        return new ModelBinder<TValue>((TValue?)result.Model, actionContext.ModelState);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services)
    {
        if (typeof(TValue).IsAssignableTo(typeof(IProvideEndpointParameterMetadata)))
        {
            return IProvideEndpointParameterMetadata.GetMetadataLateBound(parameter, services);
        }
        return Enumerable.Empty<object>();
    }
}
