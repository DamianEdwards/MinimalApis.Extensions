#if NET7_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Dapper;

namespace TodosApi.Dapper;

public static class TodosApi
{
    public static void MapTodosApi(this IEndpointRouteBuilder app)
    {
        var todos = app.MapGroup("/todos");

        todos.MapGet("/", GetAllTodos);
        todos.MapGet("/complete", GetCompleteTodos);
        todos.MapGet("/incomplete", GetIncompleteTodos);
        todos.MapGet("/{id}", GetTodoById);
        todos.MapPost("/", CreateTodo);
        todos.MapPut("/{id}", UpdateTodo);
        todos.MapPut("/{id}/mark-complete", MarkComplete);
        todos.MapPut("/{id}/mark-incomplete", MarkIncomplete);
        todos.MapDelete("/{id}", DeleteTodo);
        todos.MapDelete("/delete-all", DeleteAll);

        todos.WithParameterValidation();

        // BLOCKED by change to Swashbuckle being merged & released
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2441
        //todos.WithOpenApi();
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

    public static async Task<Results<ValidationProblem, Created<Todo>>> CreateTodo(NewTodo inputTodo, IDbConnection db)
    {
        var todo = await db.QuerySingleAsync<Todo>(
            "INSERT INTO Todos(Title, IsComplete) Values(@Title, @IsComplete) RETURNING * ", inputTodo);

        return TypedResults.Created($"/todos/{todo.Id}", todo);
    }

    public static async Task<Results<ValidationProblem, NoContent, NotFound>> UpdateTodo(int id, Todo inputTodo, IDbConnection db)
    {
        inputTodo.Id = id;

        return await db.ExecuteAsync("UPDATE Todos SET Title = @Title, IsComplete = @IsComplete WHERE Id = @Id", inputTodo) == 1
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
