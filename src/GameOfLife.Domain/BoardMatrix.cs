namespace GameOfLife.Domain;

public static class BoardMatrix
{
    public static BoardState ToBoardState(bool[][] cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        var liveCells = new List<Cell>();
        var height = cells.Length;
        var width = height == 0 ? 0 : cells[0].Length;

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                if (cells[row][column])
                {
                    liveCells.Add(new Cell(row, column));
                }
            }
        }

        return new BoardState(height, width, liveCells);
    }
}

