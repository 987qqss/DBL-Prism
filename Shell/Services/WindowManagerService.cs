using System.Windows;

namespace Shell.Services;

/// <summary>
/// 窗口管理器 —— 管理单例窗口（监控/报警）。
/// 打开时若已存在则前台显示，否则新建；确保同一窗口只有一个实例。
/// </summary>
public class WindowManagerService
{
    private readonly Dictionary<string, Window> _windows = new();
    private readonly object _lock = new();

    /// <summary>
    /// 获取或创建单例窗口。已存在则前台显示并返回 false（未新建），否则创建并返回 true。
    /// </summary>
    public bool OpenOrActivate(string key, Func<Window> factory)
    {
        lock (_lock)
        {
            if (_windows.TryGetValue(key, out var existing))
            {
                // 已存在：前台显示
                existing.Show();
                if (existing.WindowState == WindowState.Minimized)
                    existing.WindowState = WindowState.Normal;
                existing.Activate();
                return false;
            }
        }

        var window = factory();
        window.Closing += (_, _) =>
        {
            lock (_lock)
            {
                _windows.Remove(key);
            }
        };

        lock (_lock)
        {
            _windows[key] = window;
        }

        window.Show();
        return true;
    }

    /// <summary>窗口是否已打开</summary>
    public bool IsOpen(string key)
    {
        lock (_lock)
        {
            return _windows.ContainsKey(key);
        }
    }

    /// <summary>关闭并销毁指定窗口</summary>
    public void Close(string key)
    {
        lock (_lock)
        {
            if (_windows.TryGetValue(key, out var w))
            {
                _windows.Remove(key);
                w.Close();
            }
        }
    }
}
