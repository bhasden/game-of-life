using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace GameOfLife.Tests.Api;

internal sealed class GameOfLifeApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath;
    private readonly Dictionary<string, string?> _settings;

    public GameOfLifeApiFactory(string? databasePath = null, Dictionary<string, string?>? settings = null)
    {
        _databasePath = databasePath ?? Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        _settings = settings ?? [];
        _settings["Database:Path"] = _databasePath;
    }

    public string DatabasePath => _databasePath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(_settings);
        });
    }
}

