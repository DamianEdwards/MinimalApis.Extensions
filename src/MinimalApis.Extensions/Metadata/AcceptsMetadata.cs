using Microsoft.AspNetCore.Http.Metadata;

namespace MinimalApis.Extensions.Metadata;

internal sealed class AcceptsMetadata : IAcceptsMetadata
{
    public static readonly string[] DefaultContentTypes = new[] { "application/json" };
    public static readonly string[] TextPlainContentType = new[] { "text/plain" };
    public static readonly string[] MultipartFormContentType = new[] { "multipart/form-data" };

    public AcceptsMetadata(string[] contentTypes)
    {
        ArgumentNullException.ThrowIfNull(contentTypes);

        ContentTypes = contentTypes;
    }

    public AcceptsMetadata(Type? type)
        : this(type, false, DefaultContentTypes)
    {

    }

    public AcceptsMetadata(Type? type, string[] contentTypes)
        : this(type, false, contentTypes)
    {

    }

    public AcceptsMetadata(Type? type, bool isOptional, string[] contentTypes)
    {
        ArgumentNullException.ThrowIfNull(contentTypes);

        RequestType = type;
        ContentTypes = contentTypes;
        IsOptional = isOptional;
    }

    public IReadOnlyList<string> ContentTypes { get; }

    public Type? RequestType { get; }

    public bool IsOptional { get; }
}
