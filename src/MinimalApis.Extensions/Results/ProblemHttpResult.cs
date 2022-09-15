#if NET6_0
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinimalApis.Extensions.Results;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write Problem Details
/// HTTP API responses based on <see href="https://tools.ietf.org/html/rfc7807"/>
/// </summary>
public sealed class ProblemHttpResult : IResult, IStatusCodeHttpResult, IContentTypeHttpResult, IValueHttpResult, IValueHttpResult<ProblemDetails>
{
    /// <summary>
    /// Creates a new <see cref="ProblemHttpResult"/> instance with
    /// the provided <paramref name="problemDetails"/>.
    /// </summary>
    /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance to format in the entity body.</param>
    internal ProblemHttpResult(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        ProblemDetails = problemDetails;
        //HttpResultsHelper.ApplyProblemDetailsDefaults(ProblemDetails, statusCode: null);
        if (ProblemDetails.Status is null)
        {
            ProblemDetails.Status = ProblemDetails is HttpValidationProblemDetails
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;
        }

        if (ProblemDetailsDefaults.Defaults.TryGetValue(ProblemDetails.Status.Value, out var defaults))
        {
            ProblemDetails.Title ??= defaults.Title;
            ProblemDetails.Type ??= defaults.Type;
        }
    }

    /// <summary>
    /// Gets the <see cref="ProblemDetails"/> instance.
    /// </summary>
    public ProblemDetails ProblemDetails { get; }

    object? IValueHttpResult.Value => ProblemDetails;

    ProblemDetails? IValueHttpResult<ProblemDetails>.Value => ProblemDetails;

    /// <summary>
    /// Gets or sets a value indicating whether a request ID should be included in the response.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeRequestId { get; init; } = true;

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header: <c>application/problem+json</c>
    /// </summary>
    public string ContentType => "application/problem+json";

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode => ProblemDetails.Status;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (IncludeRequestId && !ProblemDetails.Extensions.ContainsKey("requestId"))
        {
            ProblemDetails.Extensions.Add("requestId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        }

        if (StatusCode is { } code)
        {
            httpContext.Response.StatusCode = code;
        }

        return httpContext.Response.WriteAsJsonAsync(ProblemDetails, null, ContentType);
    }
}
#endif
