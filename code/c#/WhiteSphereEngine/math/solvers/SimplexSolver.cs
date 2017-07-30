namespace WhiteSphereEngine.math.solvers {
    // simplex solver

    // translated from my C++ code
    class SimplexSolver {
        public class Variable {
            public enum EnumType {
                NAMEME, // TODO< name me >
                LASTLINE, // TODO< give proper name >
                UNUSED,
            }

            public Variable() {
                type = EnumType.UNUSED;
            }

            public Variable(EnumType type, int identifier) {
                this.type = type;
                this.identifier = identifier;
            }

            public int identifier;
            public EnumType type;
        }

        public struct Result {
            public enum EnumSolverState {
                FOUNDSOLUTION,
                UNBOUNDEDSOLUTION,
                TOOMANYITERATIONS,
            }

            public Result(EnumSolverState state) {
                this.state = state;
            }

            public EnumSolverState state;
        }

        public Matrix matrix;

        const uint SizeOfVariableArray = 256;

        // FIXME< not fixed size allocator for special cases >
        public Variable[] variables = new Variable[SizeOfVariableArray];

        public SimplexSolver() { }

        // see https://www.youtube.com/watch?v=Axg9OhJ4cjg (german)           bottom row is negative unlike in the video
        // see http://de.slideshare.net/itsmedv91/special-cases-in-simplex    special cases

        // TODO< handle other special cases >


        public Result iterate() {
            setupVariables();



            // for now we use a iteration counter to protect us from infinite loops
            // TODO< implement loop detection scheme, basic idea
            //       * we manage a fixed sized array as a sliding window or the hashes of the operations
            //       * if we detect a loop we apply bland's rule to resolve it (is the rule detection realy neccesary?)
            //
            //
            //       * every operation (pivot column, pivot row, number of pivot element itself) gets an hash
            //       * at each iteration we subtract the previous hash and add the current hash
            //       * if the hash doesn't change in n iteration(where n == 2 for the simple loop) we are looping
            // >

            // TODO< implement https://en.wikipedia.org/wiki/Bland's_rule >

            uint iterationCounter = 0;
            const uint MaximalIterationCounter = 128;

            for (;;) {
                iterationCounter++;
                if (iterationCounter > MaximalIterationCounter) {
                    return new Result(Result.EnumSolverState.TOOMANYITERATIONS);
                }

                bool foundMaximumColumn;

                int pivotColumnIndex = searchMinimumColumn(out foundMaximumColumn);
                // all values in the target value row are < 0.0, done
                if (!foundMaximumColumn) {
                    return new Result(Result.EnumSolverState.FOUNDSOLUTION);
                }

                //std.cout << "min column " << pivotColumnIndex << std.endl;

                if (areAllEntriesOfPivotColumnNegativeOrZero(pivotColumnIndex)) {
                    // solution is unbounded

                    return new Result(Result.EnumSolverState.UNBOUNDEDSOLUTION);
                }

                // divide numbers of pivot column with right side and store in temporary vector
                Matrix minRatioVector = divideRightSideWithPivotColumnWhereAboveZero(pivotColumnIndex);

                //std.cout << "temporary vector" << std.endl;
                //std.cout << minRatioVector << std.endl;

                int minIndexOfTargetFunctionCoefficient = getMinIndexOfMinRatioVector(minRatioVector);
                bool positiveMinRatioExists = doesPositiveMinRatioExist(minRatioVector);
                if (!positiveMinRatioExists) {
                    // solution is unbounded

                    return new Result(Result.EnumSolverState.UNBOUNDEDSOLUTION);
                }

                int pivotRowIndex = minIndexOfTargetFunctionCoefficient;

                //std.cout << "pivotRowIndex " << pivotRowIndex << std.endl;

                // C++ code was matrix(pivotRowIndex, pivotColumnIndex)
                double pivotElement = matrix[pivotRowIndex, pivotColumnIndex];

                // divide the pivot row with the pivot element
                MatrixUtilities.block(matrix, (uint)pivotRowIndex, 0, 1, matrix.columns).div(pivotElement);
                

                // TODO< assert that pivot elemnt is roughtly 1.0 >



                // do pivot operation

                for (int pivotRowIteration = 0; pivotRowIteration < matrix.rows; pivotRowIteration++) {
                    if (pivotRowIteration == pivotRowIndex) {
                        continue;
                    }

                    double iterationElementAtPivotColumn = matrix[pivotRowIteration, pivotColumnIndex];


                    //matrix.block(pivotRowIteration, 0, 1, matrix.columns) -= (matrix.block(pivotRowIndex, 0, 1, matrix.columns) * iterationElementAtPivotColumn);
                    MatrixUtilities.block(matrix, (uint)pivotRowIteration, 0, 1, matrix.columns).subEqual(
                        MatrixUtilities.block(matrix, (uint)pivotRowIndex, 0, 1, matrix.columns).scaleAndDeref(iterationElementAtPivotColumn)
                    );
                }

                // set the variable identifier that we know which row of the matrix is for which variable
                variables[pivotRowIndex] = new Variable(Variable.EnumType.NAMEME, pivotColumnIndex);

                //std.cout << matrix << std.endl;

            }
        }


        protected void setupVariables() {
            // TODO
        }

        protected int searchMinimumColumn(out bool foundMinimumColumn) {
            foundMinimumColumn = false;

            int matrixRowCount = (int)matrix.rows;

            double minValue = 0.0;
            int minColumn = -1;

            for (int iterationColumn = 0; iterationColumn < matrix.columns - 1; iterationColumn++) {
                if (matrix[matrixRowCount - 1, iterationColumn] < minValue) {
                    minValue = matrix[matrixRowCount - 1, iterationColumn];
                    minColumn = iterationColumn;
                    foundMinimumColumn = true;
                }
            }

            return minColumn;
        }

        protected Matrix/*<Type, Eigen.Dynamic, 1>*/ divideRightSideWithPivotColumnWhereAboveZero(int pivotColumnIndex) {
            int matrixRowCount = (int)matrix.rows;
            int matrixColumnCount = (int)matrix.columns;

            Matrix/*<Type, Eigen.Dynamic, 1>*/ result = Matrix.makeByRowsAndColumns((int)matrix.rows-1, 1);

            for (int rowIndex = 0; rowIndex < matrixRowCount - 1; rowIndex++) {
                if (matrix[rowIndex, pivotColumnIndex] > 0.0) {
                    result[rowIndex, 0] = matrix[rowIndex, matrixColumnCount - 1] / matrix[rowIndex, pivotColumnIndex];
                }
                else {
                    // ASK< we set it just to the right side >
                    result[rowIndex, 0] = matrix[rowIndex, matrixColumnCount - 1];
                }
            }

            return result;
        }

        protected bool areAllEntriesOfPivotColumnNegativeOrZero(int pivotColumnIndex) {
		    for(int rowIndex = 0; rowIndex<matrix.rows-1; rowIndex++ ) {
			    if(matrix[rowIndex, pivotColumnIndex] > 0.0 ) {
				    return false;
			    }
		    }

		    return true;
	    }

        protected static int getMinIndexOfMinRatioVector(Matrix/*<Type, Eigen.Dynamic, 1>*/ vector) {
            int index;
            MatrixUtilities.minCoeff(vector, out index);
            return index;
        }

        protected static bool doesPositiveMinRatioExist(Matrix/*<Type, Eigen.Dynamic, 1>*/ vector) {
            for (int i = 0; i < vector.rows; i++) {
                if (vector[i, 0] > 0.0) {
                    return true;
                }
            }

            return false;
        }
    }
}
