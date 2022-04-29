﻿using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Dapper;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodosApi.Dapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("TodosDb") ?? "Data Source=todos-default.db;Cache=Shared";
builder.Services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));
#if NET6_0
builder.Services.AddEndpointsMetadataProviderApiExplorer();
#else
builder.Services.AddEndpointsApiExplorer();
#endif
builder.Services.AddSwaggerGen(ConfigureSwaggerGen);

var app = builder.Build();

await EnsureDb(app.Services, app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/error", () => Results.Problem("An error occurred.", statusCode: 500))
   .ExcludeFromDescription();

app.MapTodosApi();

app.MapFallback(([FromHeader]string? accept) =>
    accept?.Contains("application/json") == true
        ? Results.Redirect("/swagger/v1/swagger.json")
        : Results.Redirect("/swagger"));

app.Run();

void ConfigureSwaggerGen(SwaggerGenOptions options) =>
    options.CustomOperationIds(api =>
        api.ActionDescriptor.AttributeRouteInfo?.Name
        ?? api.ActionDescriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().FirstOrDefault()?.EndpointName
        ?? api.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault()?.Name);

async Task EnsureDb(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Ensuring database exists at connection string '{connectionString}'", connectionString);

    using var db = services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
    var sql = $"""
                  CREATE TABLE IF NOT EXISTS Todos (
                  {nameof(Todo.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  {nameof(Todo.Title)} TEXT NOT NULL,
                  {nameof(Todo.IsComplete)} INTEGER DEFAULT 0 NOT NULL CHECK({nameof(Todo.IsComplete)} IN (0, 1))
                  );
               """;
    await db.ExecuteAsync(sql);
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }
