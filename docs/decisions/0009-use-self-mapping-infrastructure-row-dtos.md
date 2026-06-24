# 0009: Use Self-Mapping Infrastructure Row DTOs

## Status

Accepted

## Context

The repository reads SQLite rows through Dapper and maps those rows into repository-facing records. Keeping all mapping as repository helper methods made the repository harder to scan, but moving persistence mapping onto domain models would make the domain aware of SQLite, Dapper, JSON storage, and row-shape details.

## Decision

Allow private or internal infrastructure row DTOs to map themselves to repository-facing models when the mapping is persistence-specific and localized. Domain models must remain ignorant of SQL, Dapper, SQLite, and JSON storage concerns.

## Consequences

- Repository methods read more directly: execute SQL, get a row, return the mapped result.
- Persistence mapping stays inside `GameOfLife.Infrastructure`.
- Domain models remain persistence-ignorant.
- Row DTOs remain implementation details and should not become public contracts.
- If mapping grows more complex, a future ADR should introduce dedicated infrastructure mapper classes.

