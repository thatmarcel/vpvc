using Microsoft.Extensions.Logging;

namespace VPVC.VoiceChat; 

public class WebRtcDebugLoggerFactory: ILoggerFactory {
    public void Dispose() {}

    public ILogger CreateLogger(string categoryName) {
        return WebRtcDebugLogger.instance;
    }

    public void AddProvider(ILoggerProvider provider) {}
}