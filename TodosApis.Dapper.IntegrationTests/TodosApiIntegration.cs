using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace TodosApis.Dapper.IntegrationTests;

public class TodosApiIntegration 
{
    [Fact]
    public async Task GetTodos()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.Empty(todos);
    }

    [Fact]
    public async Task PostTodo()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = "Create Integration Tests"
        };
        
        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task DeleteTodo()
    {
        await using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodoExpected = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodoExpected);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodoExpected.Title, todo.Title);
        Assert.False(todo.IsComplete);

        response = await httpClient.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await httpClient.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAllTodos()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.Empty(todos);

        var expectedTodos = new[]
        {
            new Todo { Id = 1, Title = "First todo" },
            new Todo { Id = 2, Title = "Second todo" },
            new Todo { Id = 3, Title = "Third todo" }
        };

        foreach (var todo in expectedTodos)
        {
            var newTodos = await httpClient.PostAsJsonAsync("/todos", todo);

            Assert.Equal(HttpStatusCode.Created, newTodos.StatusCode);
        }

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.Equal(expectedTodos.Length, todos.Count);

        for (int i = 0; i < expectedTodos.Length; i++)
        {
            Assert.Equal(expectedTodos[i].Id, todos[i].Id);
            Assert.Equal(expectedTodos[i].Title, todos[i].Title);
            Assert.Equal(expectedTodos[i].IsComplete, todos[i].IsComplete);
        }

        var deletedTodos = await httpClient.DeleteAsync($"/todos/delete-all");

        Assert.Equal(HttpStatusCode.OK, deletedTodos.StatusCode);
    }

    [Fact]
    public async Task GetCompleteTodos()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var expectedTodos = new[]
        {
            new Todo { Id = 1, Title = "First todo", IsComplete = true },
            new Todo { Id = 2, Title = "Second todo", IsComplete = true },
            new Todo { Id = 3, Title = "Third todo", IsComplete = true },
            new Todo { Id = 3, Title = "Fourth todo", IsComplete = false },
            new Todo { Id = 3, Title = "Fifth todo", IsComplete = false }
        };

        foreach (var todo in expectedTodos)
        {
            var newTodos = await httpClient.PostAsJsonAsync("/todos", todo);

            Assert.Equal(HttpStatusCode.Created, newTodos.StatusCode);
        }

        var completedTodos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.Equal(expectedTodos.Where(x => x.IsComplete == true).Count(), completedTodos.Count());
    }

    [Fact]
    public async Task GetIncompleteTodos()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var expectedTodos = new[]
        {
            new Todo { Id = 1, Title = "First todo", IsComplete = true },
            new Todo { Id = 2, Title = "Second todo", IsComplete = false },
            new Todo { Id = 3, Title = "Third todo", IsComplete = true },
            new Todo { Id = 3, Title = "Fourth todo", IsComplete = false },
            new Todo { Id = 3, Title = "Fifth todo", IsComplete = false }
        };

        foreach (var todo in expectedTodos)
        {
            var newTodos = await httpClient.PostAsJsonAsync("/todos", todo);

            Assert.Equal(HttpStatusCode.Created, newTodos.StatusCode);
        }

        var completedTodos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        Assert.Equal(expectedTodos.Where(x => x.IsComplete == false).Count(), completedTodos.Count());
    }

    [Fact]
    public async Task MarkComplete_Returns_NoContent()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        var completeTodo = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        Assert.Equal(HttpStatusCode.NoContent, completeTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);
    }

    [Fact]
    public async Task MarkComplete_Returns_NotFound()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        var completeTodo = await httpClient.PutAsJsonAsync($"/todos/2/mark-complete", todo);

        Assert.Equal(HttpStatusCode.NotFound, completeTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.Empty(todos);
    }

    [Fact]
    public async Task MarkIncomplete_Returns_NotFound()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);

        var incompleteTodo = await httpClient.PutAsJsonAsync($"/todos/2/mark-incomplete", todo);

        Assert.Equal(HttpStatusCode.NotFound, incompleteTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        Assert.Empty(todos);
    }

    [Fact]
    public async Task MarkIncomplete_Returns_NoContent()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);

        var incompleteTodo = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-incomplete", todo);

        Assert.Equal(HttpStatusCode.NoContent, incompleteTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);
    }
}
