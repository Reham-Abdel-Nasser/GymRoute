using GymRoute.DataAccess.Data.Contexts;
using GymRoute.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymRoute.DataAccess.Services;

public sealed class SoftDeletedRecordsPurgeService(
    GymDbContext dbContext,
    ILogger<SoftDeletedRecordsPurgeService> logger) : ISoftDeletedRecordsPurgeService
{
    public async Task<int> PurgeAsync(CancellationToken cancellationToken = default)
    {
        var totalRemoved = 0;

        // Children first (FK-safe order)
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.Bookings, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.MemberShips, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.Sessions, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.Users, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.HealthRecords, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.Plans, cancellationToken);
        totalRemoved += await RemoveSoftDeletedAsync(dbContext.Categories, cancellationToken);

        if (totalRemoved > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Business operation: soft-delete purge completed. PermanentlyRemoved={Count}",
                totalRemoved);
        }
        else
        {
            logger.LogInformation(
                "Business operation: soft-delete purge completed. PermanentlyRemoved=0");
        }

        return totalRemoved;
    }

    private static async Task<int> RemoveSoftDeletedAsync<TEntity>(
        DbSet<TEntity> set,
        CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        var entities = await set
            .IgnoreQueryFilters()
            .Where(e => e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
            return 0;

        set.RemoveRange(entities);
        return entities.Count;
    }
}
