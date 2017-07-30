using System.Diagnostics;

namespace WhiteSphereEngine.math {
    public struct SpatialVectorDouble {
        public SpatialVectorDouble(uint size) {
            matrix = new Matrix(1, size);
        }

        public SpatialVectorDouble(Matrix matrix) {
            Debug.Assert(matrix.width == 1);
            this.matrix = matrix;
        }

        public SpatialVectorDouble(double[] arr) {
            matrix = new Matrix(arr, 1);
        }

        public uint height {
            get {
                return (uint)matrix.height;
            }
        }

        private Matrix matrix;

        public Matrix asMatrix {
            get {
                return matrix;
            }
        }
        
        public static SpatialVectorDouble operator +(SpatialVectorDouble left, SpatialVectorDouble right) {
            return new SpatialVectorDouble(left.matrix + right.matrix);
        }

        public static SpatialVectorDouble operator -(SpatialVectorDouble left, SpatialVectorDouble right) {
            return new SpatialVectorDouble(left.matrix - right.matrix);
        }

        public SpatialVectorDouble scale( double value) {
            SpatialVectorDouble result = new SpatialVectorDouble((uint)matrix.arr.Length);
            for (int i = 0; i < matrix.arr.Length; i++) {
                result[i] = matrix.arr[i] * value;
            }

            return result;
        }

        public SpatialVectorDouble componentMultiplication(SpatialVectorDouble other) {
            Debug.Assert(other.matrix.arr.Length == matrix.arr.Length);
            SpatialVectorDouble result = new SpatialVectorDouble((uint)matrix.arr.Length);
            for (int i = 0; i < matrix.arr.Length; i++) {
                result[i] = matrix.arr[i] * other.matrix.arr[i];
            }

            return result;
        }

        public SpatialVectorDouble deepClone() {
            return new SpatialVectorDouble(matrix.deepClone());
        }

        public SpatialVectorDouble normalized() {
            return scale(1.0 / length);
        }

        public double lengthSquared {
            get {
                return dot(this, this);
            }
        }

        public double length {
            get {
                return System.Math.Sqrt(lengthSquared);
            }
        }

        public double this[int i] {
            get { return matrix.arr[i]; }
            set { matrix.arr[i] = value; }
        }

        public double x {
            set {
                matrix.arr[0] = value;
            }
            get {
                return matrix.arr[0];
            }
        }

        public double y {
            set {
                matrix.arr[1] = value;
            }
            get {
                return matrix.arr[1];
            }
        }

        public double z {
            set {
                matrix.arr[2] = value;
            }
            get {
                return matrix.arr[2];
            }
        }

        // used for plane equation or 4th dimension
        public double w {
            set {
                matrix.arr[3] = value;
            }
            get {
                return matrix.arr[3];
            }
        }
        


        public static double dot(SpatialVectorDouble a, SpatialVectorDouble b) {
            Trace.Assert(a.matrix.arr.Length == b.matrix.arr.Length);

            double result = (double)0;

            for (int i = 0; i < a.matrix.arr.Length; i++) {
                result +=( a.matrix.arr[i] * b.matrix.arr[i]);
            }

            return result;
        }

        public static SpatialVectorDouble crossProduct(SpatialVectorDouble a, SpatialVectorDouble b) {
            double x = a[1] * b[2] - a[2] * b[1];
            double y = a[2] * b[0] - a[0] * b[2];
            double z = a[0] * b[1] - a[1] * b[0];
            return new SpatialVectorDouble(new double[]{ x, y, z });
        }
    }
}
