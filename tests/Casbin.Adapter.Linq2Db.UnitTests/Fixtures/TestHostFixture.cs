using Casbin.Adapter.Linq2Db.Migration;
using FluentMigrator.Runner;
using LinqToDB.AspNet;
using LinqToDB.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetCasbin.Persist;

namespace Casbin.Adapter.Linq2Db.UnitTests.Fixtures;

public sealed class TestHostFixture<T> where T : class
{
    public TestServer Server { get; }

    public IServiceProvider Services { get; }

    public TestHostFixture()
    {
        var dbName = $"{typeof(T).Name}.db";
        var connectionString = $"Data Source={dbName}";

        if (File.Exists(dbName))
            File.Delete(dbName);

        Services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(migrationRunnerBuilder =>
                migrationRunnerBuilder
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateCasbinRules).Assembly).For.Migrations()
            )
            .AddLinqToDBContext<CasbinDbContext<int>>((_, builder) =>
            {
                builder.UseSQLite(connectionString);
            })
            .AddScoped<IAdapter, Linq2DbAdapter<int>>()
            .BuildServiceProvider();

        Server = new TestServer(Services);

        UpdateDatabase(Services);
    }

    private static void UpdateDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}
