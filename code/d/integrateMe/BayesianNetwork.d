module BayesianNetwork;

import std.algorithm.iteration : map;
import std.array : array;


// native integer
private alias size_t NtvInt;

// simple implementation of baysian networks
// https://en.wikipedia.org/wiki/Bayesian_network

/*struct Distribution {
	double[2] values; // for false and true  for now
}*/

struct Value {
	bool isFreeVariable; // is this a free variable

	bool value; // boolean value if it is not an free variable

	static Value makeTrue() {
		Value result;
		result.value = true;
		return result;
	}

	static Value makeFalse() {
		Value result;
		result.value = false;
		return result;
	}

	static Value makeFreeVariable() {
		Value result;
		result.isFreeVariable = true;
		return result;
	}
}

// for debugging
import std.stdio;

struct BayesianNetwork {
	// just for testing
	public final void testMe() {
		// we use the condiional propability
		// https://en.wikipedia.org/wiki/Conditional_probability

		double divisor = propabilityWithFreeVariables([Value.makeTrue(), Value.makeFreeVariable(), Value.makeTrue()]);
		double dividend = propabilityWithFreeVariables([Value.makeTrue(), Value.makeFreeVariable(), Value.makeFreeVariable()]);
		double prop = divisor/dividend;
	}

	private final double propabilityWithFreeVariables(Value[] args) {
		// search for free variable and replace it with all possibilities, do this recursivly

		foreach( i, iterationArg; args ) {
			if( iterationArg.isFreeVariable ) {
				// if so we sum up all possibilites (recursivly)

				double result = 0;
				result += (propabilityWithFreeVariables(dupAndReplaceAt(args, i, Value.makeFalse())));
				result += (propabilityWithFreeVariables(dupAndReplaceAt(args, i, Value.makeTrue())));
				return result;
			}
		}

		// we are here if we didn't find any free variables
		// we return the jointPropability function with the variables
		return jointPropabilityFunction(args.map!(v => v.value).array);
	}

	private final double jointPropabilityFunction(bool[] values) {
		bool[] innerFnLookupValueIndices(size_t[] valueIndices) {
			return valueIndices.map!(v => values[v]).array;
		}

		// converts the list from base n to a single number
		size_t innerFnCalcConditionalIndex(bool[] values) {
			if( values.length == 0 ) {
				return 0;
			}

			NtvInt multiplier = 1;
			size_t result = 0;

			for( int i = values.length-1; i >= 0; i-- ) {
				size_t conditionalIndexValue = values[i];
				result += (multiplier*(values[i] ? 1 : 0));
				multiplier *= 2; // multiply by number of possibilities
			}

			return result;
		}

		double resultPropability = 1;

		foreach( i, iterationValue; values ) {
			size_t[] conditionalIndices = matricesWithIndirections[i].conditionalIndices;
			bool[] conditionalValues = innerFnLookupValueIndices(conditionalIndices); // translate the indices to the values they point at
			size_t conditionalIndex = innerFnCalcConditionalIndex(conditionalValues); // convert the conditional values to one index which we can use to lookup the propability in the matrix/table

			writeln("mul ", matricesWithIndirections[i].matrix[conditionalIndex, (iterationValue == false) ? 0 : 1]);
			resultPropability *= matricesWithIndirections[i].matrix[conditionalIndex, (iterationValue == false) ? 0 : 1];
		}

		writeln("joint result=", resultPropability);

		return resultPropability;
	}


	// matrices for the different elements in the graph
	// 
	MatrixWithIndirection[] matricesWithIndirections;

	static struct MatrixWithIndirection {
		static MatrixWithIndirection make(ValueMatrix!double matrix, size_t[] conditionalIndices) {
			MatrixWithIndirection result;
			result.matrix = matrix;
			result.conditionalIndices = conditionalIndices;
			return result;
		}

		ValueMatrix!double matrix;

		size_t[] conditionalIndices; // indices of the conditional variables in the argument list to jointPropabilityFunction() or propabilityWithFreeVariables()
	}
}

// for testing
void main() {
	BayesianNetwork *bayesianNetwork = new BayesianNetwork;

	{ // grass wet
		ValueMatrix!double matrix = new ValueMatrix!double(2, 4);
		// index 0 is for false and 1 is for true, this is different to wikipedia
		matrix[0, 1] = 0.0;
		matrix[0, 0] = 1.0;
		matrix[1, 1] = 0.8;
		matrix[1, 0] = 0.2;
		matrix[2, 1] = 0.9;
		matrix[2, 0] = 0.1;
		matrix[3, 1] = 0.99;
		matrix[3, 0] = 0.01;
		size_t[] conditionalIndices = [1, 2];
		bayesianNetwork.matricesWithIndirections ~= BayesianNetwork.MatrixWithIndirection.make(matrix, conditionalIndices);
	}

	{ // sprinkler
		ValueMatrix!double matrix = new ValueMatrix!double(2, 2);
		matrix[0, 1] = 0.4;
		matrix[0, 0] = 0.6;
		matrix[1, 1] = 0.01;
		matrix[1, 0] = 0.99;
		size_t[] conditionalIndices = [2];
		bayesianNetwork.matricesWithIndirections ~= BayesianNetwork.MatrixWithIndirection.make(matrix, conditionalIndices);
	}

	{ // rain
		ValueMatrix!double matrix = new ValueMatrix!double(2, 1);
		matrix[0, 1] = 0.2;
		matrix[0, 0] = 0.8;
		size_t[] conditionalIndices = [];
		bayesianNetwork.matricesWithIndirections ~= BayesianNetwork.MatrixWithIndirection.make(matrix, conditionalIndices);
	}

	bayesianNetwork.testMe();


}

private static Value[] dupAndReplaceAt(Value[] arr, size_t index, Value newValue) {
	Value[] result = arr.dup;
	result[index] = newValue;
	return result;
}














template ValueMatrix(Type) {
    class ValueMatrix {
    	public final this(uint width, uint height) {
    		protectedWidth = width;
    		protectedHeight = height;
    		
    		data.length = width*height;
    	}
    	
		public final Type opIndexAssign(Type value, size_t row, size_t column) {
			return data[row*protectedWidth + column] = value;
		}

		public final Type opIndex(size_t row, size_t column) {
			return data[row*protectedWidth + column];
		}
    	
        public final @property uint width() {
        	return protectedWidth;
        }
        
        public final @property uint height() {
        	return protectedHeight;
        }
    	
        protected Type[] data;
        
        protected uint protectedWidth, protectedHeight;
    }
}
