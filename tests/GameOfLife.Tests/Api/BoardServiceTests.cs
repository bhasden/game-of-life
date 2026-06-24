using GameOfLife.Api;
using GameOfLife.Domain;
using GameOfLife.Infrastructure;
using Xunit;

namespace GameOfLife.Tests.Api;

public sealed class BoardServiceTests
{
    [Fact]
    public async Task GetConclusionAsync_WalksForwardLinearly()
    {
        var boardId = Guid.CreateVersion7();
        var repository = new CountingBoardRepository(boardId, BoardMatrix.ToBoardState(BlinkerHorizontal()));
        var service = new BoardService(repository);

        var result = await service.GetConclusionAsync(boardId, maxAttempts: 3, CancellationToken.None);

        Assert.Equal(ConclusionLookupStatus.Found, result.Status);
        Assert.NotNull(result.Conclusion);
        Assert.Equal("cycle", result.Conclusion.Reason);
        Assert.Equal(2, result.Conclusion.Generation);
        Assert.Equal(2, result.Conclusion.Period);
        Assert.Equal(1, repository.GetLatestGenerationAtOrBeforeCalls);
        Assert.Equal(2, repository.GetGenerationCalls);
        Assert.Equal([1, 2], repository.SavedGenerations);
    }

    [Fact]
    public async Task GetGenerationAsync_StartsFromLatestCachedGeneration()
    {
        var boardId = Guid.CreateVersion7();
        var repository = new CountingBoardRepository(boardId, BoardMatrix.ToBoardState(BlinkerHorizontal()));
        repository.SetGeneration(4, BoardMatrix.ToBoardState(BlinkerHorizontal()));
        var service = new BoardService(repository);

        var response = await service.GetGenerationAsync(boardId, generation: 6, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(6, response.Generation);
        MatrixAssert.Equal(BlinkerHorizontal(), response.Cells);
        Assert.Equal(1, repository.GetLatestGenerationAtOrBeforeCalls);
        Assert.Equal([5, 6], repository.SavedGenerations);
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

    private sealed class CountingBoardRepository(Guid boardId, BoardState initialState) : IBoardRepository
    {
        private readonly Dictionary<int, StoredGeneration> _generations = new()
        {
            [0] = ToStoredGeneration(0, initialState)
        };

        public int GetGenerationCalls { get; private set; }

        public int GetLatestGenerationAtOrBeforeCalls { get; private set; }

        public List<int> SavedGenerations { get; } = [];

        public Task CreateBoardAsync(Guid boardId, BoardState initialState, CancellationToken cancellationToken)
        {
            _generations[0] = ToStoredGeneration(0, initialState);
            return Task.CompletedTask;
        }

        public Task<BoardRecord?> GetBoardAsync(Guid requestedBoardId, CancellationToken cancellationToken)
        {
            if (requestedBoardId != boardId)
            {
                return Task.FromResult<BoardRecord?>(null);
            }

            var initial = _generations[0].State;
            return Task.FromResult<BoardRecord?>(new BoardRecord(boardId, initial.Height, initial.Width, DateTimeOffset.UtcNow));
        }

        public Task<StoredGeneration?> GetGenerationAsync(Guid requestedBoardId, int generation, CancellationToken cancellationToken)
        {
            GetGenerationCalls++;

            if (requestedBoardId != boardId)
            {
                return Task.FromResult<StoredGeneration?>(null);
            }

            _generations.TryGetValue(generation, out var storedGeneration);
            return Task.FromResult(storedGeneration);
        }

        public Task<StoredGeneration?> GetLatestGenerationAtOrBeforeAsync(Guid requestedBoardId, int generation, CancellationToken cancellationToken)
        {
            GetLatestGenerationAtOrBeforeCalls++;

            if (requestedBoardId != boardId)
            {
                return Task.FromResult<StoredGeneration?>(null);
            }

            var candidate = _generations
                .Where(pair => pair.Key <= generation)
                .OrderByDescending(pair => pair.Key)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            return Task.FromResult(candidate);
        }

        public Task<StoredGeneration> SaveGenerationAsync(Guid requestedBoardId, int generation, BoardState state, CancellationToken cancellationToken)
        {
            SavedGenerations.Add(generation);

            if (requestedBoardId != boardId)
            {
                throw new InvalidOperationException("Unexpected board id.");
            }

            if (!_generations.TryGetValue(generation, out var storedGeneration))
            {
                storedGeneration = ToStoredGeneration(generation, state);
                _generations[generation] = storedGeneration;
            }

            return Task.FromResult(storedGeneration);
        }

        public void SetGeneration(int generation, BoardState state)
        {
            _generations[generation] = ToStoredGeneration(generation, state);
        }

        private static StoredGeneration ToStoredGeneration(int generation, BoardState state)
        {
            return new StoredGeneration(generation, state, state.ComputeHash(), DateTimeOffset.UtcNow);
        }
    }
}
