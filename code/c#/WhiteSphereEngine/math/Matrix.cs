using System;
using System.Diagnostics;

using WhiteSphereEngine.math.algorithms;

namespace WhiteSphereEngine.math {
    public struct Matrix {
        internal double[] arr;
        int privateColumns;
        
        public Matrix(double[] arr, uint columns) {
            this.arr = arr;
            privateColumns = (int)columns;
        }

        public static Matrix makeByRowsAndColumns(int rows, int columns) {
            Trace.Assert(rows > 0 && columns > 0);
            return new Matrix((uint)columns, (uint)rows);
        }

        // TODO< make private >
        [System.Obsolete("use makeByRowsAndColumns() instead")]
        public Matrix(uint columns, uint rows) {
            arr = new double[columns * rows];
            privateColumns = (int)columns;
        }

        public uint columns {
            get {
                return (uint)privateColumns;
            }
        }

        public uint rows {
            get {
                return (uint)(arr.Length / privateColumns);
            }
        }

        public Matrix transpose {
            get {
                Matrix created = new Matrix(rows, columns);
                for( int resultRowI = 0; resultRowI < columns; resultRowI++ ) {
                    for( int resultColumnI = 0; resultColumnI < rows; resultColumnI++ ) {
                        created[resultRowI, resultColumnI] = this[resultColumnI, resultRowI];
                    }
                }
                return created;
            }
        }


        [System.Obsolete("use columns instead")]
        public uint width {
            get {
                return (uint)privateColumns;
            }
        }

        [System.Obsolete("use rows instead")]
        public uint height {
            get {
                return (uint)(arr.Length / privateColumns);
            }
        }

        public double this[int row, int column] {
            get {
                Debug.Assert(column < width);
                return arr[row * width + column];
            }
            set {
                Debug.Assert(column < width);
                arr[row * width + column] = value;
            }
        }

        public static Matrix operator +(Matrix left, Matrix right) {
            Trace.Assert(left.arr.Length == right.arr.Length);
            Trace.Assert(left.width == right.width);

            Matrix result = new Matrix(left.width, left.height);
            for (int i = 0; i < left.arr.Length; i++) {
                result.arr[i] = left.arr[i] + right.arr[i];
            }

            return result;
        }

        public static Matrix operator -(Matrix left, Matrix right) {
            Trace.Assert(left.arr.Length == right.arr.Length);
            Trace.Assert(left.width == right.width);

            Matrix result = new Matrix(left.width, left.height);
            for (int i = 0; i < left.arr.Length; i++) {
                result.arr[i] = left.arr[i] - right.arr[i];
            }

            return result;
        }

        public static Matrix operator *(Matrix left, Matrix right) {
            Trace.Assert(left.width == right.height);
            
            Matrix result = new Matrix(right.width, left.height);

            for( int resultY = 0; resultY < result.height; resultY++ ) {
                // TODO MAYBE PERFORMANCE< parallel for >

                for (int resultX = 0; resultX < result.width; resultX++ ) {
                    double dotResult;
                    { // dot product
                        dotResult = 0.0;

                        for (int i = 0; i < left.width; i++) {
                            dotResult += (left[resultY, i] * right[i, resultX]);
                        }
                    }
                    result[resultY, resultX] = dotResult;
                }
            }

            return result;
        }

        public Matrix scale(double factor) {
            Matrix result = new Matrix(width, height);

            for (int resultY = 0; resultY < result.height; resultY++) {
                for (int resultX = 0; resultX < result.width; resultX++) {
                    result[resultY, resultX] = this[resultY, resultX] * factor;
                }
            }

            return result;
        }

        public Matrix deepClone() {
            return new Matrix((double[])arr.Clone(), columns);
        }

        public Matrix inverse() {
            Trace.Assert(width == height, "Inverse only valid for nxn matrix!");
            uint size = width;

            Matrix internalMatrix = new Matrix(size * 2, size);

            // transfer input to internal matrix
            for (int row = 0; row < size; row++) {
                for (int column = 0; column < size; column++) {
                    internalMatrix[row, column] = this[row, column];
                }
            }
            // transfer identity to internal matrix
            for (int row = 0; row < size; row++) {
                for (int column = 0; column < size; column++) {
                    internalMatrix[row, column + (int)size] = (double)0.0;
                }
            }

            for(int i = 0; i < size; i++) {
                internalMatrix[i, i + (int)size] = (double)1.0;
            }

            GaussElimination.standardGaussElimination(internalMatrix);

            Matrix result = new Matrix(size, size);
            // transfer result from matrix to result
            for(int row = 0; row < size; row++) {
                for( int column = 0; column < size; column++) {
                    result[row, column] = internalMatrix[row, column + (int)size];
                }
            }

            return result;
        }
    }
}
