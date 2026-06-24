# 0001: Use ASP.NET Core Minimal APIs

## Status

Accepted

## Context

The exercise asks for a modern C# RESTful API targeting a framework greater than .NET 7. The implementation should be complete, reviewable, and easy to discuss in an interview.

## Decision

Use ASP.NET Core Minimal APIs targeting `net10.0`.

## Consequences

- Endpoint definitions stay concise and close to HTTP routing.
- The project demonstrates modern ASP.NET Core practices.
- Endpoint handlers must remain thin so domain and persistence behavior stay testable.
- OpenAPI can be generated from the API surface for review and demo.

