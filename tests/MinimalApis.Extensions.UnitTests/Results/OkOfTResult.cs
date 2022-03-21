using Microsoft.AspNetCore.Http;
using MinimalApis.Extensions.Results;

namespace MinimalApis.Extensions.UnitTests.Results;

public class OkOfTResult
{
    [Fact]
    public void StatusCode_Is_Status200OK()
    {
        var result = new Ok<object>(new { });

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public void Value_Is_OriginalObject()
    {
        var resultObject = new { };
        var result = new Ok<object>(resultObject);

        Assert.Equal(resultObject, result.Value);
    }

    [Fact]
    public void Result_Is_OriginalObject()
    {
        var resultObject = new { };
        var result = new Ok<object>(resultObject);

        Assert.Equal(resultObject, result.Value);
    }
}
