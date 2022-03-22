using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Form<TValue>
{
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
    /// <param name="context"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static async ValueTask<Form<TValue?>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        if (!context.Request.HasFormContentType)
        {
            throw new BadHttpRequestException("Request body was not in a supported form content type.", StatusCodes.Status415UnsupportedMediaType);
        }

        var formStream = new FormCollectionStream(context);

        var value = await JsonSerializer.DeserializeAsync<TValue>(formStream);

        return new(value);

    }

    private class FormCollectionStream : Stream
    {
        private readonly HttpContext _httpContext;
        private IFormCollection? _formCollection;
        private HashSet<string>? _keys;
        private HashSet<string>.Enumerator _keyEnum;
        private string? _currentKey;
        

        public FormCollectionStream(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position { get; set; }

        public override void Flush()
        {
            
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_formCollection == null)
            {
                if (!_httpContext.Request.HasFormContentType)
                {
                    throw new InvalidOperationException("Request body was not in a supported form content type.");
                }
                _formCollection = await _httpContext.Request.ReadFormAsync(cancellationToken);
                _keys = new HashSet<string>(_formCollection.Keys, StringComparer.OrdinalIgnoreCase);
                _keyEnum = _keys.GetEnumerator();
            }

            return Read(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_formCollection == null)
            {
                throw new InvalidOperationException("Stream must be initialized with an async call to ReadAsync before calls to Read are permitted.");
            }

            var written = 0;
            var remaining = count;
            var currOffset = offset;
            if (remaining == 0) return written;

            if (Position == 0)
            {
                buffer[currOffset] = Tokens.OpeningCurlyBrace;
                written++;
                currOffset += written;
                remaining--;
                Position = 1;
            }
            if (remaining == 0) return written;

            var nextKeyByteCount = Encoding.UTF8.GetByteCount(_keyEnum.Current);
            if ((nextKeyByteCount + 2) <= remaining)
            {
                // Room for next key
                var w = WriteKey(_keyEnum.Current, buffer, ref currOffset, ref written);
                written += w;
                Position += w;
            }

            return written;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private static int WriteKey(string key, byte[] buffer, ref int currOffset, ref int written)
        {
            var startingWritten = written;

            Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
            currOffset++;
            written++;

            var keyBytes = Encoding.UTF8.GetBytes(key);
            Buffer.BlockCopy(keyBytes, 0, buffer, currOffset, keyBytes.Length);
            currOffset += keyBytes.Length;
            written += keyBytes.Length;

            Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
            currOffset++;
            written++;

            return written - startingWritten;
        }

        public static class Tokens
        {
            public static readonly byte OpeningCurlyBrace = Encoding.UTF8.GetBytes("{")[0];
            public static readonly byte ClosingCurlyBrace = Encoding.UTF8.GetBytes("}")[0];
            public static readonly byte OpeningSquareBrace = Encoding.UTF8.GetBytes("[")[0];
            public static readonly byte ClosingSquareBrace = Encoding.UTF8.GetBytes("]")[0];
            public static readonly byte Colon = Encoding.UTF8.GetBytes(":")[0];
            public static readonly byte DoubleQuotes = Encoding.UTF8.GetBytes("\"")[0];
        }
    }
}
