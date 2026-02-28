namespace CodexRemaining.App.Models;

public sealed class UsageSnapshot
{
    public required IReadOnlyList<UsageMetric> Metrics { get; init; }
    public required DateTimeOffset RetrievedAt { get; init; }
    public string? SourceStatus { get; init; }
}
