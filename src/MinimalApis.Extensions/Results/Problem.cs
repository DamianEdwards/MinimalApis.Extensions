using System.Diagnostics;
using System.Text;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a response with a machine-readable format for specifying errors in HTTP
/// API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details response body.
/// </summary>
public class Problem : Json, IProvideEndpointResponseMetadata
{
    /// <summary>
    /// The <c>application/problem+json</c> response content type.
    /// </summary>
    protected const string ResponseContentType = "application/problem+json";

    /// <summary>
    /// Initializes a new instance of the <see cref="Problem"/> class.
    /// </summary>
    /// <param name="problemDetails">The <see cref="Mvc.ProblemDetails"/> that details the problem.</param>
    public Problem(Mvc.ProblemDetails problemDetails)
        : base(problemDetails)
    {
        ContentType = ResponseContentType;
        ProblemDetailsValue = problemDetails;
        StatusCode = problemDetails.Status ??= StatusCodes.Status500InternalServerError;
    }

    /// <summary>
    /// Gets the <see cref="Mvc.ProblemDetails"/> for the response.
    /// </summary>
    public Mvc.ProblemDetails ProblemDetailsValue { get; }

    /// <summary>
    /// Writes the response body content.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="contentTypeEncoding">The <see cref="Encoding"/> to use for the response body.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="ObjectResult.StatusCode"/> is <c>null</c>.</exception>
    protected override async Task WriteResult(HttpContext httpContext, Encoding contentTypeEncoding)
    {
        if (StatusCode == null)
        {
            throw new InvalidOperationException("StatusCode should be set in constructor.");
        }

        ProblemDetailsValue.Status = StatusCode;

        if (ProblemDetailsDefaults.Defaults.TryGetValue(ProblemDetailsValue.Status.Value, out var defaults))
        {
            ProblemDetailsValue.Title ??= defaults.Title;
            ProblemDetailsValue.Type ??= defaults.Type;
        }

        if (!ProblemDetailsValue.Extensions.ContainsKey("requestId"))
        {
            ProblemDetailsValue.Extensions.Add("requestId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        }

        await base.WriteResult(httpContext, contentTypeEncoding);
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(Mvc.ProblemDetails), StatusCodes.Status500InternalServerError, ResponseContentType);
    }
}
