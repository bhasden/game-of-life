# 0010: Use a Provider-Neutral Database Connection Factory

## Status

Accepted

## Context

The application uses SQLite and Dapper for persistence. The original connection factory interface was named `ISqliteConnectionFactory` and returned `SqliteConnection`, which made repository and initialization code depend directly on SQLite-specific connection types.

SQLite remains the accepted database for this exercise, but repository code should not require SQLite-specific signatures to open and use database connections.

## Decision

Use a provider-neutral `IDbConnectionFactory` interface that returns `DbConnection`. Keep `SqliteConnectionFactory` as the concrete infrastructure implementation that creates `Microsoft.Data.Sqlite.SqliteConnection` instances and performs SQLite-specific setup.

## Consequences

- Repository and database initialization code depend on `DbConnection` rather than SQLite-specific connection types.
- SQLite remains the selected database technology by existing ADR.
- The composition root still binds `IDbConnectionFactory` to `SqliteConnectionFactory`.
- Future database provider changes would still require SQL, schema, and behavior review, but repository signatures would not need to change only because connection creation changed.

