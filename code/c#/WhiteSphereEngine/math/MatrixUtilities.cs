using System;
using System.Diagnostics;

namespace WhiteSphereEngine.math {
    public class MatrixUtilities {
        public static Matrix calcLocalToGlobalRotationMatrix(SpatialVectorDouble rotation) {
            Matrix transformationMatrix = Matrix44.createRotationX(rotation.x);
            transformationMatrix = transformationMatrix * Matrix44.createRotationY(rotation.y);
            transformationMatrix = transformationMatrix * Matrix44.createRotationZ(rotation.z);
            return transformationMatrix;
        }

        public static Matrix calcGlobalToLocalRotationMatrix(SpatialVectorDouble rotation) {
            return calcLocalToGlobalRotationMatrix(rotation).inverse();
        }

        public static Matrix calcLocalToGlobalTranslationMatrix(SpatialVectorDouble translation) {
            return Matrix44.createTranslation(translation.x, translation.y, translation.z);
        }
        
        public static Matrix calcGlobalToLocalTranslationMatrix(SpatialVectorDouble translation) {
            return calcLocalToGlobalTranslationMatrix(translation).inverse();
        }

        public static Matrix calcGlobalToLocalRotationAndTranslationMatrix(SpatialVectorDouble translation, SpatialVectorDouble rotation) {
            return calcGlobalToLocalTranslationMatrix(translation) * calcGlobalToLocalRotationMatrix(rotation);
        }

        public static Matrix calcLocalToGlobalRotationAndTranslationMatrix(SpatialVectorDouble translation, SpatialVectorDouble rotation) {
            return calcLocalToGlobalTranslationMatrix(translation) * calcLocalToGlobalRotationMatrix(rotation);
        }


        public static Matrix extractColumn3(Matrix matrix, uint columnIndex) {
            double[] rawArray = new double[]{matrix[0, (int)columnIndex], matrix[1, (int)columnIndex], matrix[2, (int)columnIndex]};
            return new Matrix(rawArray, 1);
        }

        public static double minCoeff(Matrix vector, out int index) {
            Trace.Assert(vector.columns == 1);

            index = 0;
            double minValue = vector[0, 0];

            for( int i = 0; i < vector.rows; i++ ) {
                if( vector[i, 0] < minValue ) {
                    minValue = vector[i, 0];
                    index = i;
                }
            }

            return minValue;
        }

        // inspired by eigen block method
        // https://eigen.tuxfamily.org/dox/group__TutorialBlockOperations.html
        public static MatrixRef block(Matrix matrix, uint startRow, uint startColumn, uint rows, uint columns) {
            return MatrixRef.createSubmatrix(matrix, startRow, startColumn, rows, columns);
        }

        public static Matrix identity(int width) {
            Matrix created = Matrix.makeByRowsAndColumns(width, width);
            // uncommented because C# does this automatically
            //for ( int iRow = 0; iRow < width; iRow++ ) {
            //    for( int iColumn = 0; iColumn < width; iColumn++ ) {
            //        created[iRow, iColumn] = 0;
            //    }
            //}
            for( int i = 0; i < width; i++ ) {
                created[i, i] = 1;
            }
            return created;
        }

        public static Matrix convfrom3to4Matrix(Matrix matrix) {
            Trace.Assert(matrix.columns == 3 && matrix.rows == 3);
            Matrix result = new Matrix(4, 4);
            for( int iRow = 0; iRow < 3; iRow++ ) {
                for( int iColumn = 0; iColumn < 3; iColumn++ ) {
                    result[iRow, iColumn] = matrix[iRow, iColumn];
                }
            }

            // we don't have to zero out the side values because they are already zero thanks to C#

            result[3, 3] = 1.0; // identity

            return result;
        }
    }

    // references a (sub)matrix and does operation on the referenced (sub)matrix
    // inspired by eigen
    public class MatrixRef {
        public static MatrixRef createSubmatrix(Matrix referenced, uint startRow, uint startColumn, uint rows, uint columns) {
            return new MatrixRef(referenced, startRow, startColumn, rows, columns);
        }
        
        private MatrixRef(Matrix referenced, uint startRow, uint startColumn, uint rows, uint columns) {
            // trace because it's not called that often
            Trace.Assert(startRow >= 0 && startRow < referenced.rows);
            Trace.Assert(startColumn >= 0 && startColumn < referenced.columns);
            Trace.Assert(rows >= 0 && startRow + rows <= referenced.rows);
            Trace.Assert(columns >= 0 && startColumn + columns <= referenced.columns);

            this.referenced = referenced;
            this.startRow = startRow;
            this.startColumn = startColumn;
            this.privateCachedRows = rows;
            this.privateCachedColumns = columns;
        }

        public void div(double value) {
            for (uint rowI = 0; rowI < privateCachedRows; rowI++) {
                for (uint columnI = 0; columnI < privateCachedColumns; columnI++) {
                    referenced[(int)(startRow + rowI), (int)(startColumn + columnI)] /= value;
                }
            }
        }
        
        public Matrix scaleAndDeref(double value) {
            Matrix resultDerefed = Matrix.makeByRowsAndColumns((int)privateCachedRows, (int)privateCachedColumns);

            for (uint rowI = 0; rowI < privateCachedRows; rowI++) {
                for (uint columnI = 0; columnI < privateCachedColumns; columnI++) {
                    resultDerefed[(int)rowI, (int)columnI] = referenced[(int)(startRow + rowI), (int)(startColumn + columnI)] * value;
                }
            }

            return resultDerefed;
        }

        public void subEqual(Matrix left) {
            for (uint rowI = 0; rowI < privateCachedRows; rowI++) {
                for (uint columnI = 0; columnI < privateCachedColumns; columnI++) {
                    referenced[(int)(startRow + rowI), (int)(startColumn + columnI)] -= left[(int)rowI, (int)columnI];
                }
            }
        }

        public 

        uint startRow, startColumn, privateCachedRows, privateCachedColumns;
        Matrix referenced;
    }
}
