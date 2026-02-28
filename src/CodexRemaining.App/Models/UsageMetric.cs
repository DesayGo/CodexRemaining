namespace CodexRemaining.App.Models;

public sealed class UsageMetric
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required double RemainingPercent { get; init; }
    public required DateTimeOffset ResetAt { get; init; }
    public string? Notes { get; init; }
}
