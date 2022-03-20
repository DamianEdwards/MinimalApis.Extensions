using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TBody"></typeparam>
public record struct Body<TBody> : IProvideEndpointParameterMetadata
{
    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap
    private const int MaxSizeLessThanLOH = 84999;
    private static readonly ConditionalWeakTable<ParameterInfo, MaxLengthAttribute?> _paramMaxLengthAttrCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public Body(TBody value)
    {
        if (!IsSupportedTValue(typeof(TBody)))
        {
            throw new ArgumentException(_unsupportedTypeExceptionMessage, nameof(TBody));
        }

        Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public TBody Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Value?.ToString();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator TBody(Body<TBody> value) => value.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static async ValueTask<Body<TBody>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
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
                return Create(result.Buffer.ToArray());
            }

            if (typeof(TBody) == typeof(ReadOnlyMemory<byte>))
            {
                return Create((ReadOnlyMemory<byte>)result.Buffer.ToArray());
            }

            if (typeof(TBody) == typeof(string))
            {
                var encoding = GetRequestEncoding(context);
                var bodyAsString = string.Create((int)result.Buffer.Length, (result, encoding),
                    static (chars, state) => state.encoding.GetChars(state.result.Buffer, chars));
                return Create(bodyAsString);
            }
        }
        finally
        {
            context.Request.BodyReader.AdvanceTo(position);
        }

        throw new InvalidOperationException(_unsupportedTypeExceptionMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services)
    {
        if (typeof(TBody) == typeof(byte[]) || typeof(TBody) == typeof(ReadOnlyMemory<byte>))
        {
            yield return new Mvc.ConsumesAttribute("application/octet-stream");
        }
        if (IsSupportedTValue(typeof(TBody)))
        {
            yield return new Mvc.ConsumesAttribute("text/plain");
        }
    }

    private static Encoding GetRequestEncoding(HttpContext context)
    {
        if (!string.IsNullOrEmpty(context.Request.ContentType))
        {
            return context.Request.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8;
        }

        return Encoding.UTF8;
    }

    private static readonly Type[] _supportedTypes = new[] { typeof(byte[]), typeof(string), typeof(ReadOnlyMemory<byte>) };

    private static readonly string _unsupportedTypeExceptionMessage = $"{nameof(Body<TBody>)} only supports the following types: {Environment.NewLine}"
        + string.Join(Environment.NewLine, _supportedTypes.Select(t => t.Name).ToArray());

    private static bool IsSupportedTValue(Type type)
    {
        return Array.IndexOf(_supportedTypes, type) != -1;
    }

    private static Body<TBody> Create(byte[] value) => new Body<TBody>((TBody)(object)value);
    private static Body<TBody> Create(string value) => new Body<TBody>((TBody)(object)value);
    private static Body<TBody> Create(ReadOnlyMemory<byte> value) => new Body<TBody>((TBody)(object)value);
}
