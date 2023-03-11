using System;
using System.Diagnostics;

namespace VPVC;

public static class Logger {
    public static void Log(string message) {
#if DEBUG
        try {
            Debug.WriteLine($"[VPVC] {message}");
        } catch (Exception) {}
#endif
    }
}