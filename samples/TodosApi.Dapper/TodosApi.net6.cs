#if NET6_0
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApis.Extensions.Binding;
using Dapper;
using System.ComponentModel.DataAnnotations;

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
        TypedResults.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos"));

    public static async Task<Ok<IEnumerable<Todo>>> GetCompleteTodos(IDbConnection db) =>
        TypedResults.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos WHERE IsComplete = true"));

    public static async Task<Ok<IEnumerable<Todo>>> GetIncompleteTodos(IDbConnection db) =>
        TypedResults.Ok(await db.QueryAsync<Todo>("SELECT * FROM Todos WHERE IsComplete = false"));

    public static async Task<Results<Ok<Todo>, NotFound>> GetTodoById(int id, IDbConnection db) =>
        await db.QuerySingleOrDefaultAsync<Todo>("SELECT * FROM Todos WHERE Id = @id", new { id })
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();

    public static async Task<Results<ValidationProblem, Created<Todo>>> CreateTodo(Validated<NewTodo> inputTodo, IDbConnection db)
    {
        if (!inputTodo.IsValid)
            return TypedResults.ValidationProblem(inputTodo.Errors);

        var todo = await db.QuerySingleAsync<Todo>(
            "INSERT INTO Todos(Title, IsComplete) Values(@Title, @IsComplete) RETURNING * ", inputTodo.Value);

        return TypedResults.Created($"/todos/{todo.Id}", todo);
    }

    public static async Task<Results<ValidationProblem, NoContent, NotFound>> UpdateTodo(int id, Validated<Todo> inputTodo, IDbConnection db)
    {
        if (!inputTodo.IsValid || inputTodo.Value is null)
            return TypedResults.ValidationProblem(inputTodo.Errors);

        inputTodo.Value.Id = id;

        return await db.ExecuteAsync("UPDATE Todos SET Title = @Title, IsComplete = @IsComplete WHERE Id = @Id", inputTodo.Value) == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    public static async Task<Results<NoContent, NotFound>> MarkComplete(int id, IDbConnection db) =>
        await db.ExecuteAsync("UPDATE Todos SET IsComplete = true WHERE Id = @id", new { id }) == 1
        ? TypedResults.NoContent()
        : TypedResults.NotFound();

    public static async Task<Results<NoContent, NotFound>> MarkIncomplete(int id, IDbConnection db) =>
        await db.ExecuteAsync("UPDATE Todos SET IsComplete = false WHERE Id = @id", new { id }) == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    public static async Task<Results<NoContent, NotFound>> DeleteTodo(int id, IDbConnection db) =>
        await db.ExecuteAsync("DELETE FROM Todos WHERE Id = @id", new { id }) == 1
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    public static async Task<Ok<int>> DeleteAll(IDbConnection db) =>
        TypedResults.Ok(await db.ExecuteAsync("DELETE FROM Todos"));
}

public class NewTodo
{
    [Required]
    public string? Title { get; set; }

    public bool IsComplete { get; set; }

    public static implicit operator Todo(NewTodo todo) => new() { Title = todo.Title, IsComplete = todo.IsComplete };
}

public class Todo
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    public bool IsComplete { get; set; }
}
#endif
