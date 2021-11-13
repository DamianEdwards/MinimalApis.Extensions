using Microsoft.Data.Sqlite;
using Dapper;
using MinimalApis.Extensions.Results;
using MinimalApis.Extensions.Binding;
using System.Data;

namespace TodosApi.Dapper;

public static class TodosApi
{
    public static void MapTodosApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/todos", GetAllTodos);
        app.MapGet("/todos/complete", GetCompleteTodos);
        app.MapGet("/todos/incomplete", GetIncompleteTodos);
        app.MapGet("/todos/{id}", GetTodoById);
        app.MapPost("/todos", CreateTodo);
        app.MapPut("/todos/{id}", UpdateTodo);
        app.MapPut("/todos/{id}/mark-complete", MarkComplete);
        app.MapPut("/todos/{id}/mark-incomplete", MarkIncomplete);
        app.MapDelete("/todos/{id}", DeleteTodo);
        app.MapDelete("/todos/delete-all", DeleteAll);
    }

    public static async Task<Ok<IEnumerable<Todo>>> GetAllTodos(IDbConnection db) =>
        Results.Extensions.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos"));

    public static async Task<Ok<IEnumerable<Todo>>> GetCompleteTodos(IDbConnection db) =>
        Results.Extensions.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos WHERE IsComplete = true"));

    public static async Task<Ok<IEnumerable<Todo>>> GetIncompleteTodos(IDbConnection db) =>
        Results.Extensions.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos WHERE IsComplete = false"));

    public static async Task<Results<Ok<Todo>, NotFound>> GetTodoById(int id, IDbConnection db) =>
        await db.QuerySingleOrDefaultAsync<Todo>("SELECT * FROM Todos WHERE Id = @id", new { id })
        is Todo todo
            ? Results.Extensions.Ok(todo)
            : Results.Extensions.NotFound();

    public static async Task<Results<ValidationProblem, Problem, Created<Todo>>> CreateTodo(Validated<NewTodo> inputTodo, IDbConnection db)
    {
        if (!inputTodo.IsValid)
            return Results.Extensions.ValidationProblem(inputTodo);

        var todo = await db.QuerySingleAsync<Todo>(
            "INSERT INTO Todos(Title, IsComplete) Values(@Title, @IsComplete) RETURNING * ", inputTodo.Value);

        return Results.Extensions.Created($"/todos/{todo.Id}", todo);
    }

    public static async Task<Results<ValidationProblem, Problem, NoContent, NotFound>> UpdateTodo(int id, Validated<Todo> inputTodo, IDbConnection db)
    {
        if (!inputTodo.IsValid)
            return Results.Extensions.ValidationProblem(inputTodo);

        return await db.ExecuteAsync("UPDATE Todos SET Title = @Title, IsComplete = @IsComplete WHERE Id = @Id", inputTodo.Value) == 1
            ? Results.Extensions.NoContent()
            : Results.Extensions.NotFound();
    }

    public static async Task<Results<NoContent, NotFound>> MarkComplete(int id, IDbConnection db) =>
        await db.ExecuteAsync("UPDATE Todos SET IsComplete = true WHERE Id = @id", new { id }) == 1
        ? Results.Extensions.NoContent()
        : Results.Extensions.NotFound();

    public static async Task<Results<NoContent, NotFound>> MarkIncomplete(int id, IDbConnection db) =>
        await db.ExecuteAsync("UPDATE Todos SET IsComplete = false WHERE Id = @id", new { id }) == 1
            ? Results.Extensions.NoContent()
            : Results.Extensions.NotFound();

    public static async Task<Results<NoContent, NotFound>> DeleteTodo(int id, IDbConnection db) =>
    await db.ExecuteAsync("DELETE FROM Todos WHERE Id = @id", new { id }) == 1
        ? Results.Extensions.NoContent()
        : Results.Extensions.NotFound();

    public static async Task<Ok<int>> DeleteAll(IDbConnection db) =>
        Results.Extensions.Ok(await db.ExecuteAsync("DELETE FROM Todos"));
}
