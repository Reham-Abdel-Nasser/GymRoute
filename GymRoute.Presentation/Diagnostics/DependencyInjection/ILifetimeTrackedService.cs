namespace GymRoute.Presentation.Diagnostics.DependencyInjection;

public interface ILifetimeTrackedService
{
    Guid InstanceId { get; }
    string LifetimeName { get; }
    DateTime CreatedAtUtc { get; }
}
