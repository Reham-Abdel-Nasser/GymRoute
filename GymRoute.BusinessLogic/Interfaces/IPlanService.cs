using GymRoute.DataAccess.Entities;

namespace GymRoute.BusinessLogic.Interfaces;

public interface IPlanService
{
    Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Plan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Plan> CreateAsync(Plan plan, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
