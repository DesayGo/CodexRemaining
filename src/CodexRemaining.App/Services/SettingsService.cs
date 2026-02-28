using System.Text.Json;
using CodexRemaining.App.Models;

namespace CodexRemaining.App.Services;

public sealed class SettingsService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string SettingsPath { get; } = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(SettingsPath, json);
    }
}
