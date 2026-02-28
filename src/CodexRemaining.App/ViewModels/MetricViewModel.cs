using System.Windows.Media;

namespace CodexRemaining.App.ViewModels;

public sealed class MetricViewModel : ViewModelBase
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    private double _remainingPercent;
    public double RemainingPercent
    {
        get => _remainingPercent;
        set
        {
            if (SetProperty(ref _remainingPercent, value))
            {
                RaisePropertyChanged(nameof(PercentText));
                RaisePropertyChanged(nameof(StatusBrush));
            }
        }
    }

    private string _detailText = string.Empty;
    public string DetailText
    {
        get => _detailText;
        set => SetProperty(ref _detailText, value);
    }

    public string PercentText => $"{RemainingPercent:0}%";

    public Brush StatusBrush => RemainingPercent switch
    {
        <= 10 => Brushes.IndianRed,
        <= 30 => Brushes.Gold,
        _ => Brushes.MediumSeaGreen
    };
}
