# 0003: Use SQLite and Dapper

## Status

Accepted

## Context

The service must survive restarts while retaining uploaded board state. The persistence approach should be durable, simple to demo, and SQL-first without adding ORM ceremony.

## Decision

Use SQLite for durable local storage and Dapper for explicit SQL mapping.

## Consequences

- The application can run locally without external infrastructure.
- SQL remains visible and easy to review.
- Dapper avoids EF Core migration and tracking behavior that is not needed for this exercise.
- SQLite is appropriate for the exercise scale, but a production multi-instance deployment would likely require a server database.

