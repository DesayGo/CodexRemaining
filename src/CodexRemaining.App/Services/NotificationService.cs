namespace CodexRemaining.App.Services;

public sealed class NotificationService
{
    private readonly HashSet<string> _notified = new();

    public void NotifyThreshold(string key, string message)
    {
        if (_notified.Contains(key))
        {
            return;
        }

        _notified.Add(key);
        System.Windows.MessageBox.Show(message, "Codex 使用提醒", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
    }

    public void Reset(string key)
    {
        _notified.Remove(key);
    }
}
