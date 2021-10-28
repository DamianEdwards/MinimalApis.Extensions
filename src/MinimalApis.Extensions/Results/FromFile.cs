
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace MinimalApis.Extensions.Results;
public class FromFile : IResult
{
    private const string DefaultMediaType = "text/plain";
    private static readonly IContentTypeProvider _defaultContentTypeProvider = new FileExtensionContentTypeProvider();
    private readonly string _filePath;
    private readonly string? _contentType;
    private readonly int? _statusCode;
    private readonly IFileProvider? _fileProvider;
    private readonly IContentTypeProvider? _contentTypeProvider;

    public FromFile(string filePath, string? contentType, int? statusCode, IFileProvider? fileProvider = null, IContentTypeProvider? contentTypeProvider = null)
    {
        _filePath = filePath;
        _contentType = contentType;
        _statusCode = statusCode;
        _fileProvider = fileProvider;
        _contentTypeProvider = contentTypeProvider;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var fileProvider = _fileProvider ?? httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootFileProvider;
        var file = fileProvider.GetFileInfo(_filePath);

        if (!file.Exists)
        {
            throw new InvalidOperationException($"Specified file path '{_filePath}' does not exist.");
        }

        var contentTypeProvider = _contentTypeProvider ?? _defaultContentTypeProvider;
        var contentType = _contentType;
        if (contentType is null && contentTypeProvider.TryGetContentType(_filePath, out var providerContentType))
        {
            contentType = providerContentType;
        }

        contentType ??= DefaultMediaType;

        httpContext.Response.ContentType = contentType;
        httpContext.Response.StatusCode = _statusCode ?? StatusCodes.Status200OK;

        await httpContext.Response.SendFileAsync(file);
    }
}
