using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.rigidBody {
    // translation of https://github.com/PtrMan/SpaceSimCore/blob/master/src/physics/InertiaHelper.cpp
    public class InertiaHelper {
        private InertiaHelper() {}

        public static Matrix calcInertiaTensorForSolidEllipsoid(double mass, double a, double b, double c) {
            return new Matrix(new double[] {
                (1.0 / 5.0) * (b * b + c * c), 0, 0,
                0, (1.0 / 5.0) * (a * a + c * c), 0,
                0, 0, (1.0 / 5.0) * (a * a + b * b)
            }, 3);
        }

        public static Matrix calcInertiaTensorForSolidSphere(double mass, double radius) {
            return calcInertiaTensorForSolidEllipsoid(mass, radius, radius, radius);
        }

        public static Matrix calcInertiaTensorForCube(double mass, double height, double width, double depth) {
            double ih = (mass / 12.0) * (width * width + depth * depth);
            double iw = (mass / 12.0) * (depth * depth + height * height);
            double id = (mass / 12.0) * (width * width + height * height);

            // some guesswork
            // TODO< test if correct >
            return new Matrix(new double[] {
                ih, 0, 0,
                0, iw, 0,
                0, 0, id
            }, 3);
        }
    }
}
