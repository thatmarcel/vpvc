using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace VPVC.GameCommunication;

public static class ScreenHelper {
    private static string? selectedScreenDeviceId;
    
    // Note that storing the screen bounds means they might become incorrect when a new screen is plugged in
    // (or a / the screen gets unplugged)
    private static Rectangle? selectedScreenBounds;

    public static List<ScreenInfo> GetScreens() {
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
        if (selectedScreenBounds == null) {
            foreach (var screen in Screen.AllScreens) {
                if (screen.DeviceName != selectedScreenDeviceId) {
                    continue;
                }

                selectedScreenBounds = screen.Bounds;

                break;
            }
            
            if (selectedScreenBounds == null) {
                return null;
            }
        }

        var unwrappedSelectedScreenBounds = (Rectangle) selectedScreenBounds;

        Bitmap screenshotBitmap = new Bitmap(unwrappedSelectedScreenBounds.Width, unwrappedSelectedScreenBounds.Height);
        Graphics gr = Graphics.FromImage(screenshotBitmap);
        gr.CopyFromScreen(unwrappedSelectedScreenBounds.X, unwrappedSelectedScreenBounds.Y, 0, 0, screenshotBitmap.Size);
        gr.Flush();
        gr.Dispose();
        return screenshotBitmap;
    }
}