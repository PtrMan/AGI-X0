using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaNix.framework.contempary {
    public class SupportVectorMachine {
        public struct ConstructorParameters {
            public uint maximumNumberOfIterations;
            public double learnRate;
            public double mu; // can be for example 0.1
            public bool withBias;

            public IKernel kernel;
        }

        public SupportVectorMachine(ConstructorParameters parameters) {
            this.maximumNumberOfIterations = parameters.maximumNumberOfIterations;
            this.learnRate = parameters.learnRate;
            this.kernel = parameters.kernel;
            this.mu = parameters.mu;
            this.withBias = parameters.withBias;
        }

        public void learn() {
            if (withBias) {
                learnKernelAdatronAlgorithmWithBias();
            }
            else {
                learnKernelAdatronAlgorithmWithoutBias();
            }
        }

        public double classify(double[] data) {
            double sum = 0.0;

            // TODO< do this just for the support vectors >
            for (int i = 0; i < numberOflabeledPoints; i++) {
                sum += getY(i) * alpha[i] * kernel.calc(data, labeledPoints[i].Item1);
            }

            if (withBias) {
                sum += lamda;
            }

            return Math.Sign(sum);
        }
        
        // see http://www.svms.org/training/CaCr.pdf page 11
        void learnKernelAdatronAlgorithmWithoutBias() {
            alpha = new double[numberOflabeledPoints];
            z = new double[numberOflabeledPoints];

            for (uint iteration = 0; iteration < maximumNumberOfIterations; iteration++) {
                Parallel.For(0, numberOflabeledPoints, i => z[i] = calcKernelizedSum((int)i));

                // calculate delta alpha
                for (int i = 0; i < numberOflabeledPoints; i++) {
                    double deltaAlpha = learnRate * (1.0 - z[i]*getY(i));
                    alpha[i] = Math.Max(0.0, alpha[i] + deltaAlpha);
                }
            }

            z = null;
        }

        // see http://www.svms.org/training/CaCr.pdf page 12
        void learnKernelAdatronAlgorithmWithBias() {
            alpha = new double[numberOflabeledPoints];
            z = new double[numberOflabeledPoints];

            double lamdaTMinus1 = System.Double.NaN, lamdaTMinus2 = System.Double.NaN;
            double wTMinus1 = System.Double.NaN, wTMinus2 = System.Double.NaN;

            for (uint iteration = 0; iteration < maximumNumberOfIterations; iteration++) {
                if( iteration > 1 ) {
                    double divisionDivdend = lamdaTMinus1 - lamdaTMinus2;
                    double divisionDivisor = wTMinus1 - wTMinus2;
                    lamda = lamdaTMinus1 - wTMinus1 * (divisionDivdend / divisionDivisor);
                }
                else if (iteration == 0) {
                    lamda = mu;
                }
                else if (iteration == 1) {
                    lamda = -mu;
                }

                //Console.WriteLine("lamda <{0}, {1}, {2}>", lamda, lamdaTMinus1, lamdaTMinus2);
                //Console.WriteLine("w <{0}, {1}>", wTMinus1, wTMinus2);

                Parallel.For(0, numberOflabeledPoints, i => z[i] = calcKernelizedSum((int)i));

                // calculate delta alpha
                for (int i = 0; i < numberOflabeledPoints; i++) {
                    double deltaAlpha = learnRate * (1.0 - z[i]*getY(i) - lamda*getY(i));
                    alpha[i] = Math.Max(0.0, alpha[i] + deltaAlpha);
                }

                // save history of values
                lamdaTMinus2 = lamdaTMinus1;
                lamdaTMinus1 = lamda;

                double w = 0.0;
                for (int j = 0; j < numberOflabeledPoints; j++) {
                    w += alpha[j]*getY(j);
                }

                // save history of values
                wTMinus2 = wTMinus1;
                wTMinus1 = w;

                double margin = calcMargin(z);
                if (margin >= 0.99995) {
                    break;
                }
            }

            z = null;
        }


        // helper function which calculates the sum of the kernels
        double calcKernelizedSum(int i) {
            double sum = 0.0;

            for (int j = 0; j < numberOflabeledPoints; j++) {
                sum += alpha[j] * getY(j) * kernel.calc(labeledPoints[i].Item1, labeledPoints[j].Item1);
            }

            return sum;
        }

        double calcMargin(double[] z) {
            double minZi = System.Double.MaxValue;
            for (int i = 0; i < numberOflabeledPoints; i++) {
                if (getY(i) == 1.0) {
                    minZi = Math.Min(minZi, z[i]);
                }
            }

            double maxZi = System.Double.MinValue;
            for (int i = 0; i < numberOflabeledPoints; i++) {
                if (getY(i) == -1.0) {
                    maxZi = Math.Max(maxZi, z[i]);
                }
            }

            return 0.5 * (minZi - maxZi);
        }

        double getY(int index) {
            return labeledPoints[index].Item2;
        }

        uint numberOflabeledPoints {
            get {
                return (uint)labeledPoints.Count;
            }
        }

        double[] alpha;
        double[] z;

        double lamda;

        uint maximumNumberOfIterations;

        bool withBias;

        double mu; // learn parameter for training with bias
        double learnRate;
        IKernel kernel;
        public IList<Tuple<double[], double>> labeledPoints = new List<Tuple<double[], double>>();
    }

    public interface IKernel {
        double calc(double[] a, double[] b);
    }

    public class GaussianKernel : IKernel {
        public GaussianKernel(double sigma) {
            this.sigma = sigma;
        }

        public double calc(double[] a, double[] b) {
            double squaredDistance = 0.0;

            for (int i = 0; i < a.Length; i++) {
                double diff = a[i] - b[i];
                squaredDistance += diff*diff;
            }

            return Math.Exp(- squaredDistance / (2.0*sigma));
        }

        double sigma;
    } 
}
