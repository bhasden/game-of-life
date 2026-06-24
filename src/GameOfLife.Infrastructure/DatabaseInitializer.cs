using Dapper;

namespace GameOfLife.Infrastructure;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            PRAGMA journal_mode = WAL;

            CREATE TABLE IF NOT EXISTS boards (
                id TEXT PRIMARY KEY,
                height INTEGER NOT NULL CHECK (height > 0),
                width INTEGER NOT NULL CHECK (width > 0),
                created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS board_generations (
                board_id TEXT NOT NULL,
                generation INTEGER NOT NULL CHECK (generation >= 0),
                live_cells_json TEXT NOT NULL,
                state_hash TEXT NOT NULL,
                created_at TEXT NOT NULL,
                PRIMARY KEY (board_id, generation),
                FOREIGN KEY (board_id) REFERENCES boards(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_board_generations_hash
                ON board_generations (board_id, state_hash);
            """;

        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
