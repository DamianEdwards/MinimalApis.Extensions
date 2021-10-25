namespace MinimalApis.Extensions.Results;

public class Text : StatusCode
{
    public Text(string text, string? contentType = null)
        : base(StatusCodes.Status200OK, text, contentType)
    {

    }
}
