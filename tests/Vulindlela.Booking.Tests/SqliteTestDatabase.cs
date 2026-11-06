using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;

namespace Vulindlela.Booking.Tests;

public sealed class SqliteTestDatabase : IDisposable
{
    private readonly string _path;

    public string ConnectionString { get; }

    public SqliteTestDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"vulindlela-test-{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_path};Default Timeout=30";

        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public BookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new BookingDbContext(options);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path)) File.Delete(_path);
    }
}