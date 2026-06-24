# Agent Instructions

This repository is an interview exercise for a Conway's Game of Life REST API. Treat code quality, architectural clarity, and decision documentation as part of the deliverable.

## Project Rules

- Target `net10.0`.
- Keep the modular monolith structure:
  - `src/GameOfLife.Api`
  - `src/GameOfLife.Domain`
  - `src/GameOfLife.Infrastructure`
  - `tests/GameOfLife.Tests`
- Keep Game of Life rules in the domain project.
- Keep HTTP concerns in the API project.
- Keep SQLite, Dapper, SQL, and persistence mapping in the infrastructure project.
- Do not add authentication or authorization unless a new ADR explicitly introduces it.
- Do not add MediatR, EF Core, background workers, distributed caching, containers, or custom observability unless a new ADR explicitly introduces them.

## ADR Rules

- Decision records live in `docs/decisions`.
- Accepted ADRs are immutable.
- Existing ADRs may only receive grammar, spelling, formatting, or clarity edits.
- Any substantive change to an accepted decision requires a new dated ADR that supersedes the old decision.
- New ADR filenames must use `NNNN-short-title.md`.

## API Rules

- Use resource-based routes.
- Boolean matrices are row-major: `cells[row][column]`, row is `y`, column is `x`, and `true` means alive.
- Use `ProblemDetails` for error responses.
- Keep endpoint handlers thin; validation, orchestration, persistence, and domain rules should remain easy to test.

## Persistence Rules

- Use SQLite and Dapper.
- Keep SQL explicit and reviewable.
- Use idempotent schema initialization for this exercise.
- Persist uploaded boards and derived generation snapshots.
- Store derived snapshots indefinitely unless a new ADR introduces retention.
- Generation writes must be idempotent under concurrent requests.

## Testing Rules

- Use xUnit with native assertions only.
- Use one test project and organize tests by behavior.
- Use `WebApplicationFactory` for API integration tests.
- Use real temporary SQLite databases for persistence-sensitive tests.
- Required coverage includes:
  - Game of Life rules.
  - Finite board edge behavior.
  - Still life and oscillator conclusion detection.
  - API validation and `ProblemDetails`.
  - Restart-style persistence.

## Workflow

- Run `dotnet test` before considering the work complete.
- If the .NET SDK is unavailable, state that verification could not be executed and explain what remains to run.
- Prefer small, direct abstractions that clarify boundaries over broad framework additions.

