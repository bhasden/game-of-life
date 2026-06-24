using GameOfLife.Domain;
using Xunit;

namespace GameOfLife.Tests.Domain;

public sealed class ConclusionDetectorTests
{
    [Fact]
    public void Observe_ReturnsStableWhenStateRepeatsImmediately()
    {
        var detector = new ConclusionDetector();
        var state = BoardMatrix.ToBoardState(
        [
            [true, true],
            [true, true]
        ]);

        Assert.Null(detector.Observe(0, state));
        var conclusion = detector.Observe(1, state);

        Assert.NotNull(conclusion);
        Assert.Equal(ConclusionReason.Stable, conclusion.Reason);
        Assert.Equal(1, conclusion.Generation);
        Assert.Equal(1, conclusion.Period);
        Assert.True(state.HasSameState(conclusion.State));
    }

    [Fact]
    public void Observe_ReturnsCycleWhenEarlierStateRepeats()
    {
        var detector = new ConclusionDetector();
        var horizontal = BoardMatrix.ToBoardState(
        [
            [false, false, false],
            [true, true, true],
            [false, false, false]
        ]);
        var vertical = GameOfLifeRules.Next(horizontal);

        Assert.Null(detector.Observe(0, horizontal));
        Assert.Null(detector.Observe(1, vertical));
        var conclusion = detector.Observe(2, horizontal);

        Assert.NotNull(conclusion);
        Assert.Equal(ConclusionReason.Cycle, conclusion.Reason);
        Assert.Equal(2, conclusion.Generation);
        Assert.Equal(2, conclusion.Period);
        Assert.True(horizontal.HasSameState(conclusion.State));
    }

    [Fact]
    public void Observe_ReturnsNullWhenStateHasNotRepeated()
    {
        var detector = new ConclusionDetector();
        var liveCell = BoardMatrix.ToBoardState(
        [
            [false, false, false],
            [false, true, false],
            [false, false, false]
        ]);
        var empty = GameOfLifeRules.Next(liveCell);

        Assert.Null(detector.Observe(0, liveCell));
        Assert.Null(detector.Observe(1, empty));
    }

    [Fact]
    public void HasSameState_ComparesDimensionsAndLiveCells()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [true, false],
            [false, true]
        ]);
        var equivalent = new BoardState(2, 2, [new Cell(1, 1), new Cell(0, 0)]);
        var differentDimensions = new BoardState(3, 2, [new Cell(0, 0), new Cell(1, 1)]);

        Assert.True(state.HasSameState(equivalent));
        Assert.False(state.HasSameState(differentDimensions));
    }
}
