using GameOfLife.Domain;
using Xunit;

namespace GameOfLife.Tests.Domain;

public sealed class GameOfLifeRulesTests
{
    [Fact]
    public void Next_KillsLiveCellWithFewerThanTwoNeighbors()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [false, false, false],
            [false, true, false],
            [false, false, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        Assert.Empty(next.LiveCells);
    }

    [Fact]
    public void Next_KillsLiveCellWithMoreThanThreeNeighbors()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [true, true, true],
            [true, true, false],
            [false, false, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        Assert.DoesNotContain(new Cell(1, 1), next.LiveCells);
    }

    [Fact]
    public void Next_PreservesLiveCellWithTwoOrThreeNeighbors()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [true, true],
            [true, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        Assert.Contains(new Cell(0, 0), next.LiveCells);
    }

    [Fact]
    public void Next_CreatesLiveCellWithExactlyThreeNeighbors()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [true, true],
            [true, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        Assert.Contains(new Cell(1, 1), next.LiveCells);
    }

    [Fact]
    public void Next_UsesFiniteEdges()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [true, true],
            [true, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        MatrixAssert.Equal(
        [
            [true, true],
            [true, true]
        ], next.ToMatrix());
    }

    [Fact]
    public void Next_PreservesBlockStillLife()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [false, false, false, false],
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        MatrixAssert.Equal(state.ToMatrix(), next.ToMatrix());
    }

    [Fact]
    public void Next_OscillatesBlinker()
    {
        var state = BoardMatrix.ToBoardState(
        [
            [false, false, false],
            [true, true, true],
            [false, false, false]
        ]);

        var next = GameOfLifeRules.Next(state);

        MatrixAssert.Equal(
        [
            [false, true, false],
            [false, true, false],
            [false, true, false]
        ], next.ToMatrix());
    }
}
