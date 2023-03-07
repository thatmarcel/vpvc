using System.Diagnostics;

namespace VPVC; 

public class Logger {
    public static void Log(string message) {
        Debug.WriteLine($"[VPVC] {message}");
    }
}