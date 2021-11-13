using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace MinimalApis.Extensions.UnitTests;

internal static class MockHelpers
{
    public static (Mock<HttpContext>, Mock<IFeatureCollection>, Mock<HttpRequest>, Mock<IServiceProvider>) CreateMockHttpContext(string? requestBody = null, Stream? responseBody = null)
    {
        var httpContext = new Mock<HttpContext>();
        var features = new Mock<IFeatureCollection>();
        var httpRequest = new Mock<HttpRequest>();
        var httpResponse = new Mock<HttpResponse>();
        var serviceProvider = new Mock<IServiceProvider>();
        var items = new Dictionary<object, object?>();

        httpRequest.SetupGet(x => x.Method).Returns("POST");
        httpRequest.SetupGet(x => x.HttpContext).Returns(httpContext.Object);
        httpResponse.SetupProperty(x => x.StatusCode);
        httpResponse.SetupProperty(x => x.ContentType);
        httpContext.SetupGet(x => x.Items).Returns(items);
        httpContext.SetupGet(x => x.Features).Returns(features.Object);
        httpContext.SetupGet(x => x.Request).Returns(httpRequest.Object);
        httpContext.SetupGet(x => x.Response).Returns(httpResponse.Object);
        httpContext.SetupGet(x => x.RequestAborted).Returns(CancellationToken.None);
        httpContext.SetupGet(x => x.RequestServices).Returns(serviceProvider.Object);

        if (requestBody != null)
        {
            var bodyDetectionFeature = new Mock<IHttpRequestBodyDetectionFeature>();
            var bodyBytes = Encoding.UTF8.GetBytes(requestBody);
            bodyDetectionFeature.SetupGet(x => x.CanHaveBody).Returns(true);
            features.Setup(x => x.Get<IHttpRequestBodyDetectionFeature>()).Returns(bodyDetectionFeature.Object);
            httpRequest.SetupGet(x => x.ContentType).Returns("application/json");
            httpRequest.SetupGet(x => x.ContentLength).Returns(bodyBytes.Length);
            httpRequest.SetupGet(x => x.Body).Returns(new MemoryStream(bodyBytes));
        }

        if (responseBody != null)
        {
            httpResponse.SetupGet(x => x.Body).Returns(responseBody);
        }

        return (httpContext, features, httpRequest, serviceProvider);
    }
}
