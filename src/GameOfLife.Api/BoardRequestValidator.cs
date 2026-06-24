namespace GameOfLife.Api;

public static class BoardRequestValidator
{
    public static Dictionary<string, string[]> Validate(bool[][]? cells, GameOfLifeLimits limits)
    {
        var errors = new Dictionary<string, string[]>();

        if (cells is null)
        {
            errors["cells"] = ["The cells matrix is required."];
            return errors;
        }

        if (cells.Length == 0)
        {
            errors["cells"] = ["The cells matrix must contain at least one row."];
            return errors;
        }

        if (cells[0] is null || cells[0].Length == 0)
        {
            errors["cells"] = ["The cells matrix must contain at least one column."];
            return errors;
        }

        var width = cells[0].Length;
        var liveCellCount = 0;

        if (cells.Length > limits.MaxRows)
        {
            errors["cells"] = [$"The board must contain at most {limits.MaxRows} rows."];
        }

        if (width > limits.MaxColumns)
        {
            errors["cells[0]"] = [$"The board must contain at most {limits.MaxColumns} columns."];
        }

        for (var row = 0; row < cells.Length; row++)
        {
            if (cells[row] is null)
            {
                errors[$"cells[{row}]"] = ["Rows must not be null."];
                continue;
            }

            if (cells[row].Length != width)
            {
                errors[$"cells[{row}]"] = ["All rows must contain the same number of columns."];
                continue;
            }

            for (var column = 0; column < cells[row].Length; column++)
            {
                if (cells[row][column])
                {
                    liveCellCount++;
                }
            }
        }

        if (liveCellCount > limits.MaxLiveCells)
        {
            errors["cells"] = [$"The board must contain at most {limits.MaxLiveCells} live cells."];
        }

        return errors;
    }
}

