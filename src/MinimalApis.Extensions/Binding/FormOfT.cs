using System.Buffers;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Form<TValue> : IProvideEndpointParameterMetadata
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
        var value = JsonSerializer.Deserialize<TValue?>(bufferWriter.WrittenSpan);

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

    private static void Transform(IFormCollection form, Utf8JsonWriter jsonWriter)
    {
        if (form.Keys.Count > 0)
        {
            jsonWriter.WriteStartObject();

            foreach (var field in form)
            {
                jsonWriter.WritePropertyName(field.Key);

                // TODO: Handle different number types, e.g. long, double, etc.
                if (char.IsDigit(field.Value, 0) && int.TryParse(field.Value, out var number))
                {
                    jsonWriter.WriteNumberValue(number);
                }
                else if (string.Equals(field.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(field.Value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                {
                    jsonWriter.WriteBooleanValue(bool.Parse(field.Value));
                }
                else if (DateTime.TryParseExact(field.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    jsonWriter.WriteStringValue(dateTime);
                }
                else if (DateTimeOffset.TryParseExact(field.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
                {
                    jsonWriter.WriteStringValue(dateTimeOffset);
                }
                else
                {
                    var value = field.Value.ToString();
                    jsonWriter.WriteStringValue(value);
                }
            }

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }
    }


    //private class FormCollectionStream : Stream
    //{
    //    private readonly HttpContext _httpContext;
    //    private IFormCollection? _formCollection;
    //    private HashSet<string>? _keys;
    //    private HashSet<string>.Enumerator _keyEnum;
    //    private bool _endOfKeys = false;
    //    private int _posInKeyName = 0;
    //    private int _posInValue = 0;
    //    private State _state = State.NotStarted;

    //    public FormCollectionStream(HttpContext httpContext)
    //    {
    //        _httpContext = httpContext;
    //    }

    //    public override bool CanRead => true;
    //    public override bool CanSeek => false;
    //    public override bool CanWrite => false;
    //    public override long Length => throw new NotSupportedException();
    //    public override long Position { get; set; }

    //    public override void Flush()
    //    {
            
    //    }

    //    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    //    {
    //        if (_formCollection == null)
    //        {
    //            if (!_httpContext.Request.HasFormContentType)
    //            {
    //                throw new InvalidOperationException("Request body was not in a supported form content type.");
    //            }
    //            _formCollection = await _httpContext.Request.ReadFormAsync(cancellationToken);
    //            _keys = new HashSet<string>(_formCollection.Keys, StringComparer.OrdinalIgnoreCase);
    //            _keyEnum = _keys.GetEnumerator();
    //        }

    //        return Read(buffer.Span);
    //    }

    //    public override int Read(Span<byte> buffer)
    //    {
    //        if (_keys == null)
    //        {
    //            throw new InvalidOperationException("Stream must be initialized with an async call to ReadAsync before calls to Read are permitted.");
    //        }

    //        if (_keys.Count == 0)
    //        {
    //            return 0;
    //        }

    //        var written = 0;

    //        while (written < count && !_endOfKeys)
    //        {
    //            if (_state == State.NotStarted)
    //            {
    //                written += WriteOpenCurlyBrace(buffer, ref currOffset, ref written);
    //                _state = State.BeforeKey;
    //            }
    //            else if (_state == State.BeforeKey)
    //            {
    //                _endOfKeys = _keyEnum.MoveNext();
    //                if (!_endOfKeys)
    //                {
    //                    JsonEncodedText.Encode()
    //                    var keyByteCount = Encoding.UTF8.GetByteCount(_keyEnum.Current);
    //                    if ((keyByteCount + 3) <= (count - written))
    //                    {
    //                        // Room for next quoted key name & separator so write as entire block, e.g. "keyName":
    //                        written += WriteEntireKeyName(_keyEnum.Current, buffer, ref currOffset, ref written);
    //                        _state = State.BeforeValue;
    //                    }
    //                    else // TODO: Figure out how much can be written to buffer and do that much
    //                    {
    //                        // Write opening quote
    //                        Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
    //                        written++;
    //                        _state = State.InKeyName;
    //                        _posInKeyName = 0;
    //                    }
    //                }
    //            }
    //            else if (_state == State.InKeyName)
    //            {
    //                var countToWrite = Math.Min(count - written, keyByteCount);
    //                var keyBytes = JsonEncodedText.Encode(_keyEnum.Current);
    //                //Buffer.BlockCopy(
    //            }
    //            else if (_state == State.BeforeKeyValueSeparator)
    //            {

    //            }
    //            else if (_state == State.BeforeValue)
    //            {
    //                // Write opening quote
    //                Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
    //                written++;
    //                _state = State.InStringValue;
    //                _posInValue = 0;
    //            }
    //            else if (_state == State.InStringValue)
    //            {

    //            }
    //            else if (_state == State.BeforeMemberSeparator)
    //            {

    //            }
    //        }

    //        Position += written;

    //        return written;
    //    }

    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override void SetLength(long value)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    private static int WriteOpenCurlyBrace(byte[] buffer, ref int currOffset, ref int written)
    //    {
    //        Buffer.SetByte(buffer, currOffset, Tokens.OpeningCurlyBrace);
    //        written++;
    //        currOffset += written;
    //        return 1;
    //    }

    //    private static int WriteEntireKeyName(string key, byte[] buffer, ref int currOffset, ref int written)
    //    {
    //        var startingWritten = written;

    //        // "
    //        Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
    //        currOffset++;
    //        written++;

    //        // keyname
    //        var keyBytes = Encoding.UTF8.GetBytes(key);
    //        // Can we do this with span instead?
    //        Buffer.BlockCopy(keyBytes, 0, buffer, currOffset, keyBytes.Length);
    //        currOffset += keyBytes.Length;
    //        written += keyBytes.Length;

    //        // "
    //        Buffer.SetByte(buffer, currOffset, Tokens.DoubleQuotes);
    //        currOffset++;
    //        written++;

    //        // :
    //        Buffer.SetByte(buffer, currOffset, Tokens.Colon);
    //        currOffset++;
    //        written++;

    //        return written - startingWritten;
    //    }

    //    public enum State
    //    {
    //        NotStarted,
    //        BeforeKey, //  { "name" : "value",
    //                   //   ^
    //        InKeyName, //  { "name" : "value",
    //                   //     ^
    //        BeforeKeyValueSeparator, //  { "name" : "value",
    //                                 //          ^
    //        BeforeValue, //  { "name" : "value",
    //                     //            ^
    //        InStringValue,
    //        InNumericValue,
    //        InBoolValue,
    //        InArrayValue,
    //        BetweenArrayValues,
    //        BeforeMemberSeparator //  { "name" : "value" ,
    //                              //                    ^
    //    }

    //    public static class Tokens
    //    {
    //        public static readonly byte OpeningCurlyBrace = Encoding.UTF8.GetBytes("{")[0];
    //        public static readonly byte ClosingCurlyBrace = Encoding.UTF8.GetBytes("}")[0];
    //        public static readonly byte OpeningSquareBrace = Encoding.UTF8.GetBytes("[")[0];
    //        public static readonly byte ClosingSquareBrace = Encoding.UTF8.GetBytes("]")[0];
    //        public static readonly byte Colon = Encoding.UTF8.GetBytes(":")[0];
    //        public static readonly byte DoubleQuotes = Encoding.UTF8.GetBytes("\"")[0];
    //    }
    //}
}
