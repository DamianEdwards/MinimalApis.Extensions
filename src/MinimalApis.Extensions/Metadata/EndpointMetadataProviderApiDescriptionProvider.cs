#if NET6_0
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using MinimalApis.Extensions.Infrastructure;

namespace MinimalApis.Extensions.Metadata;

/// <summary>
/// An <see cref="IApiDescriptionProvider"/> that adds <see cref="Endpoint"/> metadata provided by
/// <see cref="IEndpointParameterMetadataProvider"/> and <see cref="IEndpointMetadataProvider"/>
/// too ApiExplorer and thus OpenAPI/Swagger documents and UI.
/// </summary>
public class EndpointMetadataProviderApiDescriptionProvider : IApiDescriptionProvider
{
    private readonly IServiceProvider _services;
    private readonly EndpointDataSource _endpointDataSource;

    /// <summary>
    /// Creates an instance of <see cref="EndpointMetadataProviderApiDescriptionProvider"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <param name="endpointDataSource">The <see cref="EndpointDataSource"/>.</param>
    public EndpointMetadataProviderApiDescriptionProvider(IServiceProvider services, EndpointDataSource endpointDataSource)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(endpointDataSource, nameof(endpointDataSource));

        _services = services;
        _endpointDataSource = endpointDataSource;
    }

    /// <summary>
    /// Gets the order value for this provider. This value should ensure it runs after the in-box providers
    /// for route handler endpoints, etc.
    /// </summary>
    public int Order => -1200;

    /// <summary>
    /// Called before the providers are executed.
    /// </summary>
    /// <param name="context">The <see cref="ApiDescriptionProviderContext"/>.</param>
    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {

    }

    /// <summary>
    /// Called after the providers are executed.
    /// </summary>
    /// <param name="context">The <see cref="ApiDescriptionProviderContext"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an existing <see cref="ApiDescription"/> cannot be found for a registered <see cref="RouteEndpoint"/>.
    /// This should never happen. If it does, it might indicate an issue with the value of <see cref="Order"/>.
    /// </exception>
    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        foreach (var endpoint in _endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            var method = endpoint.Metadata.GetMetadata<MethodInfo>();
            if (method is null)
            {
                continue;
            }

            var excludeFromDescMetadata = endpoint.Metadata.GetMetadata<ExcludeFromDescriptionAttribute>();
            if (excludeFromDescMetadata is not null)
                continue;

            var returnType = AwaitableInfo.GetMethodReturnType(method);
            var parameters = method.GetParameters();
            var returnTypeProvidesMetadata = returnType.IsAssignableTo(typeof(IEndpointMetadataProvider));
            var parametersProvideMetadata = parameters.Any(p => p.ParameterType.IsAssignableTo(typeof(IEndpointParameterMetadataProvider)));

            if (!returnTypeProvidesMetadata && !parametersProvideMetadata)
            {
                continue;
            }

            // Route handler delegate has a return type and/or parameters that can provide metadata
            var apiDescription = context.Results.FirstOrDefault(a => a.ActionDescriptor.EndpointMetadata.Contains(method));

            if (apiDescription is null) throw new InvalidOperationException($"Couldn't find existing {nameof(ApiDescription)} for endpoint with route '{endpoint.RoutePattern.RawText}'.");

            if (returnTypeProvidesMetadata)
            {
                EndpointMetadataHelpers.PopulateMetadataLateBound(returnType, apiDescription.ActionDescriptor.EndpointMetadata, _services);

                var responseMetadata = apiDescription.ActionDescriptor.EndpointMetadata.OfType<IProducesResponseTypeMetadata>().ToList();

                if (apiDescription.SupportedResponseTypes.Count == 1 && responseMetadata.Count > 0)
                {
                    // Remove the default response type if we're going to add our own
                    var existingResponseType = apiDescription.SupportedResponseTypes[0];
                    if (existingResponseType.StatusCode == StatusCodes.Status200OK
                        && existingResponseType.Type == typeof(void))
                    {
                        apiDescription.SupportedResponseTypes.RemoveAt(0);
                    }
                }

                foreach (var responseType in responseMetadata)
                {
                    var apiResponseType = new ApiResponseType
                    {
                        StatusCode = responseType.StatusCode,
                        Type = responseType.Type ?? typeof(void)
                    };
                    apiResponseType.ModelMetadata = CreateModelMetadata(apiResponseType.Type);

                    //var contentTypes = new MediaTypeCollection();
                    //responseType.
                    //responseType.SetContentTypes(contentTypes);

                    foreach (var format in responseType.ContentTypes.Select(ct => new ApiResponseFormat { MediaType = ct }))
                    {
                        apiResponseType.ApiResponseFormats.Add(format);
                    }

                    // Swashbuckle doesn't support multiple response types with the same status code
                    if (!apiDescription.SupportedResponseTypes.Any(existingResponseType => existingResponseType.StatusCode == apiResponseType.StatusCode))
                    {
                        apiDescription.SupportedResponseTypes.Add(apiResponseType);
                    }
                }
            }

            if (parametersProvideMetadata)
            {
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;

                    if (!parameterType.IsAssignableTo(typeof(IEndpointParameterMetadataProvider)))
                    {
                        continue;
                    }

                    EndpointParameterMetadataHelpers.PopulateMetadataLateBound(parameter, apiDescription.ActionDescriptor.EndpointMetadata, _services);

                    var acceptsMetadata = apiDescription.ActionDescriptor.EndpointMetadata.OfType<IAcceptsMetadata>().FirstOrDefault();
                    if (acceptsMetadata is null)
                    {
                        continue;
                    }

                    var acceptsRequestType = acceptsMetadata.RequestType;
                    var isOptional = acceptsMetadata.IsOptional;
                    var parameterDescription = new ApiParameterDescription
                    {
                        Name = acceptsRequestType is not null ? acceptsRequestType.Name : typeof(void).Name,
                        ModelMetadata = CreateModelMetadata(acceptsRequestType ?? typeof(void)),
                        Source = BindingSource.Body,
                        Type = acceptsRequestType ?? typeof(void),
                        IsRequired = !isOptional,
                    };
                    apiDescription.ParameterDescriptions.Add(parameterDescription);

                    var supportedRequestFormats = apiDescription.SupportedRequestFormats;
                    foreach (var contentType in acceptsMetadata.ContentTypes)
                    {
                        supportedRequestFormats.Add(new ApiRequestFormat
                        {
                            MediaType = contentType
                        });
                    }
                }
            }
        }
    }

    private static EndpointModelMetadata CreateModelMetadata(Type type)
    {
        return new EndpointModelMetadata(ModelMetadataIdentity.ForType(type));
    }
}

