using System.Diagnostics;
using CyreneCore.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;
using Windows.Media.Core;

namespace CyreneGUI.Utils;

public static class AppUtil
{
    #region Picker

    public static async Task<string> BrowseSingleFile(List<string> filters)
    {
        var picker = new FileOpenPicker(WindowUtil.GetWindowId());

        foreach (var filter in filters)
            picker.FileTypeFilter.Add(filter);

        return (await picker.PickSingleFileAsync()).Path;
    }

    public static async Task<List<string>> BrowseMultiFile(List<string> filters)
    {
        var picker = new FileOpenPicker(WindowUtil.GetWindowId());

        foreach (var filter in filters)
            picker.FileTypeFilter.Add(filter);

        return [.. (await picker.PickMultipleFilesAsync()).Select(x => x.Path)];
    }

    public static async Task<string> BrowseFloder()
    {
        var picker = new FolderPicker(WindowUtil.GetWindowId());
        return (await picker.PickSingleFolderAsync()).Path;
    }

    public static void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath)) return;

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }

    #endregion

    #region UI

    public static Visibility IsVisible(ProcessState cur, ProcessState target)
    {
        return cur == target ? Visibility.Visible : Visibility.Collapsed;
    }

    public static bool IsState(ProcessState cur, ProcessState target)
    {
        return cur == target;
    }

    public static readonly DispatcherQueue Queue = DispatcherQueue.GetForCurrentThread();

    public static void TryEnqueue(Action action)
    {
        Queue.TryEnqueue(() => action());
    }

    public static DispatcherQueueTimer CreateTimer(int intervalMs = 0)
    {
        var timer = Queue.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        return timer;
    }

    public static void AnimateDouble(DependencyObject target, string property, double from, double to,
        int durationMs, EasingMode? easing = null, Action? onCompleted = null)
    {
        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs))
        };
        if (easing != null) anim.EasingFunction = new CubicEase { EasingMode = easing.Value };

        var sb = new Storyboard();
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
        sb.Completed += (s, e) => onCompleted?.Invoke();
        sb.Begin();
    }

    #endregion

    #region Assets

    public static string GetAsset(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
    }

    public static Uri GetUri(string fileName)
    {
        return new Uri(GetAsset(fileName));
    }

    public static ImageSource GetImage(string fileName)
    {
        var path = GetUri(fileName);
        if (Path.GetExtension(fileName)?.ToLowerInvariant() == ".svg")
            return new SvgImageSource(path);
        else
            return new BitmapImage(path);
    }

    public static MediaSource GetMedia(string fileName)
    {
        return MediaSource.CreateFromUri(GetUri(fileName));
    }

    #endregion

    #region Misc

    public static T? RandomElement<T>(this IEnumerable<T> values)
    {
        if (!values.Any()) return default;
        var index = Random.Shared.Next(values.Count());
        return values.ElementAt(index);
    }

    #endregion
}