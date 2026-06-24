# 0007: Use a Modular Monolith Structure

## Status

Accepted

## Context

The exercise is small enough for a single deployable API but substantial enough to benefit from clear boundaries. Submission should show separation of concerns without framework-heavy architecture.

## Decision

Use a modular monolith with separate API, Domain, Infrastructure, and Tests projects.

## Consequences

- Domain rules remain independent of HTTP and SQLite.
- Persistence details are isolated.
- Tests can target domain behavior and full HTTP behavior.
- The solution avoids mediator/CQRS ceremony that is not necessary for this scope.

