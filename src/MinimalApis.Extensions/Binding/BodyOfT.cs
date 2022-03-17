using System.Buffers;
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
        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap
        const int MaxSizeLessThanLOH = 84999;

        if (!IsSupportedTValue(typeof(TBody)))
        {
            throw new InvalidOperationException(_unsupportedTypeExceptionMessage);
        }

        if (context.Request.Headers.ContentLength > MaxSizeLessThanLOH)
        {
            // TODO: Allow specifying an attribute on the parameter to increase the allowed request size
            throw LimitMemoryStream.CreateOverCapacityException(MaxSizeLessThanLOH);
        }

        byte[]? bodyBuffer = null;
        int bodyLength;

        if (context.Request.Headers.ContentLength.HasValue)
        {
            // Read directly into the buffer of request size
            var contentLength = (int)context.Request.Headers.ContentLength.Value;
            bodyBuffer = new byte[contentLength];
            var offset = 0;
            var eos = false;
            while (!eos)
            {
                var bytesRead = await context.Request.Body.ReadAsync(bodyBuffer, offset, contentLength, context.RequestAborted);
                offset += bytesRead;
                eos = offset >= contentLength || bytesRead == 0;
            }
            bodyLength = offset;
        }
        else
        {
            // Read up to LOH size
            var bufferSize = 1024;
            using var ms = new LimitMemoryStream(MaxSizeLessThanLOH, bufferSize);
            await context.Request.Body.CopyToAsync(ms, bufferSize, context.RequestAborted);
            bodyBuffer = ms.ToArray();
            bodyLength = bodyBuffer.Length;
        }

        if (typeof(TBody) == typeof(byte[]))
        {
            return new Body<TBody>((TBody)(object)bodyBuffer);
        }

        if (typeof(TBody) == typeof(string))
        {
            var requestEncoding = context.Request.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8;
            var bodyAsString = requestEncoding.GetString(bodyBuffer, 0, bodyLength);
            
            return new Body<TBody>((TBody)(object)bodyAsString);
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
        if (IsSupportedTValue(typeof(TBody)))
        {
            yield return new Mvc.ConsumesAttribute("text/plain");
        }
    }

    private static readonly Type[] _supportedTypes = new[] { typeof(byte[]), typeof(string) };

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
