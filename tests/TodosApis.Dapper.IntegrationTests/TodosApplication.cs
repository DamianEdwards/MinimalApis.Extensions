using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodosApi.Dapper;

namespace TodosApis.Dapper.IntegrationTests;

class TodosApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<IDbConnection>(_ => new SqliteConnection("Data Source=todos-default.db;Cache=Shared"));

            TodosApplication.EnsureDbRecreated(services).Wait();
        });

        return base.CreateHost(builder);
    }

    private static async Task EnsureDbRecreated(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();

        using var db = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();

        await TodosApplication.EnsureTableDeleted(db);
        await EnsureTableCreated(db);
    }

    private static async Task EnsureTableCreated(IDbConnection db)
    {
        var sql = $@"CREATE TABLE IF NOT EXISTS Todos (
                {nameof(Todo.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                {nameof(Todo.Title)} TEXT NOT NULL,
                {nameof(Todo.IsComplete)} INTEGER DEFAULT 0 NOT NULL CHECK({nameof(Todo.IsComplete)} IN (0, 1))
               );";

        await db.ExecuteAsync(sql);
    }

    private static async Task EnsureTableDeleted(IDbConnection db)
    {
        var sql = $@"DROP TABLE IF EXISTS Todos";

        await db.ExecuteAsync(sql);
    }
}
