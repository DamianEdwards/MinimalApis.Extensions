using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents an <see cref="IResult"/> for a response with a machine-readable format for specifying validation errors
/// in HTTP API responses based on https://tools.ietf.org/html/rfc7807.JSON Problem Details response body.
/// </summary>
public class ValidationProblem : Problem, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status400BadRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationProblem"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationProblem(IDictionary<string, string[]> errors)
        : base(new HttpValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Status = ResponseStatusCode
        })
    {

    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static new IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), ResponseStatusCode, ResponseContentType);
    }
}
