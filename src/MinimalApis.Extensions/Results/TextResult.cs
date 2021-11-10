namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status200OK"/> response with a text response body.
/// </summary>
public abstract class TextResult : StatusCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class.
    /// </summary>
    /// <param name="text">The text to write to the response body.</param>
    /// <param name="contentType">The content type of the response body.</param>
    public TextResult(string text, string? contentType = null)
        : base(StatusCodes.Status200OK, text, contentType)
    {

    }
}
