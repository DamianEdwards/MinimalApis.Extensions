namespace MinimalApis.Extensions.Results;

public static class ResultExtensions
{
    public static Ok Ok(this IResultExtensions resultExtensions, string? message = null)
    {
        return new Ok(message);
    }

    public static Ok<TResult> Ok<TResult>(this IResultExtensions resultExtensions, TResult result)
    {
        return new Ok<TResult>(result);
    }

    public static Accepted Accepted(this IResultExtensions resultExtensions)
    {
        return new Accepted();
    }

    public static NoContent NoContent(this IResultExtensions resultExtensions)
    {
        return new NoContent();
    }

    public static Created Created(this IResultExtensions resultExtensions, string uri, object? value)
    {
        return new Created(uri, value);
    }

    public static Created<TResult> Created<TResult>(this IResultExtensions resultExtensions, string uri, TResult result)
    {
        return new Created<TResult>(uri, result);
    }

    public static Text Text(this IResultExtensions resultExtensions, string text, string? contentType = null)
    {
        return new Text(text, contentType);
    }

    public static PlainText PlainText(this IResultExtensions resultExtensions, string text)
    {
        return new PlainText(text);
    }

    public static NotFound NotFound(this IResultExtensions resultExtensions, string? message = null)
    {
        return new NotFound(message);
    }

    public static BadRequest BadRequest(this IResultExtensions resultExtensions, string? message = null, int statusCode = StatusCodes.Status400BadRequest)
    {
        return new BadRequest(message, statusCode);
    }

    public static UnprocessableEntity UnprocessableEntity(this IResultExtensions resultExtensions, string? message = null)
    {
        return new UnprocessableEntity(message);
    }

    public static UnsupportedMediaType UnsupportedMediaType(this IResultExtensions resultExtensions, string? message = null)
    {
        return new UnsupportedMediaType(message);
    }

    public static StatusCode StatusCode(this IResultExtensions resultExtensions, int statusCode, string? text, string? contentType = null)
    {
        return new StatusCode(statusCode, text, contentType);
    }

    public static Problem Problem(this IResultExtensions resultExtensions, string? detail = null, string? instance = null, int? statusCode = null, string? title = null, string? type = null, Dictionary<string, object>? extensions = null)
    {
        var problemDetails = new Mvc.ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = instance,
            Type = type
        };
        if (extensions != null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions.Add(extension.Key, extension.Value);
            }
        }

        return new Problem(problemDetails);
    }

    public static ValidationProblem ValidationProblem(this IResultExtensions resultExtensions, Dictionary<string, string[]> errors)
    {
        return new ValidationProblem(errors);
    }

    public static ValidationProblem ValidationProblem(this IResultExtensions resultExtensions, IDictionary<string, string[]> errors)
    {
        return new ValidationProblem(errors);
    }

    public static IResult Problem(this IResultExtensions resultExtensions, Mvc.ProblemDetails problemDetails)
    {
        return new Problem(problemDetails);
    }

    public static CreatedJsonOrXml<TResult> CreatedJsonOrXml<TResult>(this IResultExtensions resultExtensions, TResult responseBody, string contentType)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        Results.CreatedJsonOrXml<TResult>.ThrowIfUnsupportedContentType(contentType);

        return new CreatedJsonOrXml<TResult>(responseBody, contentType);
    }

    public static IResult Html(this IResultExtensions resultExtensions, string html)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        return new Html(html);
    }

    public static FromFile FromFile(this IResultExtensions resultExtensions, string filePath, string? contentType = null, int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        return new FromFile(filePath, contentType, statusCode);
    }
}
