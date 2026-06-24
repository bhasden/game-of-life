# 0008: Use xUnit with Native Assertions

## Status

Accepted

## Context

The project needs automated tests for domain rules, API contracts, validation, and persistence. The test stack should be familiar and dependency-light.

## Decision

Use xUnit with native framework assertions only. Do not add FluentAssertions.

## Consequences

- Tests use a common .NET framework with good ASP.NET Core integration.
- Assertions remain dependency-light and familiar.
- Matrix comparisons may use small local helper methods that call native assertions.
- Test readability depends on careful naming and focused helper methods.

