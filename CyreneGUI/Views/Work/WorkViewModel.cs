using CommunityToolkit.Mvvm.ComponentModel;
using CyreneCore.Process;
using CyreneCore.Utils;
using CyreneGUI.Components;
using CyreneGUI.Components.FileItem;
using CyreneGUI.Utils;
using CyreneGUI.Views.Console;
using System.Collections.ObjectModel;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneGUI.Views.Work;

public sealed partial class WorkViewModel : ObservableObject
{
    private CancellationTokenSource? Cts;
    private readonly Queue<Task> FileItemRefresh = [];
    [ObservableProperty] public partial ProcessState State { get; set; }
    [ObservableProperty] public partial string ProcessText { get; set; } = Localize("StartProcess");
    public ObservableCollection<FileItemViewModel> FileItems = [];
    public readonly ConsoleViewModel ConsoleModel = ModelManager.ConsoleModel;

    public WorkViewModel()
    {
        RefreshFileItems();
    }

    private void RefreshFileItems()
    {
        AppUtil.TryEnqueue(() =>
        {
            FileItems.Clear();
            foreach (var filePath in AppConfigUtil.Config.Extract.Input)
                if (File.Exists(filePath))
                    FileItems.Add(new FileItemViewModel(filePath));
        });
    }

    public void AddFileItems(List<string> paths)
    {
        AppUtil.TryEnqueue(() =>
        {
            if (paths.Count == 0) return;

            var input = AppConfigUtil.Config.Extract.Input;
            var exists = input.Select(x => Path.GetFileNameWithoutExtension(x)).ToHashSet();
            foreach (var path in paths)
                if (File.Exists(path))
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    if (exists.Add(fileName)) input.Add(path);
                }
                else if (Directory.Exists(path))
                    foreach (var file in Directory.GetFiles(path, "*.usm"))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (exists.Add(fileName)) input.Add(file);
                    }

            AppConfigUtil.Save();

            RefreshFileItems();
            Logger.Info(Localize("AddedInputs"));
        });
    }

    public void RemoveFileItem(string filePath)
    {
        AppUtil.TryEnqueue(() =>
        {
            var input = AppConfigUtil.Config.Extract.Input;
            if (input.Remove(filePath)) AppConfigUtil.Save();

            var toRemove = FileItems.FirstOrDefault(f => f.FilePath == filePath);
            if (toRemove != null) FileItems.Remove(toRemove);
        });
    }

    public void ClearFileItems()
    {
        AppUtil.TryEnqueue(() =>
        {
            AppConfigUtil.Config.Extract.Input.Clear();
            AppConfigUtil.Save();

            RefreshFileItems();
            Logger.Info(Localize("ClearedInputs"));
        });
    }

    public void OpenOutputFloder()
    {
        AppUtil.TryEnqueue(() =>
        {
            AppUtil.OpenFolder(AppConfigUtil.Config.Extract.Output);
            Logger.Info(Localize("OpenedOutput"));
        });
    }

    public void ChangePageState(ProcessState state, object? value = null)
    {
        AppUtil.TryEnqueue(() =>
        {
            State = state;
            switch (state)
            {
                case ProcessState.None:
                    foreach (var item in FileItems)
                        if (item.State == ProcessState.Pending || item.State == ProcessState.Processing)
                            item.State = ProcessState.None;

                    ProcessText = Localize("StartProcess");

                    Cts?.Dispose();
                    Cts = null;
                    break;
                case ProcessState.Pending:
                    foreach (var item in FileItems)
                        item.State = ProcessState.Pending;

                    ProcessText = LocalizeFormat("ProcessBar", "0%");
                    break;
                case ProcessState.Processing:
                    if (value is not ResultData result) break;
                    if (result.Status == ProcessState.Processing) break;

                    var text = LocalizeFormat("ProcessBar", $"{(double)result.Index / result.Total:P0}");
                    if (ProcessText != text) ProcessText = text;
                    break;
                case ProcessState.Failed:
                    if (value is not Exception ex) break;

                    foreach (var file in FileItems)
                        if (file.State == ProcessState.Pending)
                            file.State = ProcessState.None;
                        else if (file.State == ProcessState.Processing)
                        {
                            file.State = ProcessState.Failed;
                            file.ErrorTip = ex is OperationCanceledException ? "Canceled" : ex.Message;
                        }
                    break;
            }
        });
    }

    public void ChangeItemState(ResultData result, FileItemViewModel item)
    {
        AppUtil.TryEnqueue(() =>
        {
            var fileName = Path.GetFileNameWithoutExtension(result.Input);

            switch (result.Status)
            {
                case ProcessState.Processing:
                    item.State = ProcessState.Processing;
                    ConsoleModel.AddConsoleLog(LocalizeFormat("ProcessingItem", result.Index, result.Total, fileName));
                    break;
                case ProcessState.Completed:
                    item.State = ProcessState.Completed;
                    ConsoleModel.AddConsoleLog(LocalizeFormat("ProcessedItem", result.Index, result.Total, fileName));

                    FileItemRefresh.Enqueue(Task.Delay(1000).ContinueWith(_ =>
                    {
                        RemoveFileItem(item.FilePath);
                    }));
                    break;
                case ProcessState.Failed:
                    item.State = ProcessState.Failed;
                    var errMeg = item.ErrorTip = result.Msg ?? "Unknown";
                    ConsoleModel.AddConsoleLog(LocalizeFormat("ProcessItemErr", fileName, errMeg));
                    break;
            }
        });
    }

    public async Task StartProcessAsync()
    {
        await Task.Run(async () =>
        {
            if (FileItems.Count == 0 ||
                string.IsNullOrWhiteSpace(AppConfigUtil.Config.Extract.Output)) return;

            if (Cts != null) CancelProcess();
            Cts = new CancellationTokenSource();

            Logger.Info(Localize("InitState"));
            ChangePageState(ProcessState.Pending);
            var mgr = new ProcessManager(AppConfigUtil.Config, Cts.Token);
            if (!await mgr.InitAsync())
            {
                Logger.Error(LocalizeFormat("InitStateErr"));
                ChangePageState(ProcessState.None);
                return;
            }

            try
            {
                Logger.Info(Localize("ProcessState"));
                await foreach (var result in mgr.ProcessAsync().WithCancellation(Cts.Token))
                {
                    var fileItem = FileItems.FirstOrDefault(f => f.FilePath == result.Input);
                    if (fileItem == null) continue;

                    ChangeItemState(result, fileItem);
                    ChangePageState(ProcessState.Processing, result);
                }

                await Task.WhenAll(FileItemRefresh);
                Logger.Info(Localize("CompleteState"));
            }
            catch (OperationCanceledException ex)
            {
                Logger.Warn(Localize("CancelState"));
                ChangePageState(ProcessState.Failed, ex);
            }
            catch (Exception ex)
            {
                Logger.Error(LocalizeFormat("ProcessStateErr", ex.Message));
                ChangePageState(ProcessState.Failed, ex);
            }
            finally
            {
                ChangePageState(ProcessState.None);
            }
        });
    }

    public void CancelProcess()
    {
        Cts?.Cancel();
        Cts?.Dispose();
        Cts = null;
    }
}
