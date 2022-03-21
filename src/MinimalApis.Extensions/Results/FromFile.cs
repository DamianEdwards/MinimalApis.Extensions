using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> that uses the content of a file as the response body.
/// </summary>
public class FromFile : IResult
{
    private const string DefaultMediaType = "text/plain";
    private static readonly IContentTypeProvider _defaultContentTypeProvider = new FileExtensionContentTypeProvider();
    private readonly string _filePath;
    private readonly string? _contentType;
    private readonly int? _statusCode;
    private readonly IFileProvider? _fileProvider;
    private readonly IContentTypeProvider? _contentTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FromFile"/> class.
    /// </summary>
    /// <param name="filePath">The path of the file to use as the reponse body.</param>
    /// <param name="contentType">An optional content type for the response body. Defaults to a content type derived from the file name extension.</param>
    /// <param name="statusCode">An optional status code to return. Defaults to <see cref="StatusCodes.Status200OK"/>.</param>
    /// <param name="fileProvider">An optional <see cref="IFileProvider"/> to retrieve the file from. Defaults to <see cref="IHostingEnvironment.ContentRootFileProvider"/>.</param>
    /// <param name="contentTypeProvider">An option <see cref="IContentTypeProvider"/> to use to lookup the content type from the file name extension. Defaults to <see cref="FileExtensionContentTypeProvider"/>.</param>
    public FromFile(string filePath, string? contentType, int? statusCode, IFileProvider? fileProvider = null, IContentTypeProvider? contentTypeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));

        _filePath = filePath;
        _contentType = contentType;
        _statusCode = statusCode;
        _fileProvider = fileProvider;
        _contentTypeProvider = contentTypeProvider;
    }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

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
