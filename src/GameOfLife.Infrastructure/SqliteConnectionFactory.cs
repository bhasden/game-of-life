using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SQLitePCL;

namespace GameOfLife.Infrastructure;

public sealed class SqliteConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    static SqliteConnectionFactory()
    {
        Batteries_V2.Init();
    }

    public DbConnection CreateConnection()
    {
        var databasePath = options.Value.Path;
        var directory = Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new SqliteConnection(options.Value.ConnectionString);
    }
}
