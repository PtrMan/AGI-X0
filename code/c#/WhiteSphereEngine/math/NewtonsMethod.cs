using System.Diagnostics;

namespace WhiteSphereEngine.math {
    public class NewtonsMethod {
        private NewtonsMethod(){}

        public delegate double FunctionDelegateType(double value);

        public static double newtonsMethod(FunctionDelegateType f, FunctionDelegateType fDerivative, double accuracy, long maxRepetitions = -1, double x = 0.0) {
            Debug.Assert(accuracy > 0.0);

            for(long repetition = 0; repetition < (maxRepetitions == -1 ? long.MaxValue : maxRepetitions); repetition++ ) {
                double nextX = x - f(x) / fDerivative(x);
                if (System.Math.Abs(f(nextX)) < accuracy) {
                    return nextX;
                }
                x = nextX;
            }

            // if we are here we had enough repetitions
            return x;
        }
    }
}
