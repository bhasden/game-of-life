namespace GameOfLife.Domain;

public enum ConclusionReason
{
    Stable,
    Cycle
}

public sealed record Conclusion(ConclusionReason Reason, int Generation, int? Period, BoardState State);

