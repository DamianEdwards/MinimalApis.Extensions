using System.Buffers;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
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
    private static readonly ConcurrentDictionary<ParameterInfo, int> _paramMaxLengthCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Body(TBody value)
    {
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

        var maxBodySize = _paramMaxLengthCache.GetOrAdd(parameter,
            pi => pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? MaxSizeLessThanLOH);

        if (context.Request.Headers.ContentLength > maxBodySize)
        {
            throw LimitMemoryStream.CreateOverCapacityException(maxBodySize);
        }

        // Pipelines
        var readSize = (int?)context.Request.Headers.ContentLength ?? maxBodySize;
        SequencePosition position = default;
        try
        {
            var result = await context.Request.BodyReader.ReadAtLeastAsync(readSize, context.RequestAborted);
            position = result.Buffer.End;
            if (result.IsCanceled)
            {
                throw new OperationCanceledException("Read call was canceled.");
            }
            if (!result.IsCompleted)
            {
                // Too big!
                throw new BadHttpRequestException("Body is too big.", StatusCodes.Status400BadRequest);
            }

            if (typeof(TBody) == typeof(byte[]))
            {
                return new Body<TBody>((TBody)(object)result.Buffer.ToArray());
            }

            if (typeof(TBody) == typeof(ReadOnlyMemory<byte>))
            {
                return new Body<TBody>((TBody)(object)new ReadOnlyMemory<byte>(result.Buffer.ToArray()));
            }

            if (typeof(TBody) == typeof(string))
            {
                var requestEncoding = context.Request.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8;
                var bodyAsString = string.Create((int)result.Buffer.Length, result, (chars, state) => requestEncoding.GetChars(state.Buffer, chars));
                return new Body<TBody>((TBody)(object)bodyAsString);
            }
        }
        finally
        {
            context.Request.BodyReader.AdvanceTo(position);
        }

        //// Streams
        //byte[]? bodyBuffer;
        //int bodyLength;

        //if (context.Request.Headers.ContentLength.HasValue)
        //{
        //    var contentLength = (int)context.Request.Headers.ContentLength.Value;

        //    // Read directly into the buffer of request size
        //    var offset = 0;
        //    var eos = false;
        //    bodyBuffer = new byte[contentLength];
        //    while (!eos)
        //    {
        //        try
        //        {
        //            var bytesRead = await context.Request.Body.ReadAsync(bodyBuffer, offset, contentLength, context.RequestAborted);
        //            offset += bytesRead;
        //            eos = offset >= contentLength || bytesRead == 0;
        //        }
        //        catch (ArgumentOutOfRangeException)
        //        {
        //            throw new BadHttpRequestException("Content-Length header value specified length longer than actual request body size. Correct or remove the Content-Length header and try again.");
        //        }
        //    }
        //    bodyLength = offset;
        //}
        //else
        //{
        //    // Read up to max size
        //    const int bufferSize = 4096;
        //    using var limitStream = new LimitMemoryStream(maxBodySize, bufferSize);
        //    await context.Request.Body.CopyToAsync(limitStream, bufferSize, context.RequestAborted);
        //    if (typeof(TBody) == typeof(ReadOnlyMemory<byte>) && limitStream.TryGetBuffer(out var streamBuffer))
        //    {
        //        // Return the underlying buffer allocated by the MemoryStream
        //        return new Body<TBody>((TBody)(object)streamBuffer);
        //    }
        //    bodyBuffer = limitStream.ToArray();
        //    bodyLength = bodyBuffer.Length;
        //}

        //if (typeof(TBody) == typeof(byte[]))
        //{
        //    return new Body<TBody>((TBody)(object)bodyBuffer);
        //}

        //if (typeof(TBody) == typeof(ReadOnlyMemory<byte>))
        //{
        //    return new Body<TBody>((TBody)(object)new ReadOnlyMemory<byte>(bodyBuffer));
        //}

        //if (typeof(TBody) == typeof(string))
        //{
        //    var requestEncoding = context.Request.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8;
        //    var bodyAsString = requestEncoding.GetString(bodyBuffer, 0, bodyLength);
            
        //    return new Body<TBody>((TBody)(object)bodyAsString);
        //}

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

    private static readonly Type[] _supportedTypes = new[] { typeof(byte[]), typeof(string), typeof(ReadOnlyMemory<byte>) };

    private static readonly string _unsupportedTypeExceptionMessage = $"{nameof(Body<TBody>)} only supports the following types: {Environment.NewLine}"
        + string.Join(Environment.NewLine, _supportedTypes.Select(t => t.Name).ToArray());

    private static bool IsSupportedTValue(Type type)
    {
        return Array.IndexOf(_supportedTypes, type) != -1;
    }

    internal sealed class LimitMemoryStream : MemoryStream
    {
        // This class is licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        // Taken from https://github.com/dotnet/runtime

        private readonly int _maxSize;

        public LimitMemoryStream(int maxSize, int capacity)
            : base(capacity)
        {
            Debug.Assert(capacity <= maxSize);
            _maxSize = maxSize;
        }

        public byte[] GetSizedBuffer()
        {
            ArraySegment<byte> buffer;
            return TryGetBuffer(out buffer) && buffer.Offset == 0 && buffer.Count == buffer.Array!.Length ?
                buffer.Array :
                ToArray();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckSize(count);
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            CheckSize(1);
            base.WriteByte(value);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckSize(count);
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            CheckSize(buffer.Length);
            return base.WriteAsync(buffer, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            CheckSize(count);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ArraySegment<byte> buffer;
            if (TryGetBuffer(out buffer))
            {
                ValidateCopyToArguments(destination, bufferSize);

                long pos = Position;
                long length = Length;
                Position = length;

                long bytesToWrite = length - pos;
                return destination.WriteAsync(buffer.Array!, (int)(buffer.Offset + pos), (int)bytesToWrite, cancellationToken);
            }

            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        private void CheckSize(int countToAdd)
        {
            if (_maxSize - Length < countToAdd)
            {
                throw CreateOverCapacityException(_maxSize);
            }
        }

        public static Exception CreateOverCapacityException(int maxBufferSize)
        {
            return new BadHttpRequestException($"The request body size was greather than the max allowed size of {maxBufferSize}.");
        }
    }
}
