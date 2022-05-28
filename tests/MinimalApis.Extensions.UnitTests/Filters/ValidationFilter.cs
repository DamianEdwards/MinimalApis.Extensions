#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;

namespace MinimalApis.Extensions.UnitTests.Filters;

public class ValidationFilter
{
    private static List<object[]> UnvalidatableRouteHandlers() => new()
    {
        new [] { () => "Hello" },
        new [] { (int id) => "Hello" },
        new [] { (Poco poco) => "Hello" },
    };

    [Theory]
    [MemberData(nameof(UnvalidatableRouteHandlers))]
    public void ProducesMetadataNotAddedIfNoRouteHandlerParametersAreValidatable(Delegate routeHandler)
    {
        // Arrange
        var builder = new TestEndpointRouteBuilder();

        // Act
        var routeHandlerBuilder = builder.MapPost("/test", routeHandler)
            .WithParameterValidation();

        // Assert
        var endpoint = builder.DataSources.Single().Endpoints.Single();
        Assert.DoesNotContain(endpoint.Metadata, m => m is IApiResponseMetadataProvider produces
            && produces.Type == typeof(HttpValidationProblemDetails));
    }

    private static List<object[]> ValidatableRouteHandlers() => new()
    {
        new [] { (Todo todo) => "Hello" },
        new [] { (TodoValidateableObject todo) => "Hello" }
    };

    [Theory]
    [MemberData(nameof(ValidatableRouteHandlers))]
    public void ProducesMetadataAddedIfRouteHandlerHasParametersThatAreValidatable(Delegate routeHandler)
    {
        // Arrange
        var builder = new TestEndpointRouteBuilder();

        // Act
        var routeHandlerBuilder = builder.MapPost("/test", routeHandler)
            .WithParameterValidation();

        // Assert
        var endpoint = builder.DataSources.Single().Endpoints.Single();
        Assert.Contains(endpoint.Metadata, m => m is IApiResponseMetadataProvider produces
            && produces.Type == typeof(HttpValidationProblemDetails)
            && produces.StatusCode == StatusCodes.Status400BadRequest);
    }
}

internal record Poco(int Id, string Name, DateTime Created);

internal class Todo
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }
}

internal class TodoValidateableObject : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Enumerable.Empty<ValidationResult>();
    }
}

internal class TestEndpointRouteBuilder : IEndpointRouteBuilder
{
    public IServiceProvider ServiceProvider { get; } = null!;

    public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

    public IApplicationBuilder CreateApplicationBuilder()
    {
        throw new NotImplementedException();
    }
}
#endif
