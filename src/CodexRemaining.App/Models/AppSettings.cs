namespace CodexRemaining.App.Models;

public sealed class AppSettings
{
    public int RefreshSeconds { get; set; } = 10;
    public double WindowOpacity { get; set; } = 0.92;
    public bool CompactMode { get; set; }
    public string ApiBaseUrl { get; set; } = "https://api.openai.com";
    public string UsageEndpoint { get; set; } = "/v1/codex/usage-limits";
    public int WarningThresholdPercent { get; set; } = 10;
}
