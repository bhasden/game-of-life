namespace GameOfLife.Api;

public sealed record CreateBoardRequest(bool[][] Cells);

public sealed record BoardStateResponse(
    Guid Id,
    int Generation,
    int Height,
    int Width,
    bool[][] Cells);

public sealed record ConclusionResponse(
    Guid Id,
    string Reason,
    int Generation,
    int? Period,
    int Height,
    int Width,
    bool[][] Cells);

