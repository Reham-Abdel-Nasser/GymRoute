//using GymManagementSystem.Interceptors;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.DataAccess.Data.Contexts;

// GymDbContext unit of work
public class GymDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);

        modelBuilder.Entity<User>(t =>
        {
            t.HasDiscriminator<string>("UserType")
            .HasValue<Member>("Member")
            .HasValue<Trainer>("Trainer");

            t.HasQueryFilter(u => !u.IsDeleted);

        });
    }

    public DbSet<Plan> Plans { get; set; }  // repository pattern
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<MemberShip> MemberShips { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<HealthRecord> HealthRecords { get; set; }
}