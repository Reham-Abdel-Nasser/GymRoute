using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.DataAccess.Data.Seeder;

public static class PlanSeeder
{
    // static => static instances
    // method Injection
    public static async Task SeedAsync(GymDbContext dbContext)
    {
        

        bool hasAnyPlans = await dbContext.Plans.AnyAsync();

        if (hasAnyPlans)
        {
            return;
        }

        List<Plan> plans = 
            [
            new()
            {
                Name = "Basic Plan",
                Description = "Access to gym facilities during staffed hours.",
                DurationDays = 30,
                Price = 29.99m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Premium Plan",
                Description = "24/7 access to gym facilities, plus group classes.",
                DurationDays = 30,
                Price = 49.99m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Annual Plan",
                Description = "Access to gym facilities for a full year with a discount.",
                DurationDays = 365,
                Price = 299.99m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        ];

        await dbContext.Plans.AddRangeAsync(plans);

        await dbContext.SaveChangesAsync();
    }
}