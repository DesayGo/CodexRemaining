using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using CodexRemaining.App.Commands;
using CodexRemaining.App.Models;
using CodexRemaining.App.Services;

namespace CodexRemaining.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;
    private readonly IUsageApiClient _usageApiClient;
    private readonly DispatcherTimer _timer;
    private readonly AppSettings _settings;

    public ObservableCollection<MetricViewModel> Metrics { get; } = [];

    private string _lastUpdateText = "等待首次更新...";
    public string LastUpdateText
    {
        get => _lastUpdateText;
        set => SetProperty(ref _lastUpdateText, value);
    }

    public RelayCommand RefreshNowCommand { get; }
    public RelayCommand ToggleCompactCommand { get; }
    public RelayCommand HideWindowCommand { get; }

    private bool _compactMode;
    public bool CompactMode
    {
        get => _compactMode;
        set
        {
            if (SetProperty(ref _compactMode, value))
            {
                ApplyCompactMode();
                _settings.CompactMode = value;
                _settingsService.Save(_settings);
            }
        }
    }

    private double _windowOpacity;
    public double WindowOpacity
    {
        get => _windowOpacity;
        set
        {
            if (SetProperty(ref _windowOpacity, Math.Clamp(value, 0.35, 1.0)))
            {
                _settings.WindowOpacity = _windowOpacity;
                _settingsService.Save(_settings);
            }
        }
    }

    private int _refreshSeconds;
    public int RefreshSeconds
    {
        get => _refreshSeconds;
        set
        {
            var validated = Math.Clamp(value, 3, 120);
            if (SetProperty(ref _refreshSeconds, validated))
            {
                _timer.Interval = TimeSpan.FromSeconds(validated);
                _settings.RefreshSeconds = validated;
                _settingsService.Save(_settings);
            }
        }
    }

    public MainViewModel()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();
        _usageApiClient = new CodexUsageApiClient(_settings);
        _notificationService = new NotificationService();

        _windowOpacity = _settings.WindowOpacity;
        _refreshSeconds = _settings.RefreshSeconds;
        _compactMode = _settings.CompactMode;

        RefreshNowCommand = new RelayCommand(async _ => await RefreshAsync());
        ToggleCompactCommand = new RelayCommand(_ => CompactMode = !CompactMode);
        HideWindowCommand = new RelayCommand(_ => Application.Current.MainWindow?.Hide());

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(RefreshSeconds)
        };
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();

        ApplyCompactMode();
        _ = RefreshAsync();
    }

    private void ApplyCompactMode()
    {
        if (Application.Current.MainWindow is not null)
        {
            Application.Current.MainWindow.Height = CompactMode ? 250 : 400;
            Application.Current.MainWindow.Width = CompactMode ? 320 : 380;
        }
    }

    private async Task RefreshAsync()
    {
        var snapshot = await _usageApiClient.GetUsageSnapshotAsync(CancellationToken.None);
        Metrics.Clear();

        foreach (var metric in snapshot.Metrics)
        {
            var detail = $"重置时间: {metric.ResetAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}";
            if (!string.IsNullOrWhiteSpace(metric.Notes))
            {
                detail = $"{detail} · {metric.Notes}";
            }

            Metrics.Add(new MetricViewModel
            {
                Key = metric.Key,
                Name = metric.Name,
                RemainingPercent = metric.RemainingPercent,
                DetailText = detail
            });

            if (metric.RemainingPercent <= _settings.WarningThresholdPercent)
            {
                _notificationService.NotifyThreshold(metric.Key, $"{metric.Name} 剩余 {metric.RemainingPercent:0}%");
            }
            else
            {
                _notificationService.Reset(metric.Key);
            }
        }

        LastUpdateText = $"{snapshot.SourceStatus} · 更新时间 {snapshot.RetrievedAt.LocalDateTime:HH:mm:ss}";
    }
}
