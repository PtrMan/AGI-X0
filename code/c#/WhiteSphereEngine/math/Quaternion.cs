using System;
using System.Diagnostics;

namespace WhiteSphereEngine.math {
    public struct Quaternion {
        public double
            i, // i
            j, // j
            k, // k
            scalar;
        
        // see https://de.mathworks.com/help/aeroblks/quaternionmultiplication.html
        public static Quaternion operator *(Quaternion q, Quaternion r) {
            Quaternion result = new Quaternion();

            result.scalar = r.scalar*q.scalar - r.i*q.i - r.j*q.j - r.k*q.k;
            result.i = r.scalar*q.i + r.i*q.scalar - r.j*q.k + r.k*q.j;
            result.j = r.scalar*q.j + r.i*q.k + r.j*q.scalar - r.k*q.i;
            result.k = r.scalar*q.k - r.i*q.j + r.j*q.i + r.k*q.scalar;

            return result;
        }

        // see https://de.mathworks.com/help/aeroblks/quaterniondivision.html
        // the division a a scaled multiplication
        public static Quaternion operator /(Quaternion q, Quaternion r) {
            Quaternion mulResult = q * r;
            double lengthOfRSquared = r.norm1;

            Quaternion result = new Quaternion();
            result.scalar = mulResult.scalar / lengthOfRSquared;
            result.i = mulResult.i / lengthOfRSquared;
            result.j = mulResult.j / lengthOfRSquared;
            result.k = mulResult.k / lengthOfRSquared;
            return result;
        }
        
        // this quaternion is the initial quaternion, calculate the required rotation to final
        public Quaternion differenceForUnity(Quaternion final) {
            // see http://stackoverflow.com/a/4372718
            return final * this.inverseForUnity;
        }

        // this quaternion is the initial quaternion, calculate the required rotation to final
        public Quaternion difference(Quaternion final) {
            // see http://stackoverflow.com/a/4372718
            return final * this.inverse;
        }

        // assumption is that the magnitude of the quaternion is one
        // see https://de.mathworks.com/help/aeroblks/quaternioninverse.html
        public Quaternion inverseForUnity {
            get {
                Debug.Assert(magnitude - 1.0 < double.Epsilon);

                Quaternion result = new Quaternion();
                result.scalar = scalar;
                result.i = -i;
                result.j = -j;
                result.k = -k;
                return result;
            }
        }

        // see https://de.mathworks.com/help/aeroblks/quaternioninverse.html
        public Quaternion inverse {
            get {
                Quaternion result = new Quaternion();
                result.scalar = scalar / norm1;
                result.i = -i / norm1;
                result.j = -j / norm1;
                result.k = -k / norm1;
                return result;
            }
        }

        public Quaternion normalized() {
            Trace.Assert(magnitude > 0.0);
            double invMagnitude = 1.0 / magnitude;

            Quaternion result = new Quaternion();
            result.i = i * invMagnitude;
            result.j = j * invMagnitude;
            result.k = k * invMagnitude;
            result.scalar = scalar * invMagnitude;
            return result;
        }

        public double magnitude {
            get {
                // see https://www.vcalc.com/wiki/vCalc/Quaternion+Magnitude
                return System.Math.Sqrt(mangitudeSquared);
            }
        }

        public double mangitudeSquared {
            get {
                return norm1;
            }
        }
        
        public double norm1 {
            get {
                return scalar*scalar + i*i + j*j + k*k;
            }
        }
    }

    public class QuaternionUtilities {
        public static Quaternion makeFromAxisAndAngle(SpatialVectorDouble axis, double angle) {
            // source http://web.archive.org/web/20060914224155/http://web.archive.org:80/web/20041029003853/http://www.j3d.org/matrix_faq/matrfaq_latest.html#Q50
            // Q56. How do I convert a rotation axis and angle to a quaternion?
            SpatialVectorDouble normalizedAxis = axis.normalized();

            double sinAngle = System.Math.Sin(angle / 2.0);
            double cosAngle = System.Math.Cos(angle / 2.0);

            Quaternion result = new Quaternion();
            result.i = normalizedAxis.x * sinAngle;
            result.j = normalizedAxis.y * sinAngle;
            result.k = normalizedAxis.z * sinAngle;
            result.scalar = cosAngle;
            return result;
        }

        public static Quaternion makeFromEulerAngles(double x, double y, double z) {
            // from http://web.archive.org/web/20060914224155/http://web.archive.org:80/web/20041029003853/http://www.j3d.org/matrix_faq/matrfaq_latest.html
            // Q60. How do I convert Euler rotation angles to a quaternion?

            SpatialVectorDouble
                basevectorX = new SpatialVectorDouble(new double[]{1,0,0}),
                basevectorY = new SpatialVectorDouble(new double[]{0,1,0}),
                basevectorZ = new SpatialVectorDouble(new double[]{0,0,1});

            Quaternion
                quaternionX = makeFromAxisAndAngle(basevectorX, x),
                quaternionY = makeFromAxisAndAngle(basevectorY, y),
                quaternionZ = makeFromAxisAndAngle(basevectorZ, z);

            return (quaternionX * quaternionY) * quaternionZ;
        }

        public static Matrix convToRotationMatrix3(Quaternion q) {
            // see https://de.mathworks.com/help/aeroblks/quaternionrotation.html

            double ii = q.i * q.i;
            double jj = q.j * q.j;
            double kk = q.k * q.k;
            double ij = q.i * q.j;
            double ik = q.i * q.k;
            double jk = q.j * q.k;

            double si = q.scalar * q.i;
            double sj = q.scalar * q.j;
            double sk = q.scalar * q.k;

            return new Matrix(new double[] {
                1.0 - 2.0*jj - 2.0*kk, 2.0*(ij+sk)          , 2.0*(ik-sj),
                2.0*(ij - sk)        , 1.0 - 2.0*ii - 2.0*kk, 2.0*(jk+si),
                2.0*(ik + sj)        , 2.0*(jk-si)          , 1.0 - 2.0*ii - 2.0*jj
            }, 3);
        }

        public static Matrix convToRotationMatrix4(Quaternion q) {
            return MatrixUtilities.convfrom3to4Matrix(convToRotationMatrix3(q));
        }

        public static Quaternion makeIdentity() {
            Quaternion result = new Quaternion();
            result.i = 0;
            result.j = 0;
            result.k = 0;
            result.scalar = 1;
            return result;
        }
    }
}
