using System.Runtime.InteropServices;

namespace VPVC;

// see https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-backdrop-controller

public class WindowsSystemDispatcherQueueHelper {
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    object? dispatcherQueueController = null;
    
    public void EnsureWindowsSystemDispatcherQueueController() {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null) {
            return;
        }

        if (dispatcherQueueController == null) {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;
            options.apartmentType = 2;

            #pragma warning disable CS8601
            CreateDispatcherQueueController(options, ref dispatcherQueueController);
            #pragma warning restore CS8601
        }
    }
}