using WhiteSphereEngine.math;

namespace WhiteSphereEngine.geometry {
    class Plane {
        // see https://en.wikipedia.org/wiki/Line–plane_intersection#Algebraic_form
        public static double calcD(SpatialVectorDouble p0, SpatialVectorDouble n, SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection) {
            return SpatialVectorDouble.dot((p0 - rayOrigin), n) / SpatialVectorDouble.dot(rayDirection, n);
        }

        // calculate the w value of a plane, p0 is a point on the plane
        public static double calcW(SpatialVectorDouble p0, SpatialVectorDouble n) {
            return SpatialVectorDouble.dot(p0, n);
        }

        public static double dot3(SpatialVectorDouble a, SpatialVectorDouble b) {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }
    }
}
