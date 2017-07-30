using System;

namespace WhiteSphereEngine.math {
    // helper methods not covered by c# standard math library
    public class Math {
        private Math(){}

        public static double min(double v0, double v1, double v2) {
            double temp = System.Math.Min(v0, v1);
            temp = System.Math.Min(temp, v2);
            return temp;
        }

        public static double max(double v0, double v1, double v2) {
            double temp = System.Math.Max(v0, v1);
            temp = System.Math.Max(temp, v2);
            return temp;
        }

        public static double sqrt3(double value, double accuracy = 0.000000001) {
            // see https://math.stackexchange.com/a/1400281
            NewtonsMethod.FunctionDelegateType f = delegate (double v){ return v*v*v - value; };
            NewtonsMethod.FunctionDelegateType fDerivative = delegate (double v) { return 3.0 * v*v; };

            long maxRepetitions = -1;
            return NewtonsMethod.newtonsMethod(f, fDerivative, accuracy, maxRepetitions, System.Math.Sqrt(value));
        }

        public static float clamp(float value, float min, float max) {
            float clamped = System.Math.Max(value, min);
            clamped = System.Math.Min(value, max);
            return clamped;
        }

        public static double clamp(double value, double min, double max) {
            double clamped = System.Math.Max(value, min);
            clamped = System.Math.Min(value, max);
            return clamped;
        }

        public static double powerByIntegerSlow(double number, int power) {
            double result = 1.0;
            for( int i = 0; i < power; i++ ) {
                result *= number;
            }
            return result;
        }

        public static double dist2FromZero(double x, double y) {
            return dist2(0.0, 0.0, x, y);
        }

        public static double dist2(double x0, double y0, double x1, double y1) {
            double diffX = x0 - x1, diffY = y0 - y1;
            return System.Math.Sqrt(diffX*diffX + diffY*diffY);
        }
    }
}
