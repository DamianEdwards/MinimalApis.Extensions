using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results
{
    using MinimalApis.Extensions.Metadata;

    public class Ok : StatusCode, IProvideEndpointResponseMetadata
    {
        protected const int ResponseStatusCode = StatusCodes.Status200OK;

        public Ok(string? message = null)
            : base(ResponseStatusCode, message)
        {

        }

        public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
        {
            yield return new Mvc.ProducesResponseTypeAttribute(ResponseStatusCode);
        }
    }
}