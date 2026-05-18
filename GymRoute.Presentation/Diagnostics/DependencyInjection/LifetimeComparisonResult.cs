namespace GymRoute.Presentation.Diagnostics.DependencyInjection;

public sealed class LifetimeComparisonResult
{
    public required Guid HttpRequestId { get; init; }
    public required DateTime ObservedAtUtc { get; init; }

    public required LifetimeObservation Transient { get; init; }
    public required LifetimeObservation Scoped { get; init; }
    public required LifetimeObservation Singleton { get; init; }
}

public sealed class LifetimeObservation
{
    public required string LifetimeName { get; init; }
    public required IReadOnlyList<Guid> ResolvedInstanceIds { get; init; }
    public required bool AllSameInstance { get; init; }
    public required string Conclusion { get; init; }
}
