using System.Diagnostics;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a <see cref="StatusCodes.Status500InternalServerError"/> response with a machine-readable
/// format for specifying errors in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details response body.
/// </summary>
public class Problem : IResult, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// The <c>Content-Type</c> header value for Problem JSON responses.
    /// </summary>
    protected const string ProblemJsonContentType = "application/problem+json";

    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class.
    /// </summary>
    /// <param name="problemDetails">The <see cref="Mvc.ProblemDetails"/> that details the problem.</param>
    public Problem(Mvc.ProblemDetails problemDetails)
    {
        ProblemDetails = problemDetails;
    }

    /// <summary>
    /// Gets the <see cref="Mvc.ProblemDetails"/> for the response.
    /// </summary>
    public Mvc.ProblemDetails ProblemDetails { get; }

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header.
    /// </summary>
    public string ContentType => ProblemJsonContentType;

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode => ProblemDetails.Status;

    /// <summary>
    /// Gets or sets a value indicating whether a request ID should be included in the response.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeRequestId { get; init; } = true;

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;

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

        if (IncludeRequestId && !ProblemDetails.Extensions.ContainsKey("requestId"))
        {
            ProblemDetails.Extensions.Add("requestId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        }

        if (StatusCode is { } code)
        {
            response.StatusCode = code;
        }

        await httpContext.Response.WriteAsJsonAsync(ProblemDetails, null, ProblemJsonContentType);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(Mvc.ProblemDetails), StatusCodes.Status500InternalServerError, ProblemJsonContentType);
    }
}
