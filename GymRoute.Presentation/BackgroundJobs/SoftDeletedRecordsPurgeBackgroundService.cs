using GymRoute.DataAccess.Services;
using Microsoft.Extensions.Options;

namespace GymRoute.Presentation.BackgroundJobs;

/// <summary>
/// Background job that permanently removes soft-deleted records every 30 days (configurable).
/// </summary>
public sealed class SoftDeletedRecordsPurgeBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<SoftDeletePurgeOptions> options,
    ILogger<SoftDeletedRecordsPurgeBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var purgeOptions = options.Value;

        if (!purgeOptions.Enabled)
        {
            logger.LogInformation("Soft-delete purge background job is disabled.");
            return;
        }

        if (purgeOptions.IntervalDays <= 0)
        {
            logger.LogWarning(
                "Soft-delete purge IntervalDays must be greater than zero. Job will not run.");
            return;
        }

        var interval = TimeSpan.FromDays(purgeOptions.IntervalDays);
        logger.LogInformation(
            "Soft-delete purge scheduled every {Days} day(s). RunOnStartup={RunOnStartup}",
            purgeOptions.IntervalDays,
            purgeOptions.RunOnStartup);

        using var timer = new PeriodicTimer(interval);

        if (purgeOptions.RunOnStartup)
            await RunPurgeAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunPurgeAsync(stoppingToken);
    }

    private async Task RunPurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Soft-delete purge job started at {Time:O}", DateTimeOffset.UtcNow);

            await using var scope = scopeFactory.CreateAsyncScope();
            var purgeService = scope.ServiceProvider.GetRequiredService<ISoftDeletedRecordsPurgeService>();
            var removed = await purgeService.PurgeAsync(cancellationToken);

            logger.LogInformation(
                "Soft-delete purge job finished. Removed {Count} record(s) at {Time:O}",
                removed,
                DateTimeOffset.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Soft-delete purge job failed.");
        }
    }
}
