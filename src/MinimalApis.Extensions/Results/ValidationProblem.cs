using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

public class ValidationProblem : Problem, IProvideEndpointResponseMetadata
{
    private const int ResponseStatusCode = StatusCodes.Status400BadRequest;

    public ValidationProblem(IDictionary<string, string[]> errors)
        : base(new HttpValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Status = ResponseStatusCode
        })
    {

    }

    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
    {
        yield return new Mvc.ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), ResponseStatusCode, ResponseContentType);
    }
}
