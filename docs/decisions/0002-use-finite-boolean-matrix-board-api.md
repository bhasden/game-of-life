# 0002: Use a Finite Boolean Matrix Board API

## Status

Accepted

## Context

Conway's Game of Life can be modeled as an infinite sparse grid or as a finite board. The exercise does not specify either interpretation. Interviewers and academic examples often represent boards as matrices.

## Decision

Expose boards as finite boolean matrices. The matrix is row-major: `cells[row][column]`, row is `y`, column is `x`, and `true` means alive. Cells outside the uploaded dimensions are always dead.

## Consequences

- Payloads are easy to read, test, and demonstrate.
- Boundary behavior is explicit and deterministic.
- Very large sparse boards are less efficient than coordinate-list payloads.
- The implementation normalizes matrices internally to sorted live-cell coordinates for hashing and storage.

