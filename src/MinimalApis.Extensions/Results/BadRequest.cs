using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results
{
    using MinimalApis.Extensions.Metadata;

    public class BadRequest : StatusCode, IProvideEndpointResponseMetadata
    {
        private const int ResponseStatusCode = StatusCodes.Status400BadRequest;

        public BadRequest(string? message = null, int statusCode = ResponseStatusCode)
            : base(statusCode, message)
        {

        }

        public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
        {
            yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
        }
    }
}