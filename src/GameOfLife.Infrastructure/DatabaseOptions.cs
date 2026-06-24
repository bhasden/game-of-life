namespace GameOfLife.Infrastructure;

public sealed class DatabaseOptions
{
    public string Path { get; set; } = "data/gameoflife.db";

    public string ConnectionString => $"Data Source={Path}";
}

