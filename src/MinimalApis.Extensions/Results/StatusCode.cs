namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a response with the specified status code.
/// </summary>
public class StatusCode : ResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusCode"/> class.
    /// </summary>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="text">An optional message to return in the response body.</param>
    /// <param name="contentType">The content type of the response. Defaults to <c>text/plain; charset=utf-8</c> if <paramref name="text"/> is not null.</param>
    public StatusCode(int statusCode, string? text, string? contentType = null)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        ResponseContent = text;
    }
}
