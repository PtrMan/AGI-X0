using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace WhiteSphereEngine.math.algorithms {
    // translation of https://github.com/PtrMan/linopterixed/blob/master/code/d/linopterixed/linear/algorithms/GaussElimination.d

    public interface IStepType {
        void doOperationForRowsAndColumn(Matrix matrix, uint iRow, uint kRow, uint j);
    }

    public class DefaultStep : IStepType {
        public void doOperationForRowsAndColumn(Matrix matrix, uint iRow, uint kRow, uint j) {
            double divFactor = matrix[(int)kRow, (int)iRow] / matrix[(int)iRow, (int)iRow];
            matrix[(int)kRow, (int)j] = matrix[(int)kRow, (int)j] - divFactor * matrix[(int)iRow, (int)j];
        }
    }

    public struct GaussElimination {
        // in place
        public static void gaussElimination(IStepType StepType, Matrix matrix, bool withGaussJordan = true) {
            uint width = matrix.width;
            uint height = matrix.height;

            void rowOperation(uint iRow, uint kRow) {
                // split into two loops because we can't touch the value at [iRow, iRow], because we divide by it

                for (uint j = 0; j < iRow; j++) {
                    StepType.doOperationForRowsAndColumn(matrix, iRow, kRow, j);
                }

                for (uint j = iRow + 1; j < width; j++) {
                    StepType.doOperationForRowsAndColumn(matrix, iRow, kRow, j);
                }

                StepType.doOperationForRowsAndColumn(matrix, iRow, kRow, iRow);
            }



            // bring into echelon form
            for( uint iterationRow = 0; iterationRow < height; iterationRow++ ) {
                for (uint kRow = iterationRow + 1; kRow < height; kRow++ ) {
                    rowOperation(iterationRow, kRow);
                }
            }

            // calculate result matrix
            // this is the extension called gauss-jordan elimination
            if (withGaussJordan) {
                void multipleRowBy(uint row, double value) {
                    for(int i = 0; i < width; i++ ) {
                        matrix[(int)row, i] = matrix[(int)row, i] * value;
                    }
                }

                for( int bottomRow = (int)height-1; bottomRow >= 0; bottomRow-- ) { // loop must work with int because of underflow
                    multipleRowBy((uint)bottomRow, (double)1.0 / matrix[(int)bottomRow, (int)bottomRow]);

                    for( int iterationRow = 0; iterationRow < bottomRow; iterationRow++ ) {
                        rowOperation((uint)bottomRow, (uint)iterationRow);
                    }
                }
            }
        }

        public static void standardGaussElimination(Matrix matrix) {
	        gaussElimination(new DefaultStep(), matrix);
        }
    }
}
