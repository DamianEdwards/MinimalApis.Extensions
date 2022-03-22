# MinimalApis.Extensions
A set of extensions and helpers that extend the funcationality of ASP.NET Core Minimal APIs.

# Installation
[![Nuget](https://img.shields.io/nuget/v/MinimalApis.Extensions)](https://www.nuget.org/packages/MinimalApis.Extensions/)

This package is currently available in prerelease from nuget.org:

``` console
> dotnet add package MinimalApis.Extensions --prerelease
```

# Getting Started
1. [Install the NuGet package](#installation) into your ASP.NET Core project:
    ``` shell
    > dotnet add package MinimalApis.Extensions --prerelease
    ```
1. In your project's `Program.cs`, call the `AddEndpointsProvidesMetadataApiExplorer()` method on `builder.Services` to enable enhanced endpoint metadata in `ApiExplorer`:
    ``` c#
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEndpointsProvidesMetadataApiExplorer(); // <-- Add this line
    builder.Services.AddSwaggerGen();
    ...
    ```
1. Update your Minimal APIs to use the helper binding and result types from this library, e.g.:
    ``` c#
    app.MapPost("/todos", async Task<Results<ValidationProblem, Created<Todo>>> (Validated<Todo> input, TodoDb db) =>
    {
        if (!input.IsValid)
            return Results.Extensions.ValidationProblem(input.Errors);
        
        var todo = input.Value;
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return Results.Extensions.Created($"/todos/{todo.Id}", todo);
    });
    ```

# What's Included?
This library provides types that help extend the core functionality of ASP.NET Core Minimal APIs in the following ways:
- Enhanced parameter binding
- Typed `IResult` objects for easier unit testing (available via `Results.Extensions`)
- Automatic population of detailed endpoint descriptions in Swagger/OpenAPI via the ability for input and result types to add to endpoint metadata via `IProvideEndpointParameterMetadata` and `IProvideEndpointResponseMetadata`
- Union `IResult` return types via `Results<TResult1, TResultN>` that enable route handler delegates to declare all the possible `IResult` types they can return, enabling compile-time type checking and automatic population of possible responses in Swagger/OpenAPI

# Sample Projects
## [TodoApis.Dapper](/samples/TodosApi.Dapper/)
An example Todos application using ASP.NET Core Minimal APIs and the Dapper library for data storage in SQLite.

## [MinimalApis.Examples](/samples/MinimalApis.Examples/)
Contains small examples for other types in this library.

## [MinimalApiPlayground](https://github.com/DamianEdwards/MinimalApiPlayground)
Shows many examples of using the types in this library along with other things related to ASP.NET Core Minimal APIs.