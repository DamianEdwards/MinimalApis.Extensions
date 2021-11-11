namespace MinimalApis.Extensions.Results;

/// <summary>
/// Contains extension methods for creating typed <see cref="IResult"/> objects to return from Minimal APIs.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns an <see cref="Results.Ok"/> <see cref="IResult"/> with <see cref="StatusCodes.Status200OK"/> and an optional message.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.Ok"/> instance.</returns>
    public static Ok Ok(this IResultExtensions resultExtensions, string? message = null)
    {
        return new Ok(message);
    }

    /// <summary>
    /// Returns an <see cref="Results.Ok{TextResult}"/> <see cref="IResult"/> with <see cref="StatusCodes.Status200OK"/>
    /// and the provided <typeparamref name="TResult"/> serialized to JSON as the response body.
    /// </summary>
    /// <typeparam name="TResult">The <see cref="Type"/> of object to JSON serialize to the response body.</typeparam>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="result">The <typeparamref name="TResult"/> to JSON serialize to the response body.</param>
    /// <returns>The <see cref="Results.Ok{TResult}"/> instance.</returns>
    public static Ok<TResult> Ok<TResult>(this IResultExtensions resultExtensions, TResult result)
    {
        return new Ok<TResult>(result);
    }

    /// <summary>
    /// Returns an <see cref="Results.Accepted"/> <see cref="IResult"/> with <see cref="StatusCodes.Status202Accepted"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.Accepted"/> instance.</returns>
    public static Accepted Accepted(this IResultExtensions resultExtensions)
    {
        return new Accepted();
    }

    /// <summary>
    /// Returns a <see cref="Results.NoContent"/> <see cref="IResult"/> with <see cref="StatusCodes.Status204NoContent"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.NoContent"/> instance.</returns>
    public static NoContent NoContent(this IResultExtensions resultExtensions)
    {
        return new NoContent();
    }

    /// <summary>
    /// Returns a <see cref="Results.Created"/> <see cref="IResult"/> with <see cref="StatusCodes.Status201Created"/>.
    /// </summary>
    /// <remarks>
    /// This overload does not include type information for the entity created and thus cannot provide metadata for ApiExplorer and OpenAPI about
    /// the type of entity the <see cref="Endpoint"/> returns. Consider using the <see cref="Created{TResult}(IResultExtensions, string, TResult)"/>
    /// overload instead.
    /// </remarks>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="uri">The URI for the entity created, returned to the client in the <c>Location</c> response header.</param>
    /// <param name="result">The <see cref="Object"/> that represents the entity that was created.</param>
    /// <returns>The <see cref="Results.Created"/> instance.</returns>
    public static Created Created(this IResultExtensions resultExtensions, string uri, object? result)
    {
        return new Created(uri, result);
    }

    /// <summary>
    /// Returns a <see cref="Results.Created"/> <see cref="IResult"/> with <see cref="StatusCodes.Status201Created"/>.
    /// </summary>
    /// <typeparam name="TResult">The <see cref="Type"/> of the entity created.</typeparam>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="uri">The URI for the entity created, returned to the client in the <c>Location</c> response header.</param>
    /// <param name="result">The <see cref="Object"/> that represents the entity that was created.</param>
    /// <returns>The <see cref="Results.Created{TResult}"/> instance.</returns>
    public static Created<TResult> Created<TResult>(this IResultExtensions resultExtensions, string uri, TResult result)
    {
        return new Created<TResult>(uri, result);
    }

    /// <summary>
    /// Returns a <see cref="Results.Conflict"/> <see cref="IResult"/> with <see cref="StatusCodes.Status409Conflict"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.Conflict"/> instance.</returns>
    public static Conflict Conflict(this IResultExtensions resultExtensions, string? message = null)
    {
        return new Conflict(message);
    }

    /// <summary>
    /// Returns a <see cref="Results.PlainText"/> <see cref="IResult"/> with <see cref="StatusCodes.Status200OK"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="text">The text to return in the response body.</param>
    /// <returns>The <see cref="Results.PlainText"/> instance.</returns>
    public static PlainText PlainText(this IResultExtensions resultExtensions, string text)
    {
        return new PlainText(text);
    }

    /// <summary>
    /// Returns a <see cref="Results.NotFound"/> <see cref="IResult"/> with <see cref="StatusCodes.Status404NotFound"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.NotFound"/> instance.</returns>
    public static NotFound NotFound(this IResultExtensions resultExtensions, string? message = null)
    {
        return new NotFound(message);
    }

    /// <summary>
    /// Returns a <see cref="Results.BadRequest"/> <see cref="IResult"/> with <see cref="StatusCodes.Status400BadRequest"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.BadRequest"/> instance.</returns>
    public static BadRequest BadRequest(this IResultExtensions resultExtensions, string? message = null)
    {
        return new BadRequest(message);
    }

    /// <summary>
    /// Returns a <see cref="RedirectTemporary"/> <see cref="IResult"/> with <see cref="StatusCodes.Status302Found"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="RedirectTemporary"/> instance.</returns>
    public static RedirectTemporary Redirect(this IResultExtensions resultExtensions)
    {
        return new RedirectTemporary();
    }

    /// <summary>
    /// Returns a <see cref="Results.RedirectPermanent"/> <see cref="IResult"/> with <see cref="StatusCodes.Status301MovedPermanently"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.RedirectPermanent"/> instance.</returns>
    public static RedirectPermanent RedirectPermanent(this IResultExtensions resultExtensions)
    {
        return new RedirectPermanent();
    }

    /// <summary>
    /// Returns a <see cref="Results.RedirectTemporary307"/> <see cref="IResult"/> with <see cref="StatusCodes.Status307TemporaryRedirect"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.RedirectTemporary307"/> instance.</returns>
    public static RedirectTemporary307 RedirectTemporary307(this IResultExtensions resultExtensions)
    {
        return new RedirectTemporary307();
    }

    /// <summary>
    /// Returns a <see cref="Results.RedirectPermanent308"/> <see cref="IResult"/> with <see cref="StatusCodes.Status308PermanentRedirect"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <returns>The <see cref="Results.RedirectPermanent308"/> instance.</returns>
    public static RedirectPermanent308 RedirectPermanent308(this IResultExtensions resultExtensions)
    {
        return new RedirectPermanent308();
    }

    /// <summary>
    /// Returns an <see cref="Results.Unauthorized"/> <see cref="IResult"/> with <see cref="StatusCodes.Status401Unauthorized"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.Unauthorized"/> instance.</returns>
    public static Unauthorized Unauthorized(this IResultExtensions resultExtensions, string? message = null)
    {
        return new Unauthorized(message);
    }

    /// <summary>
    /// Returns a <see cref="Results.Forbidden"/> <see cref="IResult"/> with <see cref="StatusCodes.Status403Forbidden"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.Forbidden"/> instance.</returns>
    public static Forbidden Forbidden(this IResultExtensions resultExtensions, string? message = null)
    {
        return new Forbidden(message);
    }

    /// <summary>
    /// Returns an <see cref="Results.UnprocessableEntity"/> <see cref="IResult"/> with <see cref="StatusCodes.Status422UnprocessableEntity"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.UnprocessableEntity"/> instance.</returns>
    public static UnprocessableEntity UnprocessableEntity(this IResultExtensions resultExtensions, string? message = null)
    {
        return new UnprocessableEntity(message);
    }

    /// <summary>
    /// Returns an <see cref="Results.UnsupportedMediaType"/> <see cref="IResult"/> with <see cref="StatusCodes.Status415UnsupportedMediaType"/>.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="message">An optional message to return in the response body.</param>
    /// <returns>The <see cref="Results.UnsupportedMediaType"/> instance.</returns>
    public static UnsupportedMediaType UnsupportedMediaType(this IResultExtensions resultExtensions, string? message = null)
    {
        return new UnsupportedMediaType(message);
    }

    /// <summary>
    /// Returns a <see cref="Results.StatusCode"/> <see cref="IResult"/> with the provided status code.
    /// </summary>
    /// <remarks>
    /// This method cannot provide metadata for ApiExplorer and OpenAPI about the status code the <see cref="Endpoint"/> returns.
    /// Consider using one of the methods that returns an <see cref="IResult"/> that represents a specific status code instead.
    /// </remarks>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="text">An optional message to return in the response body.</param>
    /// <param name="contentType">The content type of the response. Defaults to <c>text/plain; charset=utf-8</c> if <paramref name="text"/> is not null.</param>
    /// <returns>The <see cref="Results.StatusCode"/> instance.</returns>
    public static StatusCode StatusCode(this IResultExtensions resultExtensions, int statusCode, string? text, string? contentType = null)
    {
        return new StatusCode(statusCode, text, contentType);
    }

    /// <summary>
    /// Returns a <see cref="Results.Problem"/> <see cref="IResult"/> with a response body in a machine-readable format for specifying errors
    /// in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
    /// <param name="instance">A URI reference that identifies the specific occurrence of the problem. It may or may not yield further information if dereferenced.</param>
    /// <param name="statusCode">The HTTP status code([RFC7231], Section 6) generated by the origin server for this occurrence of the problem.</param>
    /// <param name="title">A short, human-readable summary of the problem type.It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization(e.g., using proactive content negotiation; see [RFC7231], Section 3.4).</param>
    /// <param name="type">A URI reference [RFC3986] that identifies the problem type. This specification encourages that, when dereferenced, it provide human-readable documentation for the problem type (e.g., using HTML [W3C.REC-html5-20141028]). When this member is not present, its value is assumed to be "about:blank".</param>
    /// <param name="extensions">Additional members to include in the problem details response.</param>
    /// <returns>The <see cref="Results.Problem"/> instance.</returns>
    public static Problem Problem(this IResultExtensions resultExtensions, string? detail = null, string? instance = null, int? statusCode = null, string? title = null, string? type = null, Dictionary<string, object>? extensions = null)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions, nameof(resultExtensions));

        var problemDetails = new Mvc.ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = instance,
            Type = type
        };

        return Problem(resultExtensions, problemDetails, extensions);
    }

    /// <summary>
    /// Returns a <see cref="Results.Problem"/> <see cref="IResult"/> with a response body in a machine-readable format for specifying errors
    /// in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="problemDetails">The <see cref="Mvc.ProblemDetails"/> that details the problem.</param>
    /// <param name="extensions">Additional members to include in the problem details response.</param>
    /// <returns>The <see cref="Results.Problem"/> instance.</returns>
    public static Problem Problem(this IResultExtensions resultExtensions, Mvc.ProblemDetails problemDetails, Dictionary<string, object>? extensions = null)
    {
        if (extensions != null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions.Add(extension.Key, extension.Value);
            }
        }

        return new Problem(problemDetails);
    }

    /// <summary>
    /// Returns a <see cref="Results.ValidationProblem"/> <see cref="IResult"/> with a response body in a machine-readable format for specifying errors
    /// in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details due to validation errors.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="errors"></param>
    /// <returns>The <see cref="Results.ValidationProblem"/> instance.</returns>
    public static ValidationProblem ValidationProblem(this IResultExtensions resultExtensions, Dictionary<string, string[]> errors)
    {
        return new ValidationProblem(errors);
    }

    /// <summary>
    /// Returns a <see cref="Results.ValidationProblem"/> <see cref="IResult"/> with a response body in a machine-readable format for specifying errors
    /// in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details due to validation errors.
    /// </summary>
    /// <param name="resultExtensions">The <see cref="IResultExtensions"/>.</param>
    /// <param name="errors"></param>
    /// <returns>The <see cref="Results.ValidationProblem"/> instance.</returns>
    public static ValidationProblem ValidationProblem(this IResultExtensions resultExtensions, IDictionary<string, string[]> errors)
    {
        return new ValidationProblem(errors);
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
