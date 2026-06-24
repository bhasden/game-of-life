using GameOfLife.Domain;
using GameOfLife.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace GameOfLife.Tests.Infrastructure;

public sealed class BoardRepositoryTests
{
    [Fact]
    public async Task GetLatestGenerationAtOrBeforeAsync_ReturnsHighestStoredGenerationWithinBound()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            var repository = await CreateRepositoryAsync(databasePath);
            var boardId = Guid.CreateVersion7();
            var initial = BoardMatrix.ToBoardState(BlinkerHorizontal());
            var generationOne = GameOfLifeRules.Next(initial);
            var generationThree = GameOfLifeRules.Next(GameOfLifeRules.Next(generationOne));

            await repository.CreateBoardAsync(boardId, initial, CancellationToken.None);
            await repository.SaveGenerationAsync(boardId, 1, generationOne, CancellationToken.None);
            await repository.SaveGenerationAsync(boardId, 3, generationThree, CancellationToken.None);

            var latestAtTwo = await repository.GetLatestGenerationAtOrBeforeAsync(boardId, 2, CancellationToken.None);
            var latestAtThree = await repository.GetLatestGenerationAtOrBeforeAsync(boardId, 3, CancellationToken.None);

            Assert.NotNull(latestAtTwo);
            Assert.Equal(1, latestAtTwo.Generation);
            Assert.True(generationOne.HasSameState(latestAtTwo.State));

            Assert.NotNull(latestAtThree);
            Assert.Equal(3, latestAtThree.Generation);
            Assert.True(generationThree.HasSameState(latestAtThree.State));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task SaveGenerationAsync_ReturnsExistingStoredGenerationWhenInsertIsIgnored()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            var repository = await CreateRepositoryAsync(databasePath);
            var boardId = Guid.CreateVersion7();
            var initial = BoardMatrix.ToBoardState(BlinkerHorizontal());
            var firstGeneration = GameOfLifeRules.Next(initial);
            var conflictingGeneration = BoardMatrix.ToBoardState(
            [
                [true, false, false],
                [false, false, false],
                [false, false, true]
            ]);

            await repository.CreateBoardAsync(boardId, initial, CancellationToken.None);
            var firstSave = await repository.SaveGenerationAsync(boardId, 1, firstGeneration, CancellationToken.None);
            var secondSave = await repository.SaveGenerationAsync(boardId, 1, conflictingGeneration, CancellationToken.None);
            var stored = await repository.GetGenerationAsync(boardId, 1, CancellationToken.None);

            Assert.True(firstSave.State.HasSameState(secondSave.State));
            Assert.NotNull(stored);
            Assert.True(firstGeneration.HasSameState(stored.State));
            Assert.False(conflictingGeneration.HasSameState(stored.State));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static async Task<BoardRepository> CreateRepositoryAsync(string databasePath)
    {
        var connectionFactory = new SqliteConnectionFactory(Options.Create(new DatabaseOptions
        {
            Path = databasePath
        }));
        var initializer = new DatabaseInitializer(connectionFactory);

        await initializer.InitializeAsync();

        return new BoardRepository(connectionFactory);
    }

    private static string CreateDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        File.Delete(databasePath);
        File.Delete($"{databasePath}-shm");
        File.Delete($"{databasePath}-wal");
    }

    private static bool[][] BlinkerHorizontal()
    {
        return
        [
            [false, false, false],
            [true, true, true],
            [false, false, false]
        ];
    }
}
