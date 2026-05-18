using GymRoute.DataAccess.Data.Contexts;

namespace GymRoute.DataAccess.Data.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAllAsync(GymDbContext dbContext)
    {
        // Multiple seeders can be added here as needed
        await PlanSeeder.SeedAsync(dbContext);

        await CategorySeeder.SeedAsync(dbContext);

    }
}
