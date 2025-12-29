using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace CyreneGUI.Utils;

public static partial class WindowUtil
{
    private static nint OldWndProc = nint.Zero;
    private static WndProcDelegate? CurDelegate;
    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static partial nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "GetDpiForWindow")]
    private static partial uint GetDpiForWindow(nint hWnd);

    public static WindowId GetWindowId()
    {
        return Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(App.Window));
    }

    public static void InitWindowSetting(Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var app = AppWindow.GetFromWindowId(GetWindowId());

        // Disable maximize
        CurDelegate = (hWnd, msg, wParam, lParam) =>
        {
            if (msg == 0x00A3) return 0; // WM_NCLBUTTONDBLCLK
            return CallWindowProc(OldWndProc, hWnd, msg, wParam, lParam);
        };
        OldWndProc = SetWindowLongPtr(hWnd, -4, Marshal.GetFunctionPointerForDelegate(CurDelegate));

        if (app.Presenter is OverlappedPresenter p)
        {
            p.IsMaximizable = false;
            p.IsResizable = false;
        }

        // Support DPI
        var factor = GetDpiForWindow(hWnd) / 96f;
        int width = (int)(1200 * factor);
        int height = (int)(675 * factor);
        app.Resize(new SizeInt32(width, height));

        // Move to center
        var area = DisplayArea.GetFromWindowId(GetWindowId(), DisplayAreaFallback.Nearest);
        var pos = app.Position;
        pos.X = (area.WorkArea.Width - app.Size.Width) / 2;
        pos.Y = (area.WorkArea.Height - app.Size.Height) / 2;

        // Multi monitor support
        pos.X += area.WorkArea.X;
        pos.Y += area.WorkArea.Y;

        app.Move(pos);
    }
}