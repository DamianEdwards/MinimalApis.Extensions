﻿using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Http;
using Moq.Dapper;

namespace TodosApi.Dapper.UnitTests;

public class TodosApiResults
{
    [Fact]
    public async Task GetAllTodos_Returns_Ok_For_No_Todos()
    {
        var db = new Mock<IDbConnection>();

        db.SetupDapperAsync(c => c.QueryAsync<Todo>(It.IsAny<string>(), null, null, null, null))
          .ReturnsAsync(Enumerable.Empty<Todo>());

        var result = await TodosApi.GetAllTodos(db.Object);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task GetAllTodos_Returns_Ok_For_NonEmpty_Todos()
    {
        var db = new Mock<IDbConnection>();

        var expected = new[]
        {
            new Todo { Id = 1, Title = "First todo" },
            new Todo { Id = 2, Title = "Second todo" }
        };

        db.SetupDapperAsync(c => c.QueryAsync<Todo>(It.IsAny<string>(), null, null, null, null))
          .ReturnsAsync(expected);

        var result = await TodosApi.GetAllTodos(db.Object);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task GetAllTodos_Returns_Empty_For_No_Todos()
    {
        var db = new Mock<IDbConnection>();

        db.SetupDapperAsync(c => c.QueryAsync<Todo>(It.IsAny<string>(), null, null, null, null))
          .ReturnsAsync(Enumerable.Empty<Todo>());

        var result = await TodosApi.GetAllTodos(db.Object);

        Assert.Equal(0, result.Value?.Count());
    }

    [Fact]
    public async Task GetAllTodos_Returns_Todos()
    {
        var db = new Mock<IDbConnection>();

        var expected = new[]
        {
            new Todo { Id = 1, Title = "First todo" },
            new Todo { Id = 2, Title = "Second todo" }
        };

        db.SetupDapperAsync(c => c.QueryAsync<Todo>(It.IsAny<string>(), null, null, null, null))
          .ReturnsAsync(expected);

        var result = await TodosApi.GetAllTodos(db.Object);
        var resultTodos = result.Value?.ToList();

        Assert.NotNull(resultTodos);
        Assert.Equal(expected.Length, resultTodos?.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Id, resultTodos?[i].Id);
            Assert.Equal(expected[i].Title, resultTodos?[i].Title);
            Assert.Equal(expected[i].IsComplete, resultTodos?[i].IsComplete);
        }
    }

    [Fact]
    public async Task DeleteAll_Returns_Ok_For_No_Todos()
    {
        var db = new Mock<DbConnection>();

        db.SetupDapperAsync(x => x.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(It.IsAny<int>());

        var result = await TodosApi.DeleteAll(db.Object);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAll_Returns_Ok_For_NonEmpty_Todos()
    {
        var db = new Mock<DbConnection>();

        var expected = new[]
        {
            new Todo { Id = 1, Title = "First todo" },
            new Todo { Id = 2, Title = "Second todo" }
        };

        db.SetupDapperAsync(x => x.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(expected.Length);

        var result = await TodosApi.DeleteAll(db.Object);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAll_Returns_Zero_For_No_Todos()
    {
        var db = new Mock<DbConnection>();

        db.SetupDapperAsync(x => x.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(It.IsAny<int>());

        var result = await TodosApi.DeleteAll(db.Object);

        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task DeleteAll_Returns_Quantity_Of_Todos_Deleted()
    {
        var db = new Mock<DbConnection>();

        var expected = new[]
        {
            new Todo { Id = 1, Title = "First todo" },
            new Todo { Id = 2, Title = "Second todo" }
        };

        db.SetupDapperAsync(x => x.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(expected.Length);

        var result = await TodosApi.DeleteAll(db.Object);

        Assert.Equal(expected.Length, result.Value);
    }
}
