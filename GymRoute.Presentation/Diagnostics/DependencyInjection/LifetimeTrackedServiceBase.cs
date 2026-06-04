namespace GymRoute.Presentation.Diagnostics.DependencyInjection;

public abstract class LifetimeTrackedServiceBase(string lifetimeName) : ILifetimeTrackedService
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public string LifetimeName { get; } = lifetimeName;
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
}

public sealed class TransientTrackedService()
    : LifetimeTrackedServiceBase("Transient"), ITransientTrackedService;

public sealed class ScopedTrackedService()
    : LifetimeTrackedServiceBase("Scoped"), IScopedTrackedService;

public sealed class SingletonTrackedService()
    : LifetimeTrackedServiceBase("Singleton"), ISingletonTrackedService;
