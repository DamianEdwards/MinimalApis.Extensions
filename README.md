# MinimalApis.Extensions
A set of extensions and helpers that extend the functionality of ASP.NET Core Minimal APIs.

# Installation
[![Nuget](https://img.shields.io/nuget/v/MinimalApis.Extensions)](https://www.nuget.org/packages/MinimalApis.Extensions/)

## Prerelease Builds
This package is currently available in prerelease from nuget.org:

``` console
> dotnet add package MinimalApis.Extensions --prerelease
```

## CI Builds
If you wish to use builds from this repo's `main` branch you can install them from [this repo's package feed](https://github.com/DamianEdwards/MinimalApis.Extensions/packages/1064549).

1. [Create a personal access token](https://github.com/settings/tokens/new) for your GitHub account with the `read:packages` scope with your desired expiration length:
    
    [<img width="583" alt="image" src="https://user-images.githubusercontent.com/249088/160220117-7e79822e-a18a-445c-89ff-b3d9ca84892f.png">](https://github.com/settings/tokens/new)
3. At the command line, navigate to your user profile directory and run the following command to add the package feed to your NuGet configuration, replacing the `<GITHUB_USER_NAME>` and `<PERSONAL_ACCESS_TOKEN>` placeholders with the relevant values:
    ``` shell
    ~> dotnet nuget add source -n GitHub -u <GITHUB_USER_NAME> -p <PERSONAL_ACCESS_TOKEN> https://nuget.pkg.github.com/DamianEdwards/index.json
    ```
1. You should now be able to add a reference to the package specifying a version from the [repository packages feed](https://github.com/DamianEdwards/MinimalApis.Extensions/packages/1064549)
1. See [these instructions](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry) for further details about working with GitHub package feeds

# Getting Started
1. [Install the NuGet package](#installation) into your ASP.NET Core project:
    ``` shell
    > dotnet add package MinimalApis.Extensions --prerelease
    ```
1. In your project's `Program.cs`, call the `AddEndpointsMetadataProviderApiExplorer()` method on `builder.Services` to enable enhanced endpoint metadata in `ApiExplorer`:
    ``` c#
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEndpointsMetadataProviderApiExplorer(); // <-- Add this line
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
## [TodoApis.Dapper](https://github.com/DamianEdwards/MinimalApis.Extensions/tree/main/samples/TodosApi.Dapper)
An example Todos application using ASP.NET Core Minimal APIs and the Dapper library for data storage in SQLite.

## [MinimalApis.Examples](https://github.com/DamianEdwards/MinimalApis.Extensions/tree/main/samples/MinimalApis.Examples)
Contains small examples for other types in this library.

## [MinimalApiPlayground](https://github.com/DamianEdwards/MinimalApiPlayground)
Shows many examples of using the types in this library along with other things related to ASP.NET Core Minimal APIs.
