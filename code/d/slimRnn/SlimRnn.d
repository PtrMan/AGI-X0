

void applyCaRule(uint rule, uint[] inputArray, ref uint[] resultArray) {
	assert(inputArray.length == resultArray.length);

	for( int i = 0; i < inputArray.length; i++ ) {
		uint value4 = inputArray[(i + inputArray.length - 1) % inputArray.length];
		uint value2 = inputArray[i/* % inputArray.length*/];
		uint value1 = inputArray[(i + 1) % inputArray.length];

		import std.stdio;
		writeln("", value4, " ", value2, " ", value1);

		uint value = value4*4 + value2*2 + value1*1;
		resultArray[i] = (rule >> value) & 1;
		writeln("=", resultArray[i] );
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

	final void loop(uint maxIterations, out uint iterations, out bool wasTerminated) {
		foreach( i; 0..maxIterations ) {
			step();
			wasTerminated = terminated();
			if( wasTerminated ) {
				iterations = i;
				return;
			}
		}

		iterations = maxIterations;
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
		if( piece.type == Piece.EnumType.CA ) {
			uint[] inputArray, resultArray;
			inputArray.length = piece.inputs.length; // OPTIMIZATION< preallocate and check for need to resize >
			resultArray.length = piece.inputs.length;
			
			import std.stdio;
			writeln(piece.inputs.length);
			writeln(resultArray.length);

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
			map.arr[iterationPiece.output.coordinate.x] = iterationPiece.nextOutput;
		}
	}
}

void main() {
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 10;

	SlimRnn slimRnn = new SlimRnn(ctorParameters);

	slimRnn.terminal.coordinate = 3;
	slimRnn.terminal.value = 0.5f;

	slimRnn.pieces ~= Piece();
	slimRnn.pieces[0].type = Piece.EnumType.CA;
	slimRnn.pieces[0].ca.rule = 30;
	slimRnn.pieces[0].inputs = 
	[
		CoordinateWithValue.make([0, 0, 0], 0.1f),
		CoordinateWithValue.make([1, 0, 0], 0.1f),
		CoordinateWithValue.make([2, 0, 0], 0.1f),
	];

	slimRnn.pieces[0].output = CoordinateWithValue.make([3, 0, 0], 0.6f);


	// init start state
	slimRnn.map.arr[0] = 0.0f;
	slimRnn.map.arr[1] = 0.5f;
	slimRnn.map.arr[2] = 0.5f;

	uint maxIterations = 1; // just one iteration for testing
	uint iterations;
	bool wasTerminated;
	slimRnn.loop(maxIterations, /*out*/ iterations, /*out*/ wasTerminated);

	import std.stdio;
	writeln("SLIM RNN iterations=", iterations, " wasTerminated=", wasTerminated);





}
