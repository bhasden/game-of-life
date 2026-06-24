using System.Security.Cryptography;
using System.Text;

namespace GameOfLife.Domain;

public sealed class BoardState
{
    private readonly Cell[] _liveCells;

    public BoardState(int height, int width, IEnumerable<Cell> liveCells)
    {
        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Board height must be positive.");
        }

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Board width must be positive.");
        }

        Height = height;
        Width = width;

        _liveCells = [.. liveCells
            .Distinct()
            .OrderBy(cell => cell.Row)
            .ThenBy(cell => cell.Column)];

        foreach (var cell in _liveCells)
        {
            if (cell.Row < 0 || cell.Row >= Height || cell.Column < 0 || cell.Column >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(liveCells), "Live cells must be inside board bounds.");
            }
        }
    }

    public int Height { get; }

    public int Width { get; }

    public IReadOnlyList<Cell> LiveCells => _liveCells;

    public bool HasSameState(BoardState other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Height != other.Height || Width != other.Width || _liveCells.Length != other._liveCells.Length)
        {
            return false;
        }

        for (var index = 0; index < _liveCells.Length; index++)
        {
            if (_liveCells[index] != other._liveCells[index])
            {
                return false;
            }
        }

        return true;
    }

    public bool[][] ToMatrix()
    {
        var matrix = new bool[Height][];

        for (var row = 0; row < Height; row++)
        {
            matrix[row] = new bool[Width];
        }

        foreach (var cell in _liveCells)
        {
            matrix[cell.Row][cell.Column] = true;
        }

        return matrix;
    }

    public string ComputeHash()
    {
        var builder = new StringBuilder();
        builder.Append(Height).Append('x').Append(Width).Append('|');

        foreach (var cell in _liveCells)
        {
            builder.Append(cell.Row).Append(',').Append(cell.Column).Append(';');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes);
    }
}
