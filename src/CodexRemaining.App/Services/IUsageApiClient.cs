using CodexRemaining.App.Models;

namespace CodexRemaining.App.Services;

public interface IUsageApiClient
{
    Task<UsageSnapshot> GetUsageSnapshotAsync(CancellationToken cancellationToken);
}
