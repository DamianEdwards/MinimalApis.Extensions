using System.Buffers;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Form<TValue> : IProvideEndpointParameterMetadata
{
    private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Form(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="formValue"></param>
    public static implicit operator TValue(Form<TValue> formValue) => formValue.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static async ValueTask<Form<TValue?>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        if (!context.Request.HasFormContentType)
        {
            throw new BadHttpRequestException("Request body was not in a supported form content type.", StatusCodes.Status415UnsupportedMediaType);
        }

        var formCollection = await context.Request.ReadFormAsync();
        if (formCollection.Keys.Count == 0)
        {
            return new(default);
        }

        // TODO: Change this to use an IBufferWriter that pools the underlying array buffer, e.g.:
        // https://github.com/dotnet/runtime/blob/c5d40c9e703fd257db1b26ef4fd1399bbae73ab0/src/libraries/Common/src/System/Text/Json/PooledByteBufferWriter.cs
        var bufferSize = CalculateBufferSize(formCollection);
        var bufferWriter = new ArrayBufferWriter<byte>(bufferSize);
        // TODO: Pool the Utf8JsonWriter (it can be reset)
        var jsonWriter = new Utf8JsonWriter(bufferWriter);

        Transform(formCollection, jsonWriter);

        var jsonOptions = context.RequestServices.GetService<JsonOptions>()?.SerializerOptions ?? _defaultJsonOptions;
        var value = JsonSerializer.Deserialize<TValue>(bufferWriter.WrittenSpan, jsonOptions);

        return new(value);
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
            var metadata = new List<object>(_defaultMetadata);
            metadata.AddRange(IProvideEndpointParameterMetadata.GetMetadataLateBound(parameter, services));
            return metadata;
        }

        return _defaultMetadata;
    }

    private static IEnumerable<object> _defaultMetadata = new[] { new Mvc.ConsumesAttribute(typeof(TValue), "multipart/form-data") };

    private static int CalculateBufferSize(IFormCollection form)
    {
        // Rough approximation
        var size = 0;
        foreach (var field in form)
        {
            size += Encoding.UTF8.GetByteCount(field.Key);
            size += Encoding.UTF8.GetByteCount(field.Value);
            size += 5; // two quotes for key, two quotes for value, colon name/value separator
        }

        size += form.Count; // comma field separator
        size += form.Count * 6; // encoding allowance

        return size;
    }

    public static void Transform(IFormCollection form, Utf8JsonWriter jsonWriter)
    {
        if (form.Keys.Count > 0)
        {
            jsonWriter.WriteStartObject();

            var keys = new HashSet<string>(form.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(key => key);
            var currentDepth = 0;
            var closedPrevObject = false;
            ReadOnlySpan<char> prevKeySpan = default;

            foreach (var key in keys)
            {
                // TODO: Good golly supporting this stuff is going to be hard :\
                // Handle objects
                // Title=The Title of the Thing
                // Category.Id=42
                // Category.Name=Thingamabobs
                // Category.Owner.Name=John+Smith
                // {"Title:"The Title of the Thing","Category":{"Id":42,"Name":"Thingamabobs","Owner":{"Name":"John Smith"}}}
                // 
                // TODO: Handle enumerables/dictionaries
                // Enumerables:
                //   Widgets[0].Name=Widget1
                //   Widgets[1].Name=Widget2
                //   {"Widgets":[{"Name":"Widget1"},{"Name":"Widget2"}]}
                // Dictionaries:
                //   Widgets[id1].Name=Widget1
                //   Widgets[id2].Name=Widget2
                //   {"Widgets":{"id1":{"Name":"Widget1"},"id2":{"Name":"Widget2"}}}

                // Write out member name
                var keySpan = key.AsSpan();
                var delimIndex = keySpan.IndexOf('.');

                if (delimIndex > 0)
                {
                    // Walk segments writing out opening object
                    var startIndex = 0;
                    while (delimIndex > 0)
                    {
                        var nextDelimIndex = keySpan.Slice(delimIndex + 1).IndexOf('.');
                        var propertyNameLength = nextDelimIndex > 0
                            ? nextDelimIndex
                            : keySpan.Length - delimIndex - 1;
                        var propertyName = keySpan.Slice(startIndex, propertyNameLength);

                        // Check if we're still in the same object
                        if (!prevKeySpan.StartsWith(keySpan.Slice(0, delimIndex + 1)))
                        {
                            if (!closedPrevObject)
                            {
                                // Close prev object
                                while (currentDepth > 0)
                                {
                                    jsonWriter.WriteEndObject();
                                    currentDepth--;
                                }
                                closedPrevObject = true;
                            }

                            jsonWriter.WriteStartObject(propertyName);
                            currentDepth++;
                        }

                        startIndex = delimIndex + 1;
                        delimIndex = nextDelimIndex > 0 ? delimIndex + nextDelimIndex + 1 : -1;
                    }

                    // Write leaf property name
                    var leafName = keySpan.Slice(startIndex);
                    jsonWriter.WritePropertyName(leafName);
                }
                else if (delimIndex == 0)
                {
                    throw new InvalidDataException("Yeah, nah");
                }
                else
                {
                    // Top level property
                    while (currentDepth > 0)
                    {
                        jsonWriter.WriteEndObject();
                        currentDepth--;
                    }

                    jsonWriter.WritePropertyName(key);
                }
                prevKeySpan = keySpan;
                closedPrevObject = false;

                // Write out value
                var values = form[key];
                if (values.Count > 1)
                {
                    // TODO: Handle multi-value fields as array
                }
                else if (values.Count == 0 || values == "")
                {
                    jsonWriter.WriteNullValue();
                }
                // TODO: Handle different number types, e.g. long, double, etc.
                else if (int.TryParse(values, out var number))
                {
                    jsonWriter.WriteNumberValue(number);
                }
                else if (bool.TryParse(values, out var boolean))
                {
                    jsonWriter.WriteBooleanValue(boolean);
                }
                else if (DateTime.TryParseExact(values, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    jsonWriter.WriteStringValue(dateTime);
                }
                else if (DateTimeOffset.TryParseExact(values, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
                {
                    jsonWriter.WriteStringValue(dateTimeOffset);
                }
                else
                {
                    jsonWriter.WriteStringValue(values);
                }
            }

            // Close open bjects
            while (currentDepth > 0)
            {
                jsonWriter.WriteEndObject();
                currentDepth--;
            }

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }
    }
}
