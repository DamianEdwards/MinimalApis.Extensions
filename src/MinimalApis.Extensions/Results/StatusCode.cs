namespace MinimalApis.Extensions.Results;

public class StatusCode : ContentResult
{
    public StatusCode(int statusCode, string? text, string? contentType = null)
    {
        StatusCode = statusCode;
        ResponseContent = text;
        ContentType = contentType;
    }
}
