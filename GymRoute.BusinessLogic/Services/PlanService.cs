using GymRoute.BusinessLogic.Interfaces;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymRoute.BusinessLogic.Services;

public class PlanService(IGymDbContext db, ILogger<PlanService> logger) : IPlanService
{
    public async Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await db.Plans
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved active plans. Count={PlanCount}",
            plans.Count);

        return plans;
    }

    public async Task<Plan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await db.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan is null)
            logger.LogWarning("Plan not found. PlanId={PlanId}", id);

        return plan;
    }

    public async Task<Plan> CreateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        db.Plans.Add(plan);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Plan created. PlanId={PlanId} Name={PlanName} Price={PlanPrice}",
            plan.Id,
            plan.Name,
            plan.Price);

        return plan;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await db.Plans.FindAsync([id], cancellationToken);
        if (plan is null)
        {
            logger.LogWarning("Plan soft-delete skipped — not found. PlanId={PlanId}", id);
            return false;
        }

        plan.IsDeleted = true;
        plan.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Plan soft-deleted. PlanId={PlanId} Name={PlanName}",
            plan.Id,
            plan.Name);

        return true;
    }
}
