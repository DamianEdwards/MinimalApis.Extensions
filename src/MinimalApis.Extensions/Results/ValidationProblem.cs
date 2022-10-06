#if NET6_0
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using MinimalApis.Extensions.Results;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write Problem Details
/// HTTP API responses based on https://tools.ietf.org/html/rfc7807
/// </summary>
public sealed class ValidationProblem : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IContentTypeHttpResult, IValueHttpResult, IValueHttpResult<HttpValidationProblemDetails>
{
    internal ValidationProblem(HttpValidationProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails, nameof(problemDetails));

        if (problemDetails is { Status: not null and not StatusCodes.Status400BadRequest })
        {
            throw new ArgumentException($"{nameof(ValidationProblem)} only supports a 400 Bad Request response status code.", nameof(problemDetails));
        }

        ProblemDetails = problemDetails;

        if (ProblemDetails.Status is null)
        {
            ProblemDetails.Status = StatusCode;
        }

        if (ProblemDetailsDefaults.Defaults.TryGetValue(ProblemDetails.Status.Value, out var defaults))
        {
            ProblemDetails.Title ??= defaults.Title;
            ProblemDetails.Type ??= defaults.Type;
        }
    }

    /// <summary>
    /// Gets the <see cref="HttpValidationProblemDetails"/> instance.
    /// </summary>
    public HttpValidationProblemDetails ProblemDetails { get; }

    object? IValueHttpResult.Value => ProblemDetails;

    HttpValidationProblemDetails? IValueHttpResult<HttpValidationProblemDetails>.Value => ProblemDetails;

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header: <c>application/problem+json</c>.
    /// </summary>
    public string ContentType => "application/problem+json";

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status400BadRequest"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status400BadRequest;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;

        return httpContext.Response.WriteAsJsonAsync(ProblemDetails, null, ContentType);
    }


    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="metadata"></param>
    /// <param name="services"></param>
    public static void PopulateMetadata(MethodInfo method, IList<object> metadata, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(services);

        metadata.Add(new ProducesResponseTypeMetadata(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json"));
    }
}
#endif
