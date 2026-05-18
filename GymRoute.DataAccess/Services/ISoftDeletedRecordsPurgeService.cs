namespace GymRoute.DataAccess.Services;

public interface ISoftDeletedRecordsPurgeService
{
    /// <summary>
    /// Permanently deletes all entities where <see cref="Entities.BaseEntity.IsDeleted"/> is true.
    /// </summary>
    /// <returns>Total number of rows removed.</returns>
    Task<int> PurgeAsync(CancellationToken cancellationToken = default);
}
