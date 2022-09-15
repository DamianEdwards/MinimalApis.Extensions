using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TodosApi.Dapper;
using Xunit;

namespace TodosApis.Dapper.IntegrationTests;

public class TodosApiIntegration 
{
    [Fact]
    public async Task GetTodos_IsEmpty_ReturnsOk()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetTodos_IsNotEmpty_ReturnsOk()
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

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task PostTodo_Returns_Created_For_Valid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);
    }
    
    [Fact]
    public async Task PostTodo_Returns_BadRequest_For_Invalid_Todo()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new NewTodo
        {
            Title = null
        };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task UpdateTodo_Returns_NoContent_For_Valid_Todo()
    {
        await using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodoExpected = new NewTodo
        {
            Title = "Create Integration Tests"
        };

        var newTitle = "Title Updated";

        var response = await httpClient.PostAsJsonAsync("/todos", newTodoExpected);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodoExpected.Title, todo.Title);
        Assert.False(todo.IsComplete);

        todo.Title = newTitle;

        response = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}", todo);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        todo = await httpClient.GetFromJsonAsync<Todo>($"/todos/{todo.Id}");

        Assert.Equal(newTitle, todo?.Title);
    }

    [Fact]
    public async Task UpdateTodo_Returns_BadRequest_For_Invalid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodoExpected.Title, todo.Title);
        Assert.False(todo.IsComplete);

        todo.Title = null;

        response = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}", todo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        todo = await httpClient.GetFromJsonAsync<Todo>($"/todos/{todo.Id}");

        Assert.Equal(newTodoExpected.Title, todo?.Title);
    }

    [Fact]
    public async Task UpdateTodo_Returns_NotFound_For_Todo_That_Not_Exists()
    {
        await using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var newTodo = new Todo { Id = 1, Title = "First todo" };

        var response = await httpClient.PostAsJsonAsync("/todos", newTodo);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        var todoTobeUpdated = Assert.Single(todos);

        todoTobeUpdated.Title = "New Title";

        response = await httpClient.PutAsJsonAsync($"/todos/4", todoTobeUpdated);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        var todoWithOldTitle = todos?.FirstOrDefault();

        var todo = await httpClient.GetFromJsonAsync<Todo>($"/todos/{todoTobeUpdated.Id}");

        Assert.Equal(todoWithOldTitle?.Title, todo?.Title);
    }

    [Fact]
    public async Task DeleteTodo_Returns_NoContent_For_Todo_Deleted()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodoExpected.Title, todo.Title);
        Assert.False(todo.IsComplete);

        response = await httpClient.DeleteAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await httpClient.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_Returns_NotFound_For_Todo_That_Not_Exists()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodoExpected.Title, todo.Title);
        Assert.False(todo.IsComplete);

        response = await httpClient.DeleteAsync($"/todos/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        response = await httpClient.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAllTodos_Returns_Ok_For_Successfully_Deleted()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
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

        Assert.Equal(expectedTodos.Length, todos?.Count);

        var deletedTodos = await httpClient.DeleteAsync($"/todos/delete-all");

        Assert.Equal(HttpStatusCode.OK, deletedTodos.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetCompleteTodos_Returns_Ok()
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

        Assert.Equal(expectedTodos.Where(x => x.IsComplete == true).Count(), completedTodos?.Count);
    }

    [Fact]
    public async Task GetCompleteTodos_IsEmpty_ReturnsOk()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetIncompleteTodos_Returns_Ok()
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

        Assert.NotNull(completedTodos);
        Assert.Equal(expectedTodos.Where(x => x.IsComplete == false).Count(), completedTodos?.Count);
    }

    [Fact]
    public async Task GetIncompleteTodos_IsEmpty_ReturnsOk()
    {
        using var application = new TodosApplication();

        var httpClient = application.CreateClient();

        var todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task MarkComplete_Returns_NoContent_For_Valid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        var completeTodo = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        Assert.Equal(HttpStatusCode.NoContent, completeTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.NotNull(todos);
        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);
    }

    [Fact]
    public async Task MarkComplete_Returns_NotFound_For_Invalid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        var completeTodo = await httpClient.PutAsJsonAsync($"/todos/2/mark-complete", todo);

        Assert.Equal(HttpStatusCode.NotFound, completeTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task MarkIncomplete_Returns_NotFound_For_Invalid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.NotNull(todos);
        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);

        var incompleteTodo = await httpClient.PutAsJsonAsync($"/todos/2/mark-incomplete", todo);

        Assert.Equal(HttpStatusCode.NotFound, incompleteTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task MarkIncomplete_Returns_NoContent_For_Valid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);

        await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-complete", todo);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/complete");

        Assert.NotNull(todos);
        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.True(todo.IsComplete);

        var incompleteTodo = await httpClient.PutAsJsonAsync($"/todos/{todo.Id}/mark-incomplete", todo);

        Assert.Equal(HttpStatusCode.NoContent, incompleteTodo.StatusCode);

        todos = await httpClient.GetFromJsonAsync<List<Todo>>("/todos/incomplete");

        Assert.NotNull(todos);
        todo = Assert.Single(todos);
        Assert.Equal(newTodo.Title, todo.Title);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public async Task GetTodoById_Returns_Ok_For_Valid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);

        response = await httpClient.GetAsync($"/todos/{todo.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTodoById_Returns_NotFound_For_Invalid_Todo()
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

        Assert.NotNull(todos);
        var todo = Assert.Single(todos);

        response = await httpClient.GetAsync($"/todos/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
