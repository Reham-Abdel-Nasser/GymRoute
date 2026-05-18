namespace GymRoute.Presentation.Diagnostics.DependencyInjection;

/// <summary>
/// Scoped per HTTP request. Resolves each tracked service multiple times
/// to demonstrate lifetime behavior inside one request scope.
/// </summary>
public sealed class LifetimeComparisonService(IServiceProvider serviceProvider)
{
    public Guid ServiceInstanceId { get; } = Guid.NewGuid();

    public LifetimeComparisonResult Compare()
    {
        var transientIds = ResolveMany<ITransientTrackedService>(5);
        var scopedIds = ResolveMany<IScopedTrackedService>(5);
        var singletonIds = ResolveMany<ISingletonTrackedService>(5);

        return new LifetimeComparisonResult
        {
            HttpRequestId = Guid.NewGuid(),
            ObservedAtUtc = DateTime.UtcNow,
            Transient = BuildObservation("Transient", transientIds,
                "New instance on every resolve — even within the same HTTP request."),
            Scoped = BuildObservation("Scoped", scopedIds,
                "Same instance for every resolve within this HTTP request."),
            Singleton = BuildObservation("Singleton", singletonIds,
                "Same instance for every resolve for the entire application lifetime.")
        };
    }

    private List<Guid> ResolveMany<T>(int count) where T : ILifetimeTrackedService
    {
        var ids = new List<Guid>(count);
        for (var i = 0; i < count; i++)
            ids.Add(serviceProvider.GetRequiredService<T>().InstanceId);
        return ids;
    }

    private static LifetimeObservation BuildObservation(
        string lifetimeName,
        IReadOnlyList<Guid> ids,
        string whenReused)
    {
        var allSame = ids.Distinct().Count() == 1;
        var conclusion = allSame
            ? whenReused
            : $"{ids.Distinct().Count()} distinct instance(s) from {ids.Count} resolves — objects were recreated.";

        return new LifetimeObservation
        {
            LifetimeName = lifetimeName,
            ResolvedInstanceIds = ids,
            AllSameInstance = allSame,
            Conclusion = conclusion
        };
    }
}
