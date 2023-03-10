using System.Drawing;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace VPVC.GameCommunication;

public static class ScreenHelper {
    private static string? selectedScreenDeviceId;
    
    public static List<ScreenInfo> GetScreens() {
        ManagementObjectSearcher displayRecordSearcher = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution, DeviceID, Name, CurrentRefreshRate FROM Win32_VideoController");

        var screens = new List<ScreenInfo>();

        foreach (var screen in Screen.AllScreens) {
            var screenDisplayName = $"Screen {screen.DeviceName.Replace("\\\\.\\DISPLAY", "")} ({(screen.Primary ? "Primary, " : "")}{screen.Bounds.Width}x{screen.Bounds.Height})";
            screens.Add(new ScreenInfo(screen.DeviceName, screenDisplayName));
        }
        
       return screens;
    }

    public static void SelectScreenWithDeviceId(string deviceId) {
        selectedScreenDeviceId = deviceId;
    }
    
    public static Bitmap? TakeScreenshot() {
        Screen? selectedScreen = null;
        
        foreach (var screen in Screen.AllScreens) {
            if (screen.DeviceName != selectedScreenDeviceId) {
                continue;
            }

            selectedScreen = screen;

            break;
        }

        if (selectedScreen == null) {
            return null;
        }
        
        Bitmap screenshotBitmap = new Bitmap(selectedScreen.Bounds.Width, selectedScreen.Bounds.Height);
        Graphics gr = Graphics.FromImage(screenshotBitmap);
        gr.CopyFromScreen(selectedScreen.Bounds.X, selectedScreen.Bounds.Y, 0, 0, screenshotBitmap.Size);
        return screenshotBitmap;
    }
}