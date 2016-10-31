module slimRnn.SlimRnn;

void applyCaRule(uint rule, uint[] inputArray, ref uint[] resultArray) {
	assert(inputArray.length == resultArray.length);
	assert(inputArray.length >= 2); // the optimization wasn't done for less elements

	static uint calcResultFirst(uint rule, uint[] inputArray) {
		uint value4 = inputArray[inputArray.length-1];
		uint value2 = inputArray[0/* % inputArray.length*/];
		uint value1 = inputArray[(0 + 1)];

		uint value = value4*4 + value2*2 + value1*1;
		return (rule >> value) & 1;
	}

	static uint calcResultLast(uint rule, uint[] inputArray) {
		uint value4 = inputArray[inputArray.length-2];
		uint value2 = inputArray[inputArray.length-1];
		uint value1 = inputArray[0];

		uint value = value4*4 + value2*2 + value1*1;
		return (rule >> value) & 1;
	}

	resultArray[0] = calcResultFirst(rule, inputArray);
	resultArray[$-1] = calcResultLast(rule, inputArray);
	for( int i = 1; i < inputArray.length - 1; i++ ) {
		uint value4 = inputArray[i - 1];
		uint value2 = inputArray[i    ];
		uint value1 = inputArray[i + 1];

		uint value = value4*4 + value2*2 + value1*1;
		resultArray[i] = (rule >> value) & 1;
	}
}

alias uint[3] CoordinateType;

@property uint x(CoordinateType coordinate) {
	return coordinate[0];
}

@property uint x(ref CoordinateType coordinate, uint newValue) {
	return coordinate[0] = newValue;
}

@property uint y(CoordinateType coordinate) {
	return coordinate[1];
}

@property uint y(ref CoordinateType coordinate, uint newValue) {
	return coordinate[1] = newValue;
}

@property uint layer(CoordinateType coordinate) {
	return coordinate[2];
}

@property uint layer(ref CoordinateType coordinate, uint newValue) {
	return coordinate[2] = newValue;
}


// implementation of an basic 

struct CoordinateWithValue {
	CoordinateType coordinate;
	float value;

	final @property float threshold() {
		return value;
	}

	final @property float strength() {
		return value;
	}

	static CoordinateWithValue make(CoordinateType coordinate, float value) {
		CoordinateWithValue result;
		result.coordinate = coordinate;
		result.value = value;
		return result;
	}
}

alias CoordinateWithValue CoordinateWithThreshold;
alias CoordinateWithValue CoordinateWithStrength;

struct Piece {
	enum EnumType {
		CA, // cellular automata
	}

	bool enabled = true;

	EnumType type;

	union {
		static struct Ca {
			uint rule;
			uint readofIndex; // from which index in the CA should the result be read
		}

		Ca ca;
	}

	CoordinateWithThreshold[] inputs; 
	CoordinateWithStrength output;

	float nextOutput;

	// returns the number of cells in the CA
	final uint getCaWidth() {
		assert(type == EnumType.CA);
		return inputs.length; // number of cells is for now the size/width of the input
	}
}

// TODO< make this 2d >
struct Map1d {
	float[] arr;


}

struct SlimRnnCtorParameters {
	uint[1] mapSize;
}

// inspired by paper
// J. Schmidhuber. Self-delimiting neural networks. Technical Report IDSIA-08-12, arXiv:1210.0118v1 [cs.NE], IDSIA, 2012.

class SlimRnn {
	Piece[] pieces;

	Map1d map;

	CoordinateWithThreshold terminal;

	final this(SlimRnnCtorParameters parameters) {
		map.arr.length = parameters.mapSize[0];
	}

	final SlimRnn clone() {
		SlimRnnCtorParameters parameters;
		parameters.mapSize[0] = map.arr.length;
		SlimRnn result = new SlimRnn(parameters);
		result.pieces = pieces.dup;
		result.map = map;
		result.terminal = terminal;
		return result;
	}

	final void loop(uint maxIterations, out uint iterations, out bool wasTerminated) {
		foreach( i; 0..maxIterations ) {
			debugState(i);

			step();
			wasTerminated = terminated();
			if( wasTerminated ) {
				iterations = i;
				return;
			}
		}

		iterations = maxIterations;
	}

	private final void debugState(uint iteration) {
		import std.stdio;

		writeln("iteration=",iteration);

		foreach(value;map.arr) {
			write(value, " ");
		}
		writeln();
		writeln("terminal.index=", terminal.coordinate.x);
		writeln("---");
	}

	private final void step() {
		calcNextStates();
		applyOutputs();
	}

	private final bool readAtCoordinateAndCheckForThreshold(ref CoordinateWithValue coordinateWithValue) {
		return map.arr[coordinateWithValue.coordinate.x] >= coordinateWithValue.threshold;
	}

	private final bool terminated() {
		return map.arr[terminal.coordinate.x] >= terminal.threshold;
	}

	private final void calcNextState(ref Piece piece) {
		if( !piece.enabled ) {
			return;
		}

		if( piece.type == Piece.EnumType.CA ) {
			uint[] inputArray, resultArray;
			inputArray.length = piece.inputs.length; // OPTIMIZATION< preallocate and check for need to resize >
			resultArray.length = piece.inputs.length;

			foreach( inputIndex; 0..inputArray.length ) {
				bool activated = readAtCoordinateAndCheckForThreshold(piece.inputs[inputIndex]);
				inputArray[inputIndex] = activated ? 1 : 0;
			}

			applyCaRule(piece.ca.rule, inputArray, /*out*/ resultArray);

			bool outputActivation = resultArray[piece.ca.readofIndex] != 0;
			piece.nextOutput = (outputActivation ? piece.output.strength : 0.0f);
		}
	}

	private final void calcNextStates() {
		foreach( ref iterationPiece; pieces ) {
			calcNextState(iterationPiece);
		}
	}

	private final void applyOutputs() {
		foreach( iterationPiece; pieces ) {
			if( !iterationPiece.enabled ) {
				continue;
			}

			map.arr[iterationPiece.output.coordinate.x] = iterationPiece.nextOutput;
		}
	}
}
