using System.Data.Common;
using System.Text.Json;
using Dapper;
using GameOfLife.Domain;

namespace GameOfLife.Infrastructure;

public interface IBoardRepository
{
    Task CreateBoardAsync(Guid boardId, BoardState initialState, CancellationToken cancellationToken);

    Task<BoardRecord?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken);

    Task<StoredGeneration?> GetGenerationAsync(Guid boardId, int generation, CancellationToken cancellationToken);

    Task<StoredGeneration?> GetLatestGenerationAtOrBeforeAsync(Guid boardId, int generation, CancellationToken cancellationToken);

    Task<StoredGeneration> SaveGenerationAsync(Guid boardId, int generation, BoardState state, CancellationToken cancellationToken);
}

public sealed record BoardRecord(Guid Id, int Height, int Width, DateTimeOffset CreatedAt);

public sealed record StoredGeneration(int Generation, BoardState State, string StateHash, DateTimeOffset CreatedAt);

public sealed class BoardRepository(IDbConnectionFactory connectionFactory) : IBoardRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task CreateBoardAsync(Guid boardId, BoardState initialState, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var createdAt = DateTimeOffset.UtcNow;

        const string insertBoardSql = """
            INSERT INTO boards (id, height, width, created_at)
            VALUES (@Id, @Height, @Width, @CreatedAt);
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            insertBoardSql,
            new
            {
                Id = boardId.ToString(),
                initialState.Height,
                initialState.Width,
                CreatedAt = createdAt.ToString("O")
            },
            transaction,
            cancellationToken: cancellationToken));

        await SaveGenerationCoreAsync(connection, transaction, boardId, 0, initialState, createdAt, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<BoardRecord?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT id, height, width, created_at AS CreatedAt
            FROM boards
            WHERE id = @Id;
            """;

        var row = await connection.QuerySingleOrDefaultAsync<BoardRow>(new CommandDefinition(
            sql,
            new { Id = boardId.ToString() },
            cancellationToken: cancellationToken));

        return row?.ToBoardRecord();
    }

    public async Task<StoredGeneration?> GetGenerationAsync(Guid boardId, int generation, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await GetGenerationCoreAsync(connection, null, boardId, generation, cancellationToken);
    }

    public async Task<StoredGeneration?> GetLatestGenerationAtOrBeforeAsync(Guid boardId, int generation, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT g.generation,
                   b.height,
                   b.width,
                   g.live_cells_json AS LiveCellsJson,
                   g.state_hash AS StateHash,
                   g.created_at AS CreatedAt
            FROM board_generations g
            INNER JOIN boards b ON b.id = g.board_id
            WHERE g.board_id = @BoardId AND g.generation <= @Generation
            ORDER BY g.generation DESC
            LIMIT 1;
            """;

        var row = await connection.QuerySingleOrDefaultAsync<GenerationRow>(new CommandDefinition(
            sql,
            new { BoardId = boardId.ToString(), Generation = generation },
            cancellationToken: cancellationToken));

        return row?.ToStoredGeneration(JsonOptions);
    }

    public async Task<StoredGeneration> SaveGenerationAsync(Guid boardId, int generation, BoardState state, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await SaveGenerationCoreAsync(connection, null, boardId, generation, state, DateTimeOffset.UtcNow, cancellationToken);

        var storedGeneration = await GetGenerationCoreAsync(connection, null, boardId, generation, cancellationToken);
        return storedGeneration ?? throw new InvalidOperationException("Saved generation could not be loaded.");
    }

    private static async Task SaveGenerationCoreAsync(
        DbConnection connection,
        DbTransaction? transaction,
        Guid boardId,
        int generation,
        BoardState state,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT OR IGNORE INTO board_generations
                (board_id, generation, live_cells_json, state_hash, created_at)
            VALUES
                (@BoardId, @Generation, @LiveCellsJson, @StateHash, @CreatedAt);
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                BoardId = boardId.ToString(),
                Generation = generation,
                LiveCellsJson = JsonSerializer.Serialize(state.LiveCells, JsonOptions),
                StateHash = state.ComputeHash(),
                CreatedAt = createdAt.ToString("O")
            },
            transaction,
            cancellationToken: cancellationToken));
    }

    private static async Task<StoredGeneration?> GetGenerationCoreAsync(
        DbConnection connection,
        DbTransaction? transaction,
        Guid boardId,
        int generation,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT g.generation,
                   b.height,
                   b.width,
                   g.live_cells_json AS LiveCellsJson,
                   g.state_hash AS StateHash,
                   g.created_at AS CreatedAt
            FROM board_generations g
            INNER JOIN boards b ON b.id = g.board_id
            WHERE g.board_id = @BoardId AND g.generation = @Generation;
            """;

        var row = await connection.QuerySingleOrDefaultAsync<GenerationRow>(new CommandDefinition(
            sql,
            new { BoardId = boardId.ToString(), Generation = generation },
            transaction,
            cancellationToken: cancellationToken));

        return row?.ToStoredGeneration(JsonOptions);
    }

    private sealed class BoardRow
    {
        public string Id { get; set; } = string.Empty;

        public long Height { get; set; }

        public long Width { get; set; }

        public string CreatedAt { get; set; } = string.Empty;

        public BoardRecord ToBoardRecord()
        {
            return new BoardRecord(
                Guid.Parse(Id),
                (int)Height,
                (int)Width,
                DateTimeOffset.Parse(CreatedAt));
        }
    }

    private sealed class GenerationRow
    {
        public long Generation { get; set; }

        public long Height { get; set; }

        public long Width { get; set; }

        public string LiveCellsJson { get; set; } = string.Empty;

        public string StateHash { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;

        public StoredGeneration ToStoredGeneration(JsonSerializerOptions jsonOptions)
        {
            var liveCells = JsonSerializer.Deserialize<Cell[]>(LiveCellsJson, jsonOptions) ?? [];
            var state = new BoardState((int)Height, (int)Width, liveCells);

            return new StoredGeneration(
                (int)Generation,
                state,
                StateHash,
                DateTimeOffset.Parse(CreatedAt));
        }
    }
}
