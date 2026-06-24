# 0006: Use Resource-Based Generation Routes

## Status

Accepted

## Context

The exercise names actions such as "get next state" and "get final state", but the API should be RESTful and resource-oriented.

## Decision

Model generations and conclusions as resources:

- `POST /boards`
- `GET /boards/{boardId}`
- `GET /boards/{boardId}/generations/{generation}`
- `GET /boards/{boardId}/conclusion`

Generation `1` is the next state.

## Consequences

- Routes are flat, predictable, and resource-based.
- The exercise requirements map cleanly to the route set.
- The API avoids action-heavy route names.
- Documentation must clearly state that generation `1` satisfies the "next state" requirement.

