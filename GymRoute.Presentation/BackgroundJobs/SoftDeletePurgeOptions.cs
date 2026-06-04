namespace GymRoute.Presentation.BackgroundJobs;

public sealed class SoftDeletePurgeOptions
{
    public const string SectionName = "SoftDeletePurge";

    public bool Enabled { get; set; } = true;

    /// <summary>How often the purge runs (default: every 30 days).</summary>
    public int IntervalDays { get; set; } = 30;

    /// <summary>When true, runs once when the application starts, then on each interval.</summary>
    public bool RunOnStartup { get; set; }
}
