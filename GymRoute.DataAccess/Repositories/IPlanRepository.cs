using GymRoute.DataAccess.Entities;

namespace GymRoute.DataAccess.Repositories;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync();

    Task<Plan?> GetByIdAsync(int id);

    void Add(Plan plan);

    void Update(Plan plan);

    void Delete(Plan plan);

    Task<int> SaveChangesAsync();
}
