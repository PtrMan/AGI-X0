module slimRnn.SlimRnn;

import std.stdint;

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

struct Coordinate {
	private size_t[1] values;

	final @property size_t x() {
		return values[0];
	}

	final @property size_t x(size_t newValue) {
		values[0] = newValue;
		return newValue;
	}

	static Coordinate make(size_t value) {
		Coordinate result;
		result.values[0] = value;
		return result;
	}
}


alias Coordinate CoordinateType;

/*
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
}*/


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
alias CoordinateWithValue CoordinateWithAttribute;
alias CoordinateWithValue CoordinateWithStrength;

struct Piece {
	enum EnumType {
		CA, // cellular automata
		CLASSICNEURON, // can be a function with multiple inputs and an output function
		XOR, // TODO< encoding >
	}

	bool enabled = true;

	EnumType type;

	uint32_t functionType;


	static struct Ca {
		final @property uint rule() pure {
			return *functionTypePtr;
		}

		uint readofIndex; // from which index in the CA should the result be read

		uint32_t *functionTypePtr;
	}

	static struct ClassicalNeuron {
		enum EnumType : uint32_t {
			MULTIPLICATIVE,
			//ADDITIVE,
		}
		private static const uint ENUMSIZE = 1;

		final @property EnumType type() pure {
			uint32_t value = *functionTypePtr >= ENUMSIZE ? ENUMSIZE-1 : *functionTypePtr;
			return cast(EnumType)value;
		}

		uint32_t *functionTypePtr;
	}

	// can't be an (D)union because it produces wrong values when an algorithm switches the type	
	Ca ca;
	ClassicalNeuron classicalNeuron; // if it is an classical neuron the input attributes are interpreted as factors for the activation


	CoordinateWithAttribute[] inputs; 
	CoordinateWithStrength output;

	float nextOutput;

	// returns the number of cells in the CA
	final size_t getCaWidth() {
		assert(type == EnumType.CA);
		return inputs.length; // number of cells is for now the size/width of the input
	}

	final void initInternalValues() {
		ca.functionTypePtr = &functionType;
		classicalNeuron.functionTypePtr = &functionType;
	}

	final @property string humanReadableDescription() {
		import std.conv : to;
		import std.format : format;
		import std.algorithm.iteration : map;

		string resultString;
		resultString ~= "\ttype=%s\n".format(type.to!string);
		resultString ~= "\tinputs=%s\n".format(inputs.map!(v => "(idx=%s,t=%s)".format(v.coordinate.x, v.threshold)));
		resultString ~= "\toutput=(idx=%s,s=%s)\n".format(output.coordinate.x, output.strength);
		// TODO< other >

		return resultString;
	}
}

// TODO< make this 2d >
struct Map1d {
	float[] arr;


}

struct SlimRnnCtorParameters {
	size_t[1] mapSize;
}

// inspired by paper
// J. Schmidhuber. Self-delimiting neural networks. Technical Report IDSIA-08-12, arXiv:1210.0118v1 [cs.NE], IDSIA, 2012.

class SlimRnn {
	Piece[] pieces;

	Map1d map;

	CoordinateWithThreshold terminal;

	size_t defaultInputSwitchboardIndex = 0; // index of the switchboard to which created inputs are automatically set to

	final this(SlimRnnCtorParameters parameters) {
		map.arr.length = parameters.mapSize[0];
	}

	final SlimRnn clone() {
		SlimRnnCtorParameters parameters;
		parameters.mapSize[0] = map.arr.length;
		SlimRnn result = new SlimRnn(parameters);
		result.pieces = pieces.dup;
		result.initInternalValuesOfPieces(); // rewire pointers
		result.map = map;
		result.terminal = terminal;
		return result;
	}

	// sets the internal pointers of all pieces to the right values
	final void initInternalValuesOfPieces() {
		foreach( ref iterationPiece; pieces ) {
			iterationPiece.initInternalValues();
		}
	}

