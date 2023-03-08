using System;
using Microsoft.Extensions.Logging;

namespace VPVC.VoiceChat; 

public class WebRtcDebugLogger: ILogger, IDisposable {
    public static WebRtcDebugLogger instance = new();
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        Logger.Log($"[WebRtcDebugLogger] [{logLevel}] {formatter.Invoke(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) {
        return instance;
    }

    public void Dispose() {}
}