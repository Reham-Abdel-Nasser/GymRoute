using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoute.DataAccess.Data.Seeder;

public static class CategorySeeder
{
    public static async Task SeedAsync(GymDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
            return;

        var categories = new List<Category>
            {
                new() { Name = "Cardio Equipment" },
                new() { Name = "Strength & Weightlifting" },
                new() { Name = "Supplements & Protein" },
                new() { Name = "Fitness Wear & Clothes" },
                new() { Name = "Gym Accessories" },
                new() { Name = "Yoga & Pilates" },
                new() { Name = "Boxing & MMA" },
                new() { Name = "Recovery & Massage Tools" },
                new() { Name = "Smart Fitness Gadgets" },
                new() { Name = "Healthy Snacks & Drinks" }
            };

        await dbContext.Categories.AddRangeAsync(categories);

        await dbContext.SaveChangesAsync();
    }

}
