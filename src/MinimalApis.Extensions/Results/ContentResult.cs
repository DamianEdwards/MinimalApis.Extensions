using System.Text;

namespace MinimalApis.Extensions.Results
{
    public abstract class ContentResult : IResult
    {
        private const string DefaultContentType = "text/plain; charset=utf-8";
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public string? ContentType { get; init; }

        public string? ResponseContent { get; init; }

        public int? StatusCode { get; init; }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var response = httpContext.Response;

            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                ContentType,
                response.ContentType,
                (DefaultContentType, DefaultEncoding),
                ResponseContentTypeHelper.GetEncoding,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (StatusCode != null)
            {
                response.StatusCode = StatusCode.Value;
            }

            if (ResponseContent != null)
            {
                response.ContentLength = resolvedContentTypeEncoding.GetByteCount(ResponseContent);
                await response.WriteAsync(ResponseContent, resolvedContentTypeEncoding);
            }
        }
    }
}