using System;

namespace VPVC.Helpers; 

public static class PointDistance {
    public static double Calculate(int x1, int y1, int x2, int y2) {
        return Math.Sqrt(
            ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1))
        );
    }
}