using Mvc = Microsoft.AspNetCore.Mvc;

namespace MinimalApis.Extensions.Results
{
    using MinimalApis.Extensions.Metadata;

    public class Html : Text, IProvideEndpointResponseMetadata
    {
        private const string HtmlMediaType = "text/html";

        public Html(string html)
            : base(html, HtmlMediaType)
        {

        }

        public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services)
        {
            yield return new Mvc.ProducesAttribute(HtmlMediaType);
        }
    }
}