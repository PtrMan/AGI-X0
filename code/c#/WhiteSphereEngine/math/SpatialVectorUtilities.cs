namespace WhiteSphereEngine.math {
    class SpatialVectorUtilities {
        public static SpatialVectorDouble toVector4(SpatialVectorDouble vector, double w = 1) {
            return new SpatialVectorDouble(new double[] { vector.x, vector.y, vector.z, w });
        }

        public static SpatialVectorDouble toVector3(SpatialVectorDouble vector) {
            return new SpatialVectorDouble(new double[] { vector.x, vector.y, vector.z});
        }
    }
}
