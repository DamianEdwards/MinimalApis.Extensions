using System.Diagnostics;
using System.Text;
using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results;

public class Problem : Json
{
    protected const string ResponseContentType = "application/problem+json";

    public Problem(Mvc.ProblemDetails problemDetails)
        : base(problemDetails)
    {
        ContentType = ResponseContentType;
        ProblemDetailsValue = problemDetails;
        StatusCode = problemDetails.Status ??= StatusCodes.Status500InternalServerError;
    }

    public Mvc.ProblemDetails ProblemDetailsValue { get; }

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

        //await httpContext.Response.WriteAsJsonAsync(_problemDetails, _problemDetails.GetType(), options: null, contentType: "application/problem+json");

        await base.WriteResult(httpContext, contentTypeEncoding);
    }
}
