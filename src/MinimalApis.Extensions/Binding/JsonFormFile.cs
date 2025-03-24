using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// Represents a JSON file in a multipart/form-data request (i.e. a form upload).
/// </summary>
/// <typeparam name="TValue">The <see cref="Type"/> to deserialize the JSON payload into.</typeparam>
public class JsonFormFile<TValue> : JsonFormFile, IEndpointParameterMetadataProvider
{
    /// <summary>
    /// Creates a new <see cref="JsonFormFile{TValue}"/>.
    /// </summary>
    /// <param name="value">The value deserialized from the JSON file.</param>
    /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use when deserializing.</param>
    public JsonFormFile(TValue value, JsonSerializerOptions jsonSerializerOptions)
        : base(jsonSerializerOptions)
    {
        Value = value;
    }

    /// <summary>
    /// The value deserialized from the JSON file.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Opens the underlying file stream from the <see cref="HttpRequest"/>.
    /// Note this method should not be called directly when using <see cref="JsonFormFile{TValue}"/>.
    /// Access the deserialized value via the <see cref="Value"/> property instead.
    /// </summary>
    /// <returns>The <see cref="Stream"/>.</returns>
    /// <exception cref="InvalidOperationException">Always thrown when called on <see cref="JsonFormFile{TValue}"/>.</exception>
    public override Stream OpenReadStream() =>
        throw new InvalidOperationException($"Cannot open underlying file stream directly. Access the deserialized value via the {nameof(Value)} property.");

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="JsonFormFile{TValue}"/> if a value for <typeparamref name="TValue"/> can be
    /// deserialized from the posted file, otherwise <c>null</c>.</returns>
    public static new async ValueTask<JsonFormFile<TValue>?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        var jsonFile = await JsonFormFile.BindAsync(context, parameter);

        if (jsonFile is not null)
        {
            var value = await jsonFile.DeserializeAsync<TValue>();
            if (value is not null)
            {
                return new JsonFormFile<TValue>(value, jsonFile.JsonSerializerOptions);
            }
        }

        return null;
    }
}

/// <summary>
/// Represents a JSON file in a multipart/form-data request (i.e. a form-based file upload).
/// </summary>
public class JsonFormFile : IEndpointParameterMetadataProvider
{
    internal static readonly JsonSerializerOptions DefaultSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// The <see cref="IFormFile"/>.
    /// </summary>
    protected IFormFile? FormFile;

    /// <summary>
    /// Initializes a new instnace of the <see cref="JsonFormFile"/> class.
    /// </summary>
    /// <param name="jsonSerializerOptions"></param>
    protected internal JsonFormFile(JsonSerializerOptions jsonSerializerOptions)
    {
        JsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Creates a new <see cref="JsonFormFile"/>.
    /// </summary>
    /// <param name="formFile">The <see cref="IFormFile"/>.</param>
    /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use when deserializing the file.</param>
    public JsonFormFile(IFormFile formFile, JsonSerializerOptions jsonSerializerOptions)
        : this(jsonSerializerOptions)
    {
        FormFile = formFile;
    }

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> that are used when deserializing the file.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; init; }

    /// <summary>
    /// Opens the underlying file stream of the uploaded <see cref="FormFile"/>.
    /// </summary>
    /// <returns>The <see cref="Stream"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called before <see cref="BindAsync"/> is called.</exception>
    public virtual Stream OpenReadStream()
    {
        if (FormFile is not null)
        {
            return FormFile.OpenReadStream();
        }

        throw new InvalidOperationException($"Cannot open the file read stream before {nameof(BindAsync)} is called.");
    }

    /// <summary>
    /// Deserializes the contents of the JSON file to an instance of <typeparamref name="TValue"/> using the options
    /// from <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON file to.</typeparam>
    /// <returns>An instance of <typeparamref name="TValue"/> if the contents can be deserialized, otherwise <c>null</c>.</returns>
    public ValueTask<TValue?> DeserializeAsync<TValue>() => DeserializeAsync<TValue>(JsonSerializerOptions);

    /// <summary>
    /// Deserializes the contents of the JSON file to an instance of <typeparamref name="TValue"/> using the provided
    /// <paramref name="jsonOptions"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON file to.</typeparam>
    /// <param name="jsonOptions">The options to use when deserializing the JSON file.</param>
    /// <returns>An instance of <typeparamref name="TValue"/> if the contents can be deserialized, otherwise <c>null</c>.</returns>
    public async ValueTask<TValue?> DeserializeAsync<TValue>(JsonSerializerOptions? jsonOptions)
    {
        using var fileStream = OpenReadStream();
        return await JsonSerializer.DeserializeAsync<TValue>(fileStream, jsonOptions);
    }

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>
    /// An instance of <see cref="JsonFormFile"/> if the request contains a form file field with the same name as the provided
    /// <see cref="ParameterInfo.Name"/>, otherwise <c>null</c>.
    /// </returns>
    public static async ValueTask<JsonFormFile?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        if (!context.Request.HasFormContentType)
        {
            return null;
        }

        var fieldName = parameter.Name;
        var form = await context.Request.ReadFormAsync();

        if (!string.IsNullOrEmpty(fieldName)
            && (form.Files.GetFile(fieldName) is IFormFile file)
            && file.ContentType == "application/json")

        {
            var jsonOptions = context.RequestServices.GetService<JsonOptions>()?.SerializerOptions ?? DefaultSerializerOptions;
            return new JsonFormFile(file, jsonOptions);
        }

        return null;
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// Populates metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="builder"></param>
    public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(builder);

        PopulateMetadataImpl(parameter, builder.Metadata, builder.ApplicationServices);
    }
#else
    /// <summary>
    /// Populates metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="metadata"></param>
    /// <param name="services"></param>
    public static void PopulateMetadata(ParameterInfo parameter, IList<object> metadata, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(services);

        PopulateMetadataImpl(parameter, metadata, services);
    }
#endif

    private static void PopulateMetadataImpl(ParameterInfo parameter, IList<object> metadata, IServiceProvider services)
    {
        metadata.Add(new MinimalApis.Extensions.Metadata.AcceptsMetadata(Metadata.AcceptsMetadata.MultipartFormContentType));
        // TODO: Ensure this metadata is consumed by EndpointMetadataProviderApiDescriptionProvider to configure the parameter
        //       such that the Swagger UI will render the file upload UI automatically.
        metadata.Add(new Mvc.ApiExplorer.ApiParameterDescription { Name = parameter.Name ?? "file", Source = Mvc.ModelBinding.BindingSource.FormFile });
    }
}
