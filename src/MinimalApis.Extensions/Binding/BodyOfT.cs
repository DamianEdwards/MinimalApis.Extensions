using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Net.Http.Headers;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// Represents the request body read into the type specified by <typeparamref name="TBody"/>.<br/>
/// Max accepted request body size defaults to <c>84999</c> bytes to prevent allocations to the <see href="https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap">Large Object Heap</see>.
/// <para>
/// The following types are supported:
/// <list type="bullet">
/// <item><see cref="String"/>, i.e. <c>Body&lt;string&gt;</c></item>
/// <item><see cref="T:byte[]"/>, i.e. <c>Body&lt;byte[]&gt;</c></item>
/// <item><see cref="ReadOnlyMemory{Byte}"/>, i.e. <c>Body&lt;ReadOnlyMemory&lt;byte&gt;&gt;</c></item>
/// </list>
/// </para>
/// <example>
/// Use the <see cref="MaxLengthAttribute"/> to set the size of the request body accepted:
/// <code>
/// app.MapPost("/myapi", ([MaxLength(100)]Body&lt;string&gt; body) => $"Received: {body}");
/// </code>
/// </example>
/// </summary>
/// <typeparam name="TBody">
/// The type to read the request body into.
/// <para>
/// The following types are supported:
/// <list type="bullet">
/// <item><see cref="String"/>, i.e. <c>Body&lt;string&gt;</c></item>
/// <item><see cref="T:byte[]"/>, i.e. <c>Body&lt;byte[]&gt;</c></item>
/// <item><see cref="ReadOnlyMemory{Byte}"/>, i.e. <c>Body&lt;ReadOnlyMemory&lt;byte&gt;&gt;</c></item>
/// </list>
/// </para>
/// </typeparam>
public record struct Body<TBody> : IEndpointParameterMetadataProvider
{
    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap
    private const int MaxSizeLessThanLOH = 84999;
    private static readonly ConditionalWeakTable<ParameterInfo, MaxLengthAttribute?> _paramMaxLengthAttrCache = new();
    private HttpContext _httpContext;
    private MediaTypeHeaderValue? _requestContentType = null;
    private bool _requestContentTypeSet = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Body{TBody}"/> class.
    /// </summary>
    /// <param name="value">The body value.</param>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="TBody"/> is not one of the supported types.</exception>
    public Body(TBody value, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!IsSupportedTValue(typeof(TBody)))
        {
            throw new ArgumentException(_unsupportedTypeExceptionMessage, nameof(TBody));
        }

        Value = value;
        _httpContext = httpContext;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Body{TBody}"/> class.
    /// </summary>
    /// <param name="value">The body value.</param>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <param name="contentType">The request body's content type.</param>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="TBody"/> is not one of the supported types.</exception>
    public Body(TBody value, HttpContext httpContext, MediaTypeHeaderValue? contentType = null)
        : this(value, httpContext)
    {
        _requestContentType = contentType;
        _requestContentTypeSet = true;
    }

    /// <summary>
    /// Gets the body value.
    /// </summary>
    public TBody Value { get; }

    /// <summary>
    /// Gets the request body's content type if the <c>Content-Type</c> header was set.
    /// </summary>
    public MediaTypeHeaderValue? ContentType
    {
        get
        {
            if (!_requestContentTypeSet)
            {
                _requestContentType = _httpContext.Request.GetTypedHeaders().ContentType;
                _requestContentTypeSet = true;
            }
            return _requestContentType;
        }
    }

    /// <summary>
    /// Gets the request body's <see cref="System.Text.Encoding"/> if the <c>Content-Type</c> header was set.
    /// </summary>
    /// <remarks>
    /// Note that if <typeparamref name="TBody"/> is <see cref="String"/> and the request did not specify an encoding via the <c>Content-Type</c>
    /// header, the string is created using <see cref="Encoding.UTF8"/> and this property will be <c>null</c>.
    /// </remarks>
    public Encoding? Encoding => ContentType?.Encoding;

    /// <summary>
    /// Gets the result of calling <c><see cref="Value"/>.ToString()</c>.
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Value?.ToString();

