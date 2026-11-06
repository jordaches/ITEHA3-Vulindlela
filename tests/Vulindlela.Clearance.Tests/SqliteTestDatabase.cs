using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data;

namespace Vulindlela.Clearance.Tests;

public sealed class SqliteTestDatabase : IDisposable
{
    private readonly string _path;
    public string ConnectionString { get; }

    public SqliteTestDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"vulindlela-clearance-test-{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_path};Default Timeout=30";
        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public ClearanceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClearanceDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new ClearanceDbContext(options);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path)) File.Delete(_path);
    }
}