internal class EndpointModelMetadata : ModelMetadata
{
    public EndpointModelMetadata(ModelMetadataIdentity identity) : base(identity)
    {
        IsBindingAllowed = true;
    }

    public override IReadOnlyDictionary<object, object> AdditionalValues { get; } = ImmutableDictionary<object, object>.Empty;
    public override string? BinderModelName { get; }
    public override Type? BinderType { get; }
    public override BindingSource? BindingSource { get; }
    public override bool ConvertEmptyStringToNull { get; }
    public override string? DataTypeName { get; }
    public override string? Description { get; }
    public override string? DisplayFormatString { get; }
    public override string? DisplayName { get; }
    public override string? EditFormatString { get; }
    public override ModelMetadata? ElementMetadata { get; }
    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues { get; }
    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues { get; }
    public override bool HasNonDefaultEditFormat { get; }
    public override bool HideSurroundingHtml { get; }
    public override bool HtmlEncode { get; }
    public override bool IsBindingAllowed { get; }
    public override bool IsBindingRequired { get; }
    public override bool IsEnum { get; }
    public override bool IsFlagsEnum { get; }
    public override bool IsReadOnly { get; }
    public override bool IsRequired { get; }
    public override ModelBindingMessageProvider ModelBindingMessageProvider { get; } = new DefaultModelBindingMessageProvider();
    public override string? NullDisplayText { get; }
    public override int Order { get; }
    public override string? Placeholder { get; }
    public override ModelPropertyCollection Properties { get; } = new(Enumerable.Empty<ModelMetadata>());
    public override IPropertyFilterProvider? PropertyFilterProvider { get; }
    public override Func<object, object>? PropertyGetter { get; }
    public override Action<object, object?>? PropertySetter { get; }
    public override bool ShowForDisplay { get; }
    public override bool ShowForEdit { get; }
    public override string? SimpleDisplayProperty { get; }
    public override string? TemplateHint { get; }
    public override bool ValidateChildren { get; }
    public override IReadOnlyList<object> ValidatorMetadata { get; } = Array.Empty<object>();
}
#endif
