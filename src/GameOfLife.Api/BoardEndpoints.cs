using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GameOfLife.Api;

public static class BoardEndpoints
{
    public static RouteGroupBuilder MapBoardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var boards = endpoints.MapGroup("/boards")
            .WithTags("Boards");

        boards.MapPost("", CreateBoardAsync)
            .WithName("CreateBoard");

        boards.MapGet("/{boardId:guid}", GetBoardAsync)
            .WithName("GetBoard");

        boards.MapGet("/{boardId:guid}/generations/{generation:int}", GetGenerationAsync)
            .WithName("GetBoardGeneration");

        boards.MapGet("/{boardId:guid}/conclusion", GetConclusionAsync)
            .WithName("GetBoardConclusion");

        return boards;
    }

    private static async Task<Results<Created<BoardStateResponse>, ValidationProblem>> CreateBoardAsync(
        CreateBoardRequest request,
        BoardService service,
        IOptions<GameOfLifeLimits> limits,
        CancellationToken cancellationToken)
    {
        var validationErrors = BoardRequestValidator.Validate(request.Cells, limits.Value);
        if (validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var response = await service.CreateBoardAsync(request.Cells, cancellationToken);
        return TypedResults.Created($"/boards/{response.Id}", response);
    }

    private static async Task<Results<Ok<BoardStateResponse>, NotFound<ProblemDetails>>> GetBoardAsync(
        Guid boardId,
        BoardService service,
        CancellationToken cancellationToken)
    {
        var response = await service.GetBoardAsync(boardId, cancellationToken);
        return response is null
            ? NotFoundProblem($"Board '{boardId}' was not found.")
            : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<BoardStateResponse>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> GetGenerationAsync(
        Guid boardId,
        int generation,
        BoardService service,
        IOptions<GameOfLifeLimits> limits,
        CancellationToken cancellationToken)
    {
        if (generation < 0)
        {
            return BadRequestProblem("Generation must be greater than or equal to zero.");
        }

        if (generation > limits.Value.MaxGeneration)
        {
            return BadRequestProblem($"Generation must be less than or equal to {limits.Value.MaxGeneration}.");
        }

        var response = await service.GetGenerationAsync(boardId, generation, cancellationToken);
        return response is null
            ? NotFoundProblem($"Board '{boardId}' was not found.")
            : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<ConclusionResponse>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> GetConclusionAsync(
        Guid boardId,
        int? maxAttempts,
        BoardService service,
        IOptions<GameOfLifeLimits> limits,
        CancellationToken cancellationToken)
    {
        var resolvedMaxAttempts = maxAttempts ?? 1000;

        if (resolvedMaxAttempts <= 0)
        {
            return BadRequestProblem("maxAttempts must be positive.");
        }

        if (resolvedMaxAttempts > limits.Value.MaxConclusionAttempts)
        {
            return BadRequestProblem($"maxAttempts must be less than or equal to {limits.Value.MaxConclusionAttempts}.");
        }

        var result = await service.GetConclusionAsync(boardId, resolvedMaxAttempts, cancellationToken);
        return result.Status switch
        {
            ConclusionLookupStatus.Found => TypedResults.Ok(result.Conclusion),
            ConclusionLookupStatus.BoardNotFound => NotFoundProblem($"Board '{boardId}' was not found."),
            _ => TypedResults.UnprocessableEntity(new ProblemDetails
            {
                Title = "No conclusion found.",
                Detail = $"The board did not reach a stable or cyclic state within {resolvedMaxAttempts} attempts.",
                Status = StatusCodes.Status422UnprocessableEntity
            })
        };
    }

    private static BadRequest<ProblemDetails> BadRequestProblem(string detail)
    {
        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid request.",
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static NotFound<ProblemDetails> NotFoundProblem(string detail)
    {
        return TypedResults.NotFound(new ProblemDetails
        {
            Title = "Resource not found.",
            Detail = detail,
            Status = StatusCodes.Status404NotFound
        });
    }
}
