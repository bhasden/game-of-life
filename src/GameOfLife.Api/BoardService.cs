using GameOfLife.Domain;
using GameOfLife.Infrastructure;

namespace GameOfLife.Api;

public sealed class BoardService(IBoardRepository repository)
{
    public async Task<BoardStateResponse> CreateBoardAsync(bool[][] cells, CancellationToken cancellationToken)
    {
        var boardId = Guid.CreateVersion7();
        var initialState = BoardMatrix.ToBoardState(cells);

        await repository.CreateBoardAsync(boardId, initialState, cancellationToken);
        return new BoardStateResponse(boardId, 0, initialState.Height, initialState.Width, initialState.ToMatrix());
    }

    public async Task<BoardStateResponse?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await repository.GetBoardAsync(boardId, cancellationToken);
        if (board is null)
        {
            return null;
        }

        var generation = await repository.GetGenerationAsync(boardId, 0, cancellationToken);
        return generation is null
            ? null
            : new BoardStateResponse(
                boardId,
                generation.Generation,
                generation.State.Height,
                generation.State.Width,
                generation.State.ToMatrix());
    }

    public async Task<BoardStateResponse?> GetGenerationAsync(Guid boardId, int generation, CancellationToken cancellationToken)
    {
        var storedGeneration = await GetOrCreateGenerationAsync(boardId, generation, cancellationToken);
        return storedGeneration is null
            ? null
            : new BoardStateResponse(
                boardId,
                storedGeneration.Generation,
                storedGeneration.State.Height,
                storedGeneration.State.Width,
                storedGeneration.State.ToMatrix());
    }

    public async Task<ConclusionLookupResult> GetConclusionAsync(Guid boardId, int maxAttempts, CancellationToken cancellationToken)
    {
        var current = await GetOrCreateGenerationAsync(boardId, 0, cancellationToken);
        if (current is null)
        {
            return ConclusionLookupResult.NotFound();
        }

        var detector = new ConclusionDetector();
        detector.Observe(current.Generation, current.State);

        for (var generation = 1; generation <= maxAttempts; generation++)
        {
            var storedGeneration = await repository.GetGenerationAsync(boardId, generation, cancellationToken);
            if (storedGeneration is null)
            {
                var nextState = GameOfLifeRules.Next(current.State);
                storedGeneration = await repository.SaveGenerationAsync(boardId, generation, nextState, cancellationToken);
            }

            current = storedGeneration;
            var conclusion = detector.Observe(current.Generation, current.State);
            if (conclusion is not null)
            {
                return ConclusionLookupResult.Found(new ConclusionResponse(
                    boardId,
                    conclusion.Reason.ToString().ToLowerInvariant(),
                    conclusion.Generation,
                    conclusion.Period,
                    conclusion.State.Height,
                    conclusion.State.Width,
                    conclusion.State.ToMatrix()));
            }
        }

        return ConclusionLookupResult.NoConclusion();
    }

    private async Task<StoredGeneration?> GetOrCreateGenerationAsync(Guid boardId, int targetGeneration, CancellationToken cancellationToken)
    {
        var current = await repository.GetLatestGenerationAtOrBeforeAsync(boardId, targetGeneration, cancellationToken);
        if (current is null)
        {
            return null;
        }

        for (var generation = current.Generation + 1; generation <= targetGeneration; generation++)
        {
            var nextState = GameOfLifeRules.Next(current.State);
            current = await repository.SaveGenerationAsync(boardId, generation, nextState, cancellationToken);
        }

        return current;
    }
}

public enum ConclusionLookupStatus
{
    Found,
    BoardNotFound,
    NoConclusion
}

public sealed record ConclusionLookupResult(ConclusionLookupStatus Status, ConclusionResponse? Conclusion)
{
    public static ConclusionLookupResult Found(ConclusionResponse conclusion)
    {
        return new ConclusionLookupResult(ConclusionLookupStatus.Found, conclusion);
    }

    public static ConclusionLookupResult NotFound()
    {
        return new ConclusionLookupResult(ConclusionLookupStatus.BoardNotFound, null);
    }

    public static ConclusionLookupResult NoConclusion()
    {
        return new ConclusionLookupResult(ConclusionLookupStatus.NoConclusion, null);
    }
}
