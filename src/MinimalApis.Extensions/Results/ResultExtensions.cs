namespace MinimalApis.Extensions.Results;

/// <summary>
/// Contains extension methods for creating typed <see cref="IResult"/> objects to return from Minimal APIs.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns a <see cref="Results.PlainText"/> <see cref="IResult"/> with <see cref="StatusCodes.Status200OK"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="text">The text to return in the response body.</param>
    /// <returns>The <see cref="Results.PlainText"/> instance.</returns>
    public static PlainText PlainText(this IResultExtensions resultExtensions, string text)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));
        ArgumentNullException.ThrowIfNull(text, nameof(text));

        return new PlainText(text);
    }

    /// <summary>
    /// Returns an <see cref="Results.UnsupportedMediaType"/> <see cref="IResult"/> with <see cref="StatusCodes.Status415UnsupportedMediaType"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.UnsupportedMediaType"/> instance.</returns>
    public static UnsupportedMediaType UnsupportedMediaType(this IResultExtensions resultExtensions)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        return new UnsupportedMediaType();
    }

    /// <summary>
    /// Returns an <see cref="Results.Html"/> <see cref="IResult"/> with <see cref="StatusCodes.Status200OK"/> and an HTML response body.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="html">The HTML content to write to the response body.</param>
    /// <returns>The <see cref="Results.Html"/> instance.</returns>
    public static Html Html(this IResultExtensions resultExtensions, string html)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        return new Html(html);
    }

    /// <summary>
    /// Returns an <see cref="Results.FromFile"/> <see cref="IResult"/> with the contents of the file as the response body.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="filePath">The path of the file to use as the reponse body.</param>
    /// <param name="contentType">An optional content type for the response body. Defaults to a content type derived from the file name extension.</param>
    /// <param name="statusCode">An optional status code to return. Defaults to <see cref="StatusCodes.Status200OK"/>.</param>
    /// <returns>The <see cref="Results.FromFile"/> instance.</returns>
    public static FromFile FromFile(this IResultExtensions resultExtensions, string filePath, string? contentType = null, int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        return new FromFile(filePath, contentType, statusCode);
    }
}
