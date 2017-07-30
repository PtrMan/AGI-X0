using System.Diagnostics;

namespace WhiteSphereEngine.math.control {
    public class QuaternionFeedbackRegulatorForSpacecraft {
        // paper "Quaternion Feedback Regulator for Large Angle Maneuvers of Underactuated Spacecraft" [QuaternionFeedbackRegulator]
        // URL http://www.dcsc.tudelft.nl/~bdeschutter/private_20100705_acc_2010/data/papers/1771.pdf


        // this is after the design example in [QuaternionFeedbackRegulator] because an aproximation of an cube body is for now sufficient

        // just valid for the example of the PDF

        // \param J... : principal moments of inertia of the body
        // \param w1 ... w3 : angular velocity of the body-fixed frame (relative to the body)
        // \param q1 ... q4 : error quaternion, is the difference of the current orientation quaternion to the target one

        // \param aCoefficient
        // \param gamma positive scalar gain
        // \param d positive scalar gain, see equation (33)
        // \param k positive scalar gain, see equation (33)
        // \param betaOne configuration coefficient (see paper)
        public static void calcControl(
            double J11, double J22, double J33,
            double q1, double q2, double q3, double q4,
            double w1, double w2, double w3,

            out double q1Dot, out double q2Dot, out double q3Dot, out double q4Dot,
            out double w1Dot, out double w2Dot, out double w3Dot,
            
            double aCoefficient,
            double gamma,
            double d, double k,
            double betaOne
        ) {
            Matrix a = calcA(J11, J22, J33, q2, q3, w2, w3);
            

            // after equation [QuaternionFeedbackRegulator] (37)
            {
                double _1div2J11Factor = 1.0 / (2.0 * J11);
                q1Dot = _1div2J11Factor * (J11 * (q4 * w1 + q2 * w3 - q3 * w2));
                w1Dot = _1div2J11Factor * (2.0 * (J22 - J33) * w2 * w3);
            }

            // after equation [QuaternionFeedbackRegulator] (38)
            {
                q2Dot = 0.5 * (q3 * w1 + q4 * w2 - q1 * w3);
                q3Dot = 0.5 * (-q2 * w1 + q1 * w2 + q4 * w3);
            }

            // after equation [QuaternionFeedbackRegulator] (39)
            {
                q4Dot = -0.5 * (q1 * w1 + q2 * w2 + q3 * w3);
            }

            // after equation [QuaternionFeedbackRegulator] (40)
            {
                // the term (vector) after the _1divJ22J33 factor
                double term0_0 = J33 * (J33 - J11) * w1 * w3;
                double term0_1 = J22 * w2 * (J11 * w1 - J22 * w3);

                // the ( a_d^+(x) b(x) ) / (J_22 * J_33) term as a vector
                Matrix adxbxTermVector;
                {
                    double divisor = J22 * J33;
                    adxbxTermVector = (calcAPlusd(a, betaOne).scale(calcB(gamma, aCoefficient, w1, q1, w1Dot, q1Dot))).scale(1.0 / divisor);
                }

                // the - P_d(x) / (J22*J33) matrix
                Matrix factorMatrixMinusPdxDivJ33J33;
                {
                    Matrix Pdx = calcP(a);
                    double scale = -1.0 / (J22 * J33);
                    factorMatrixMinusPdxDivJ33J33 = Pdx.scale(scale);

                }

                // the big remaining vector
                Matrix remainingVector = new Matrix(new double[]{
                    J33*(J33 - J11)*w1*w3 - d*w2-k*q2,
                    J22*w2*(J11*w1 - J22*w3) - d*w3 - k*q3
                }, 1);

                Matrix factorMatrixMinusPdxDivJ33J33MulremainingVector = factorMatrixMinusPdxDivJ33J33 * remainingVector;

                double _1divJ22J33Factor = 1.0 / (J22 * J33);

                w2Dot = _1divJ22J33Factor * term0_0 + adxbxTermVector[0, 0] - factorMatrixMinusPdxDivJ33J33MulremainingVector[0, 0];
                w3Dot = _1divJ22J33Factor * term0_1 + adxbxTermVector[1, 0] - factorMatrixMinusPdxDivJ33J33MulremainingVector[1, 0];
            }
        }


        // after equation [QuaternionFeedbackRegulator] (41)
        // just valid for the example of the PDF
        static Matrix calcA(double J11, double J22, double J33, double q2, double q3, double w2, double w3) {
            double _1div2J11Factor = 1.0 / (2.0 * J11);
            return new Matrix(new double[]{
                -J11*q3 + (J22 - J33)*w3,
                J11*q2 + (J22 - J33)*w2
            }, 1);
        }

        // after equation [QuaternionFeedbackRegulator] (19)
        static double calcB(double gamma, double a,   double w_1, double q_1, double w_1_dot, double q_1_dot) {
            // equation (19) is
            // b(x) = -L_r^2 phi(x_u) - 2 * gamma * L_r phi(x_u) - gamma^2 * phi(x_u)
            // where we already have all the components as variables, namely
            //  L_r phi(x_u)    is equivalent to  phi^dot(x_u)  after equation (15)
            //  phi(x_u)          is equivalent to  phi(x_u)
            // where
            //  phi(x_u) = w_u + a * q_u   after equation (13)
            // where
            //  w_u is equivlanet with w_1 (see same site in the paper under equation (8) )
            //  q_u is equivalent with q_1 (see same site in the paper under equation (8) )
            // the derivative of w_1 and q_1 is also already known

            double
                w_u = w_1,
                q_u = q_1,
                w_u__dot = w_1_dot,
                q_u__dot = q_1_dot;

            double
                phi__x_u = w_u + a * q_u,
                phi__x_u__dot = w_u__dot + a * q_u__dot;

            double
                L_r_phi__x_u = phi__x_u__dot, // equivalent with the derivative of (13)
                L_r_exp_2_phi__x_u = L_r_phi__x_u * L_r_phi__x_u;

            return -L_r_exp_2_phi__x_u - 2.0*gamma * L_r_phi__x_u - gamma*gamma * phi__x_u;
        }

        // after equation [QuaternionFeedbackRegulator] (22)
        static Matrix calcAPlus(Matrix a) {
            if (new SpatialVectorDouble(a).length < double.Epsilon) {
                return new Matrix(new double[] { 0, 0 }, 1);
            }
            else {
                return a.scale(1.0 / new SpatialVectorDouble(a).lengthSquared);
            }
        }

        // a^+_d(x)
        // after equation [QuaternionFeedbackRegulator] (25)
        // betaOne is an coefficient
        static Matrix calcAPlusd(Matrix a, double betaOne) {
            if (new SpatialVectorDouble(a).length >= betaOne) {
                return a.scale(1.0 / new SpatialVectorDouble(a).lengthSquared);
            }
            else {
                return a.scale(betaOne * betaOne);
            }
        }

        // after equation [QuaternionFeedbackRegulator] (22)
        static Matrix calcP(Matrix a) {
            return MatrixUtilities.identity(2) + calcAPlus(a) * a.transpose;
        }
    }
}