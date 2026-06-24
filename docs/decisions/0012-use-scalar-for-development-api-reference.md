# 0012: Use Scalar for Development API Reference

## Status

Accepted

## Context

The API already generates an OpenAPI document through `Microsoft.AspNetCore.OpenApi`, but an OpenAPI JSON document alone is not the same as an interactive testing UI. The project should be easy to exercise during interview review without introducing controllers or changing the Minimal API architecture.

## Decision

Use Scalar.AspNetCore to expose a development-only interactive API reference backed by the generated OpenAPI document. Keep OpenAPI document generation through `Microsoft.AspNetCore.OpenApi`.

## Consequences

- Developers and reviewers can explore and invoke endpoints from a browser in development.
- The generated OpenAPI JSON remains available for tools and clients.
- The project does not add Swashbuckle or Swagger UI.
- The interactive API reference is not exposed outside the `Development` environment.

