using System.Drawing;
using System.Collections.Generic;
using VPVC.ScreenCapture;

#pragma warning disable CA1416

namespace VPVC.GameCommunication;

public static class ScreenHelper {
    public static List<ScreenInfo> GetScreens() {
        return ScreenCaptureManager.instance.GetScreens();
    }

    public static void SelectScreenWithDeviceId(string deviceId) {
        ScreenCaptureManager.instance.SelectScreenWithDeviceId(deviceId);
    }
    
    public static Bitmap? TakeScreenshot() {
        return ScreenCaptureManager.instance.CaptureScreen();
    }
}