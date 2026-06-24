namespace GameOfLife.Domain;

public sealed class ConclusionDetector
{
    private readonly Dictionary<string, List<ObservedState>> _seen = [];
    private int? _lastObservedGeneration;

    public Conclusion? Observe(int generation, BoardState state)
    {
        if (_lastObservedGeneration is not null && generation <= _lastObservedGeneration)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generations must be observed in increasing order.");
        }

        ArgumentNullException.ThrowIfNull(state);

        var hash = state.ComputeHash();
        if (_seen.TryGetValue(hash, out var candidates))
        {
            foreach (var candidate in candidates)
            {
                if (state.HasSameState(candidate.State))
                {
                    var period = generation - candidate.Generation;
                    var reason = period == 1 ? ConclusionReason.Stable : ConclusionReason.Cycle;
                    _lastObservedGeneration = generation;
                    return new Conclusion(reason, generation, period, state);
                }
            }
        }
        else
        {
            candidates = [];
            _seen[hash] = candidates;
        }

        candidates.Add(new ObservedState(generation, state));
        _lastObservedGeneration = generation;

        return null;
    }

    private sealed record ObservedState(int Generation, BoardState State);
}
