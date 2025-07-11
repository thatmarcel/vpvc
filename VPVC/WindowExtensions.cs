using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT;
using WinRT.Interop;

namespace VPVC;

public static class WindowExtensions {
    private const int WM_SETICON = 0x0080;
    
    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]  
    private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam); 
    
    public static void ConfigureWindowWithSize(this Window window, int width, int height) {
        var hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        
        var sExe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (sExe != null) {
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(sExe);
            if (icon != null) {
                SendMessage(hWnd, WM_SETICON, 1, icon.Handle);
            }
        }
        
        appWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        appWindow.Title = "VPVC";

        appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        
        appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        var nativeWindow = window.As<IWindowNative>();
        var nativeWindowHandle = nativeWindow.WindowHandle;
        var dpi = PInvoke.User32.GetDpiForWindow(nativeWindowHandle);
        var scalingFactor = dpi / 96d;

        appWindow.Resize(new SizeInt32((int) (width * scalingFactor), (int) (height * scalingFactor)));
        
        var presenter = appWindow.Presenter as OverlappedPresenter;
        
        if (presenter != null) {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
    }
}