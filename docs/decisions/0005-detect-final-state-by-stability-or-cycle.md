# 0005: Detect Final State by Stability or Cycle

## Status

Accepted

## Context

The exercise asks for a final state and an error if the board does not conclude after a configured number of attempts. Game of Life patterns may become still lifes or oscillators.

## Decision

Treat a board as concluded when it reaches a stable state or repeats a previously seen state. Stable states are cycles with period `1`; longer repeats are returned as cycles with a period.

## Consequences

- Still lifes and oscillators are both handled correctly.
- The API can explain why computation concluded.
- Spaceships on a finite board eventually die, stabilize, or cycle according to the finite boundary.
- Detection requires deterministic state hashing.

