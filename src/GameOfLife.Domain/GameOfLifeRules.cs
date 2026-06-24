namespace GameOfLife.Domain;

public static class GameOfLifeRules
{
    public static BoardState Next(BoardState current)
    {
        ArgumentNullException.ThrowIfNull(current);

        var liveCells = current.LiveCells.ToHashSet();
        var neighborCounts = new Dictionary<Cell, int>();

        foreach (var cell in liveCells)
        {
            for (var rowDelta = -1; rowDelta <= 1; rowDelta++)
            {
                for (var columnDelta = -1; columnDelta <= 1; columnDelta++)
                {
                    if (rowDelta == 0 && columnDelta == 0)
                    {
                        continue;
                    }

                    var neighbor = new Cell(cell.Row + rowDelta, cell.Column + columnDelta);
                    if (neighbor.Row < 0 || neighbor.Row >= current.Height ||
                        neighbor.Column < 0 || neighbor.Column >= current.Width)
                    {
                        continue;
                    }

                    neighborCounts[neighbor] = neighborCounts.GetValueOrDefault(neighbor) + 1;
                }
            }
        }

        var nextLiveCells = new List<Cell>();
        foreach (var (cell, count) in neighborCounts)
        {
            var isAlive = liveCells.Contains(cell);
            if ((isAlive && count is 2 or 3) || (!isAlive && count == 3))
            {
                nextLiveCells.Add(cell);
            }
        }

        return new BoardState(current.Height, current.Width, nextLiveCells);
    }
}