	final void resetMap() {
		foreach( ref iv; map.arr ) {
			iv = 0.0f;
		}
	}

	final void resetPiecesToCaCount(uint countOfPieces) {
		pieces.length = 0;
		pieces.length = countOfPieces;

		initInternalValuesOfPieces();

		foreach( ref iterationPiece; pieces ) {
			iterationPiece.type = Piece.EnumType.CA;
			iterationPiece.functionType = 0;
			iterationPiece.inputs = 
			[
				CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.1f),
				CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.1f),
				//CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.1f),  uncomemnting this has an effect on how the network will be structured for the XOR example/test
			];

			iterationPiece.output = CoordinateWithValue.make(Coordinate.make(3), 0.6f);
			iterationPiece.enabled = false;
		}
	}

	final void loop(uint maxIterations, out uint iterations, out bool wasTerminated, out bool executionError) {
		executionError = false;

		foreach( i; 0..maxIterations ) {
			debugState(i);

			step(/*out*/executionError);
			if( executionError ) {
				return;
			}

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

		if(!false) {
			return;
		}

		writeln("iteration=",iteration);

		foreach(value;map.arr) {
			write(value, " ");
		}
		writeln();
		writeln("terminal.index=", terminal.coordinate.x);
		writeln("---");
	}

	final string humanReadableDescriptionOfPieces() {
		import std.format;

		string result;
		foreach( i, iterationPiece; pieces ) {
			result ~= "piece #=%s\n".format(i);
			result ~= "\t" ~ iterationPiece.humanReadableDescription;
		}
		return result;
	}

	private final void step(out bool executionError) {
		calcNextStates();
		applyOutputs(/*out*/executionError);
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

			bool outputActivation = resultArray[piece.ca.readofIndex % resultArray.length] != 0;
			piece.nextOutput = (outputActivation ? piece.output.strength : 0.0f);
		}
		else if( piece.type == Piece.EnumType.CLASSICNEURON ) {
			float inputActivation = 1.0f;

			if( piece.classicalNeuron.type == Piece.ClassicalNeuron.EnumType.MULTIPLICATIVE ||
				piece.classicalNeuron.type > Piece.ClassicalNeuron.ENUMSIZE // to handle values out of range, we default to the most useful, which is multiplicative
			) {
				inputActivation = 1.0f;

				foreach( iterationInput; piece.inputs ) {
					float iterationInputActivation = (map.arr[iterationInput.coordinate.x] * iterationInput.value); // we just do a multiplication with an factor for simplicity
					inputActivation *= iterationInputActivation;
				}
			}
			else {
				throw new Exception("Internal Error");
			}

			// TODO< add activation function

			// note< check for equivalence is important, because it allows us to activate a Neuron if the input is zero for neurons which have to be all the time on >
			piece.nextOutput = (inputActivation >= piece.output.value ? piece.output.value : 0.0f);
		}
		else if( piece.type == Piece.EnumType.XOR ) {
			bool inputActivation = false;

			foreach( iterationInput; piece.inputs ) {
				bool iterationInputActivation = map.arr[iterationInput.coordinate.x] >= iterationInput.value;
				inputActivation ^= iterationInputActivation;
			}

			piece.nextOutput = inputActivation ? piece.output.value : 0.0f;
		}
	}

	private final void calcNextStates() {
		foreach( ref iterationPiece; pieces ) {
			calcNextState(iterationPiece);
		}
	}

	private final void applyOutputs(out bool executionError) {
		executionError = true;
		foreach( iterationPiece; pieces ) {
			if( !iterationPiece.enabled ) {
				continue;
			}

			if( iterationPiece.output.coordinate.x >= map.arr.length ) {
				return; // we return with an execution error
			}

			map.arr[iterationPiece.output.coordinate.x] = iterationPiece.nextOutput;
		}
		executionError = false;
	}
}
