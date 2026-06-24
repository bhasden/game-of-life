# 0004: Persist Derived Generations as a Cache

## Status

Accepted

## Context

The API computes future board states. Recomputing every generation from the initial state after a restart would be correct but wasteful. The service also needs a credible restart-safety story.

## Decision

Persist every generated board snapshot indefinitely. Treat generated snapshots as a durable cache derived from generation `0`.

## Consequences

- Repeated requests for the same generation are fast.
- Previously computed progress survives restarts.
- Concurrent requests can write snapshots idempotently.
- Storage grows with requested generations; production systems would need retention or compaction policy.

