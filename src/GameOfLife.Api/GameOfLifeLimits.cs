namespace GameOfLife.Api;

public sealed class GameOfLifeLimits
{
    public int MaxRows { get; set; } = 200;

    public int MaxColumns { get; set; } = 200;

    public int MaxLiveCells { get; set; } = 10_000;

    public int MaxGeneration { get; set; } = 10_000;

    public int MaxConclusionAttempts { get; set; } = 10_000;
}

