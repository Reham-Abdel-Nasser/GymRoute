using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.BusinessLogic.Interfaces;

public interface IGymDbContext
{
    DbSet<Plan> Plans { get; }
    DbSet<Category> Categories { get; }
    DbSet<User> Users { get; }
    DbSet<Session> Sessions { get; }
    DbSet<MemberShip> MemberShips { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<HealthRecord> HealthRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
