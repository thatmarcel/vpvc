using System.Drawing;
using System.Management;

#pragma warning disable CA1416

namespace VPVC.GameCommunication;

public static class ScreenHelper {
    public static Bitmap? TakeScreenshot() {
        // return LoadScreenshotFromFile();

        uint horizontalResolution = 0;
        uint verticalResolution = 0;
        
        ManagementObjectSearcher displayResolutionRecordSearcher = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
        foreach (var displayResolutionRecordObject in displayResolutionRecordSearcher.Get()) {
            var displayResolutionRecord = (ManagementObject) displayResolutionRecordObject;
            horizontalResolution = (uint) displayResolutionRecord["CurrentHorizontalResolution"];
            verticalResolution = (uint) displayResolutionRecord["CurrentVerticalResolution"];
            break;
        }

        if (horizontalResolution == 0 || verticalResolution == 0) {
            return null;
        }
        
        Bitmap screenshotBitmap = new Bitmap((int) horizontalResolution, (int) verticalResolution);
        Graphics gr = Graphics.FromImage(screenshotBitmap);
        gr.CopyFromScreen(0, 0, 0, 0, screenshotBitmap.Size);
        return screenshotBitmap;
    }

    private static Bitmap? LoadScreenshotFromFile() {
        try {
            return (Bitmap) Image.FromFile(@"C:\Users\mrcl\Pictures\Screenshots\Screenshot 2023-03-03 190339.png", true);
        } catch (System.IO.FileNotFoundException) {
            return null;
        }
    }
}