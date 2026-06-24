using System.Net;
using System.Net.Http.Json;
using GameOfLife.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GameOfLife.Tests.Api;

public sealed class ApiContractTests
{
    [Fact]
    public async Task PostBoards_CreatesBoardAndReturnsInitialState()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/boards", new CreateBoardRequest(
        [
            [false, true, false],
            [false, true, false],
            [false, true, false]
        ]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var board = await response.Content.ReadFromJsonAsync<BoardStateResponse>();
        Assert.NotNull(board);
        Assert.NotEqual(Guid.Empty, board.Id);
        Assert.Equal(3, board.Height);
        Assert.Equal(3, board.Width);
        Assert.Equal(0, board.Generation);
    }

    [Fact]
    public async Task GetBoard_ReturnsPersistedInitialState()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, Block());

        var board = await client.GetFromJsonAsync<BoardStateResponse>($"/boards/{created.Id}");

        Assert.NotNull(board);
        Assert.Equal(created.Id, board.Id);
        Assert.Equal(0, board.Generation);
        MatrixAssert.Equal(Block(), board.Cells);
    }

    [Fact]
    public async Task GetGenerationOne_ReturnsNextState()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, BlinkerHorizontal());

        var next = await client.GetFromJsonAsync<BoardStateResponse>($"/boards/{created.Id}/generations/1");

        Assert.NotNull(next);
        MatrixAssert.Equal(BlinkerVertical(), next.Cells);
    }

    [Fact]
    public async Task GetGeneration_ReturnsArbitraryFutureState()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, BlinkerHorizontal());

        var generationTwo = await client.GetFromJsonAsync<BoardStateResponse>($"/boards/{created.Id}/generations/2");

        Assert.NotNull(generationTwo);
        MatrixAssert.Equal(BlinkerHorizontal(), generationTwo.Cells);
    }

    [Fact]
    public async Task GetConclusion_ReturnsStableForStillLife()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, Block());

        var conclusion = await client.GetFromJsonAsync<ConclusionResponse>($"/boards/{created.Id}/conclusion?maxAttempts=3");

        Assert.NotNull(conclusion);
        Assert.Equal("stable", conclusion.Reason);
        Assert.Equal(1, conclusion.Period);
        MatrixAssert.Equal(Block(), conclusion.Cells);
    }

    [Fact]
    public async Task GetConclusion_ReturnsCycleForOscillator()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, BlinkerHorizontal());

        var conclusion = await client.GetFromJsonAsync<ConclusionResponse>($"/boards/{created.Id}/conclusion?maxAttempts=3");

        Assert.NotNull(conclusion);
        Assert.Equal("cycle", conclusion.Reason);
        Assert.Equal(2, conclusion.Period);
        MatrixAssert.Equal(BlinkerHorizontal(), conclusion.Cells);
    }

    [Fact]
    public async Task GetConclusion_ReturnsUnprocessableEntityWhenAttemptsAreTooLow()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, BlinkerHorizontal());

        var response = await client.GetAsync($"/boards/{created.Id}/conclusion?maxAttempts=1");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(422, problem.Status);
    }

    [Fact]
    public async Task PostBoards_ReturnsValidationProblemForJaggedMatrix()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/boards", new CreateBoardRequest(
        [
            [true, false],
            [true]
        ]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("cells[1]", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetBoard_ReturnsNotFoundForUnknownBoard()
    {
        using var factory = new GameOfLifeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/boards/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGeneration_RejectsConfiguredLimit()
    {
        using var factory = new GameOfLifeApiFactory(settings: new Dictionary<string, string?>
        {
            ["GameOfLife:MaxGeneration"] = "1"
        });
        using var client = factory.CreateClient();
        var created = await CreateBoardAsync(client, Block());

        var response = await client.GetAsync($"/boards/{created.Id}/generations/2");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PersistedBoardSurvivesApplicationRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        Guid boardId;

        using (var factory = new GameOfLifeApiFactory(databasePath))
        {
            using var client = factory.CreateClient();
            var created = await CreateBoardAsync(client, BlinkerHorizontal());
            boardId = created.Id;

            var generationOne = await client.GetFromJsonAsync<BoardStateResponse>($"/boards/{boardId}/generations/1");
            Assert.NotNull(generationOne);
            MatrixAssert.Equal(BlinkerVertical(), generationOne.Cells);
        }

        using (var factory = new GameOfLifeApiFactory(databasePath))
        {
            using var client = factory.CreateClient();
            var generationOne = await client.GetFromJsonAsync<BoardStateResponse>($"/boards/{boardId}/generations/1");

            Assert.NotNull(generationOne);
            MatrixAssert.Equal(BlinkerVertical(), generationOne.Cells);
        }
    }

    private static async Task<BoardStateResponse> CreateBoardAsync(HttpClient client, bool[][] cells)
    {
        var response = await client.PostAsJsonAsync("/boards", new CreateBoardRequest(cells));
        response.EnsureSuccessStatusCode();

        var board = await response.Content.ReadFromJsonAsync<BoardStateResponse>();
        Assert.NotNull(board);
        return board;
    }

    private static bool[][] Block()
    {
        return
        [
            [false, false, false, false],
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
        ];
    }

    private static bool[][] BlinkerHorizontal()
    {
        return
        [
            [false, false, false],
            [true, true, true],
            [false, false, false]
        ];
    }

    private static bool[][] BlinkerVertical()
    {
        return
        [
            [false, true, false],
            [false, true, false],
            [false, true, false]
        ];
    }
}
