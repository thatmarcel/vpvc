using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using VPVC.BackendCommunication;
using VPVC.GameCommunication;

namespace VPVC;

public delegate void AppEmptyCallback();

public partial class App: Application {
    private Window? mainWindow;

    private static DispatcherQueue? dispatcherQueue;
        
    public App() {
        InitializeComponent();
    }
        
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        mainWindow = new MainWindow();
        mainWindow.Activate();

        dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        GameStateAndCoordinatesExtractor.StartRepeatedExtraction();
        
        RunInBackground(() => {
            for (;;) {
                MessageReceiver.ProcessNextMessage();
                Thread.Sleep(1);
            }

            // ReSharper disable once FunctionNeverReturns
        });
        
        DebuggingInformationHelper.StartUpdating();
    }

    public static void RunInForeground(AppEmptyCallback callback) {
        if (dispatcherQueue?.HasThreadAccess ?? false) {
            try {
                callback.Invoke();
            } catch (Exception exception) {
                DebuggingInformationHelper.hasEverEncounteredExceptionWhenRunningInForeground = true;
                Logger.Log(exception.ToString());
            }
        } else {
            var success = dispatcherQueue?.TryEnqueue(callback.Invoke) ?? false;

            if (!success) {
                DebuggingInformationHelper.hasEnqueuingInForegroundEverFailed = true;
            }
        }
    }
    
    public static void RunInBackground(AppEmptyCallback callback) {
        Task.Run(() => {
            try {
                callback.Invoke();
            } catch (Exception exception) {
                DebuggingInformationHelper.hasEverEncounteredExceptionWhenRunningInBackground = true;
                Logger.Log(exception.ToString());
            }
        });
    }
}