    /// <summary>
    /// Gets <see cref="Body{TBody}"/> as <typeparamref name="TBody"/>.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator TBody(Body<TBody> value) => value.Value;

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="Body{TValue}"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="TBody"/> is not one of the supported types.</exception>
    /// <exception cref="BadHttpRequestException">Thrown when the request body exceeds the maximum size allowed.</exception>
    /// <exception cref="OperationCanceledException">Thrown when reading the request body is canceled.</exception>
    public static async ValueTask<Body<TBody>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameter);

        if (!IsSupportedTValue(typeof(TBody)))
        {
            throw new ArgumentException(_unsupportedTypeExceptionMessage, nameof(TBody));
        }

        var maxBodySize = _paramMaxLengthAttrCache.GetValue(parameter,
            static pi => pi.GetCustomAttribute<MaxLengthAttribute>())?.Length ?? MaxSizeLessThanLOH;

        var contentLength = context.Request.Headers.ContentLength;

        if (contentLength > maxBodySize)
        {
            throw new BadHttpRequestException($"The request body was larger than the max allowed of {maxBodySize} bytes.", StatusCodes.Status400BadRequest);
        }

        var readSize = (int?)contentLength ?? maxBodySize;
        SequencePosition position = default;
        try
        {
            var result = await context.Request.BodyReader.ReadAtLeastAsync(readSize, context.RequestAborted);
            position = result.Buffer.End;
            if (result.IsCanceled)
            {
                throw new OperationCanceledException("Read call was canceled.");
            }
            if (!result.IsCompleted || result.Buffer.Length > readSize)
            {
                // Too big!
                throw new BadHttpRequestException($"The request body was larger than the max allowed of {readSize} bytes.", StatusCodes.Status400BadRequest);
            }

            if (typeof(TBody) == typeof(byte[]))
            {
                return Create(result.Buffer.ToArray(), context);
            }

            if (typeof(TBody) == typeof(ReadOnlyMemory<byte>))
            {
                return Create((ReadOnlyMemory<byte>)result.Buffer.ToArray(), context);
            }

            if (typeof(TBody) == typeof(string))
            {
                var requestContentType = GetRequestContentType(context);
                var encoding = requestContentType?.Encoding ?? Encoding.UTF8;
                var bodyAsString = encoding.GetString(result.Buffer);
                return Create(bodyAsString, context, requestContentType);
            }
        }
        finally
        {
            context.Request.BodyReader.AdvanceTo(position);
        }

        // Should never hit here
        throw new InvalidOperationException("Supported types mismatch.");
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
        if (typeof(TBody) == typeof(string))
        {
            metadata.Add(new MinimalApis.Extensions.Metadata.AcceptsMetadata(typeof(string), MinimalApis.Extensions.Metadata.AcceptsMetadata.TextPlainContentType));
        }
    }

    private static MediaTypeHeaderValue? GetRequestContentType(HttpContext context)
    {
        if (!string.IsNullOrEmpty(context.Request.ContentType))
        {
            return context.Request.GetTypedHeaders().ContentType;
        }

        return null;
    }

    private static readonly Type[] _supportedTypes = new[] { typeof(byte[]), typeof(string), typeof(ReadOnlyMemory<byte>) };

    private static readonly string _unsupportedTypeExceptionMessage = $"{nameof(Body<TBody>)} only supports the following types: {Environment.NewLine}"
        + string.Join(Environment.NewLine, _supportedTypes.Select(t => t.Name).ToArray());

    private static bool IsSupportedTValue(Type type)
    {
        return Array.IndexOf(_supportedTypes, type) != -1;
    }

    private static Body<TBody> Create(byte[] value, HttpContext httpContext) => new Body<TBody>((TBody)(object)value, httpContext);
    private static Body<TBody> Create(ReadOnlyMemory<byte> value, HttpContext httpContext) => new Body<TBody>((TBody)(object)value, httpContext);
    private static Body<TBody> Create(string value, HttpContext httpContext, MediaTypeHeaderValue? contentType) => new Body<TBody>((TBody)(object)value, httpContext, contentType);
}
