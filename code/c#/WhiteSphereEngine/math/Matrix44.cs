namespace WhiteSphereEngine.math {
    class Matrix44 {
        public static Matrix createIdentity() {
	        double one = 1.0;
            double zero = 0.0;
	
	        return new Matrix(new double[]{one, zero, zero, zero, zero, one, zero, zero, zero, zero, one, zero, zero, zero, zero, one}, 4);
        }

        private static Matrix createRotationXInternal(double cosValue, double sinValue) {
            double one = 1.0;
            double zero = 0.0;

            return new Matrix(new double[]{
                one, zero, zero, zero,
                zero, cosValue, -sinValue, zero,
                zero, sinValue, cosValue, zero,
                zero, zero, zero, one
            }, 4);
        }

        private static Matrix createRotationYInternal(double cosValue, double sinValue) {
	        double one = 1.0;
            double zero = 0.0;

	        return new Matrix(new double[]{
                cosValue, zero, sinValue, zero,
                zero, one, zero, zero,
                -sinValue, zero, cosValue, zero,
                zero, zero, zero, one
            }, 4);
        }

        private static Matrix createRotationZInternal(double cosValue, double sinValue) {
            double one = 1.0;
            double zero = 0.0;

            return new Matrix(new double[]{
                cosValue, -sinValue, zero, zero,
                sinValue, cosValue, zero, zero,
                zero, zero, one, zero,
                zero, zero, zero, one
            }, 4);
        }

        public static Matrix createRotationX(double angle) {
	        return createRotationXInternal(System.Math.Cos(angle), System.Math.Sin(angle));
        }

        public static Matrix createRotationY(double angle) {
	        return createRotationYInternal(System.Math.Cos(angle), System.Math.Sin(angle));
        }

        public static Matrix createRotationZ(double angle) {
	        return createRotationZInternal(System.Math.Cos(angle), System.Math.Sin(angle));
        }

        public static Matrix createTranslation(double x, double y, double z) {
	        Matrix result = createIdentity();
            result[0, 3] = x;
	        result[1, 3] = y;
	        result[2, 3] = z;

	        return result;
        }

        public static Matrix createScale(double x, double y, double z) {
	        Matrix result = createIdentity();
            result[0, 0] = x;
	        result[1, 1] = y;
	        result[2, 2] = z;
	        return result;
        }
    }
}
