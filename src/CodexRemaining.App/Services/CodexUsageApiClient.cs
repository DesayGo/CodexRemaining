using System.Net.Http.Headers;
using System.Text.Json;
using CodexRemaining.App.Models;

namespace CodexRemaining.App.Services;

public sealed class CodexUsageApiClient : IUsageApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;

    public CodexUsageApiClient(AppSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.ApiBaseUrl)
        };

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public async Task<UsageSnapshot> GetUsageSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(_settings.UsageEndpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<UsageApiPayload>(responseStream, cancellationToken: cancellationToken);

            return BuildSnapshot(payload);
        }
        catch
        {
            return BuildFallbackSnapshot();
        }
    }

    private static UsageSnapshot BuildSnapshot(UsageApiPayload? payload)
    {
        var now = DateTimeOffset.Now;
        var metrics = new List<UsageMetric>
        {
            ToMetric("limit_5h", "5小时使用限制", payload?.Limit5hRemainingPercent, payload?.Limit5hResetAt, now),
            ToMetric("limit_weekly", "每周使用限制", payload?.WeeklyRemainingPercent, payload?.WeeklyResetAt, now),
            ToMetric("spark_5h", "GPT-5.3 Codex-Spark 5小时", payload?.Spark5hRemainingPercent, payload?.Spark5hResetAt, now),
            ToMetric("spark_weekly", "GPT-5.3 Codex-Spark 每周", payload?.SparkWeeklyRemainingPercent, payload?.SparkWeeklyResetAt, now),
            ToMetric("code_review", "代码审查使用限制", payload?.CodeReviewRemainingPercent, payload?.CodeReviewResetAt, now),
            ToMetric("credits", "剩余信用", payload?.CreditRemainingPercent, payload?.CreditResetAt, now)
        };

        return new UsageSnapshot
        {
            Metrics = metrics,
            RetrievedAt = now,
            SourceStatus = "实时数据"
        };
    }

    private static UsageMetric ToMetric(string key, string name, double? percent, DateTimeOffset? resetAt, DateTimeOffset now)
    {
        return new UsageMetric
        {
            Key = key,
            Name = name,
            RemainingPercent = Math.Clamp(percent ?? 0, 0, 100),
            ResetAt = resetAt ?? now.AddHours(5),
            Notes = ""
        };
    }

    private static UsageSnapshot BuildFallbackSnapshot()
    {
        var now = DateTimeOffset.Now;
        return new UsageSnapshot
        {
            RetrievedAt = now,
            SourceStatus = "无法获取数据，显示模拟值",
            Metrics =
            [
                new() { Key = "limit_5h", Name = "5小时使用限制", RemainingPercent = 66, ResetAt = now.AddHours(3) },
                new() { Key = "limit_weekly", Name = "每周使用限制", RemainingPercent = 52, ResetAt = now.AddDays(2) },
                new() { Key = "spark_5h", Name = "GPT-5.3 Codex-Spark 5小时", RemainingPercent = 41, ResetAt = now.AddHours(4) },
                new() { Key = "spark_weekly", Name = "GPT-5.3 Codex-Spark 每周", RemainingPercent = 73, ResetAt = now.AddDays(4) },
                new() { Key = "code_review", Name = "代码审查使用限制", RemainingPercent = 18, ResetAt = now.AddHours(12) },
                new() { Key = "credits", Name = "剩余信用", RemainingPercent = 34, ResetAt = now.AddDays(30) }
            ]
        };
    }

    private sealed record UsageApiPayload
    {
        public double? Limit5hRemainingPercent { get; init; }
        public DateTimeOffset? Limit5hResetAt { get; init; }
        public double? WeeklyRemainingPercent { get; init; }
        public DateTimeOffset? WeeklyResetAt { get; init; }
        public double? Spark5hRemainingPercent { get; init; }
        public DateTimeOffset? Spark5hResetAt { get; init; }
        public double? SparkWeeklyRemainingPercent { get; init; }
        public DateTimeOffset? SparkWeeklyResetAt { get; init; }
        public double? CodeReviewRemainingPercent { get; init; }
        public DateTimeOffset? CodeReviewResetAt { get; init; }
        public double? CreditRemainingPercent { get; init; }
        public DateTimeOffset? CreditResetAt { get; init; }
    }
}
