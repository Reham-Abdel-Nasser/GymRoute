using GymRoute.BusinessLogic.Interfaces;
using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.BusinessLogic;

/// <summary>
/// Forwards to GymDbContext so BusinessLogic can depend on IGymDbContext
/// without DataAccess referencing BusinessLogic (avoids circular projects).
/// </summary>
public sealed class GymDbContextAdapter(GymDbContext context) : IGymDbContext
{
    public DbSet<Plan> Plans => context.Plans;
    public DbSet<Category> Categories => context.Categories;
    public DbSet<User> Users => context.Users;
    public DbSet<Session> Sessions => context.Sessions;
    public DbSet<MemberShip> MemberShips => context.MemberShips;
    public DbSet<Booking> Bookings => context.Bookings;
    public DbSet<HealthRecord> HealthRecords => context.HealthRecords;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
