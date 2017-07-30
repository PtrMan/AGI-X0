using System;

namespace MetaNix.math {
    public static class MathMisc {
        public static bool checkEpsilon(float a, float b, float epsilon) {
            return Math.Abs(a - b) < epsilon;
        }
    }
}
