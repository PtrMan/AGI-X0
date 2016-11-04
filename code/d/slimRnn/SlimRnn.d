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

// optimization:
// we employ a Copy-on-write (COW) to get rid of most copying overhead
// the SlimRnn maintains an array of Pieces and points on the parent SlimRnn
// if a value is written to a piece another struct decides if an copy operation is necessary
// reads can be shadowed to the copy'ed one or the orginal one from the SlimRnn parent

struct PieceCowFacade {
	private Piece *shadowed; // can be null if the SlimRnn (parent)'s list of pieces is smaller than the pieces of the SlimRnn used for the front
	private Piece *front; // can't be null

	bool opaque = false; // OPTIMIZATION< COW >< is this piece or the piece of the parent accessed when reading data? >

	// we can't directly access the inputs for writing
	final @property void setInputs(CoordinateWithAttribute[] inputs) {
		if( opaque ) {
			front.inputs = inputs;
		}
		else {
			copyShadowedToFront();
			opaque = true;
			setInputs(inputs);
		}
	}

	// for read only access
	final @property CoordinateWithAttribute[] inputs() pure {
		if( opaque ) {
			return front.inputs;
		}
		else {
			return shadowed.inputs;
		}
	}


	private static string ctfeSetAccessorFor(string type, string name) {
		import std.format;

		return
			("final @property %1$s %2$s(%1$s newValue) {"~
			"if( opaque ) {"~
			"front.%2$s = newValue;"~
			"}"~
			"else {"~
			"copyShadowedToFront();"~
			"opaque = true;"~
			"return %2$s(newValue);"~
			"}"~
			"return newValue;"~
			"}").format(type, name);
	}

	private static string ctfeGetAccessorFor(string type, string name, bool enabledGetter) {
		import std.format;

		if(!enabledGetter) {
			return "";
		}

		return
			("final @property %1$s %2$s() {"~
			"if( opaque ) {"~
			"return front.%2$s;"~
			"}"~
			"return shadowed.%2$s;"~
			"}").format(type, name);
	}

	private static string ctfeSetGetAccessors(string type, string name, bool enabledGetter = true) {
		return ctfeSetAccessorFor(type, name) ~ ctfeGetAccessorFor(type, name, enabledGetter);
	}

	mixin(ctfeSetGetAccessors("bool", "enabled"));
	mixin(ctfeSetGetAccessors("Piece.EnumType", "type"));
	mixin(ctfeSetGetAccessors("CoordinateWithStrength", "output"));
	mixin(ctfeSetGetAccessors("CoordinateWithAttribute[]", "inputs", false));

	final @property uint caRule() {
		if( opaque ) {
			return front.caRule;
		}
		return shadowed.caRule;
	}

	final @property uint caReadofIndex() {
		if( opaque ) {
			return front.caReadofIndex;
		}
		return shadowed.caReadofIndex;
	}


	final bool isCa() {
		if( opaque ) {
			return front.isCa;
		}
		return shadowed.isCa;
	}


	// the only way to set an input!
	final void setInputAt(size_t index, CoordinateWithAttribute value) {
		if( opaque ) {
			front.inputs[index] = value;
		}
		else {
			copyShadowedToFront();
			opaque = true;
			setInputAt(index, value);
		}
	}

	// returns the number of cells in the CA
	final size_t getCaWidth() {
		if( opaque ) {
			return front.getCaWidth;
		}
		return shadowed.getCaWidth;
	}

	final @property string humanReadableDescription() {
		if( opaque ) {
			return front.humanReadableDescription;
		}
		return shadowed.humanReadableDescription;
	}
	


	private void copyShadowedToFront() {
		copy(front, shadowed);
	}

	static private void copy(Piece *destination, Piece *source) {
		destination.enabled = source.enabled;
		destination.type = source.type;
		destination.caReadofIndex = source.caReadofIndex;
		
		if( destination.inputs.length != source.inputs.length ) {
			destination.inputs.length = source.inputs.length;
		}

		foreach( i; 0..source.inputs.length) {
			destination.inputs[i].coordinate.x = source.inputs[i].coordinate.x;
			destination.inputs[i].value = source.inputs[i].value;
		}

		destination.output.coordinate.x = source.output.coordinate.x;
		destination.output.value = source.output.value;
	}
}


// do not access directly because it's under COW!
struct Piece {
	enum EnumType : uint32_t {
		XOR, // TODO< encoding >
		CLASSICNEURON_MULTIPLICATIVE, // can be a function with multiple inputs and an output function

		_CASTART, // cellular automata
		          // all values greater or equal are the ca-rules
		
	}

	bool enabled = true;

	EnumType type;

	uint caReadofIndex; // from which index in the CA should the result be read
	
	CoordinateWithAttribute[] inputs; 
	CoordinateWithStrength output;

	// returns the number of cells in the CA
	final size_t getCaWidth() {
		assert(type >= EnumType._CASTART);
		return inputs.length; // number of cells is for now the size/width of the input
	}

	final @property uint32_t caRule() {
		assert(isCa);
		return type - EnumType._CASTART;
	}

	final @property bool isCa() {
		return type >= EnumType._CASTART;
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
	size_t numberOfPieces;
}

// inspired by paper
// J. Schmidhuber. Self-delimiting neural networks. Technical Report IDSIA-08-12, arXiv:1210.0118v1 [cs.NE], IDSIA, 2012.

class SlimRnn {
	//Piece[] pieces;
	private Piece[] opaquePieces;
	private SlimRnn *parent; // for COW, can be null if it is the root

	// length should not be changed
	PieceCowFacade[] pieceAccessors;

	private float[] nextOutputs; // next outputs of pieces
	                             // not under COW

	Map1d map; // not under COW

	CoordinateWithThreshold terminal; // not under COW

	size_t defaultInputSwitchboardIndex = 0; // index of the switchboard to which created inputs are automatically set to
	                                         // not under COW

	static SlimRnn makeRoot(SlimRnnCtorParameters parameters) {
		SlimRnn result = new SlimRnn(parameters);
		result.cowResetAndSetCountOfPiecesFor(parameters.numberOfPieces);
		result.cowSetAllToOpaque();
		return result;
	}

	final private this(SlimRnnCtorParameters parameters) {
		assert(parameters.numberOfPieces > 0);

		map.arr.length = parameters.mapSize[0];
		nextOutputs.length = parameters.numberOfPieces;
	}

	final void resizePieces(uint countOfPieces) {
		cowResetAndSetCountOfPiecesFor(countOfPieces);
		nextOutputs.length = countOfPieces;
	}

	// array of piece indices which are ready to be executed
	// is updated by the user with compile()
	private size_t[] entryReadySet;

	private size_t[] readySet;


	// sets up acceleration datastructures
	final void compile() {
		// some checks
		assert(opaquePieces.length == pieceAccessors.length);
		assert(opaquePieces.length == nextOutputs.length);

		// compile entry ready set

		size_t readyElements = 0;

		foreach( i, ref iterationPieceAccessor; pieceAccessors ) {
			if( iterationPieceAccessor.enabled ) {
				readyElements++;
			}
		}

		entryReadySet.length = readyElements;

		size_t entryReadySetI = 0;

		// for our purposes we just take all activated pieces into it
		// even if we can add "false friends"
		foreach( i, ref iterationPieceAccessor; pieceAccessors ) {
			if( iterationPieceAccessor.enabled ) {
				entryReadySet[entryReadySetI++] = i;
			}
		}
	}



	// clones it and handles this SlimRnn as if it were the root SlimRnn
	final SlimRnn cloneUnderCowAsRoot() {
		assert(parent is null, "assumption is that this SlimRnn is the root, having a parent breaks this assumption");

		SlimRnnCtorParameters parameters;
		parameters.mapSize[0] = map.arr.length;
		parameters.numberOfPieces = pieceAccessors.length;
		SlimRnn result = new SlimRnn(parameters);
		result.parent = &this;

		result.terminal = terminal;
		result.defaultInputSwitchboardIndex = defaultInputSwitchboardIndex;

		result.cowResetAndSetCountOfPiecesFor(pieceAccessors.length);
		result.cowFlushOpaquePieces();

		return result;
	}

	final void resetMap() {
		foreach( ref iv; map.arr ) {
			iv = 0.0f;
		}
	}

	final void resetPiecesToTypeByCount(uint countOfPieces, Piece.EnumType type) {
		cowResetAndSetCountOfPiecesFor(countOfPieces);

		foreach( ref iterationPieceCowFacade; pieceAccessors ) {
			iterationPieceCowFacade.type = type;
			iterationPieceCowFacade.inputs =
			[
				CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.8f),
				CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.8f),
				//CoordinateWithValue.make(Coordinate.make(defaultInputSwitchboardIndex), 0.1f),  uncomemnting this has an effect on how the network will be structured for the XOR example/test
			];

			iterationPieceCowFacade.output = CoordinateWithValue.make(Coordinate.make(2), 0.6f);
			iterationPieceCowFacade.enabled = false;
		}
	}

	final void run(uint maxIterations, out uint iterations, out bool wasTerminated_, out bool executionError) {
		///import std.stdio;
		///writeln(humanReadableDescriptionOfPieces());

		executionError = false;

		readySet = entryReadySet.dup;

		foreach( i; 0..maxIterations ) {
			debugSwitchboardState(i);

			step(/*out*/executionError);
			if( executionError ) {
				return;
			}

			wasTerminated_ = wasTerminated();
			if( wasTerminated_ ) {
				iterations = i;
				return;
			}
		}

		iterations = maxIterations;
	}

	private final void debugSwitchboardState(uint iteration) {
		import std.stdio;

		bool debug_ = false;

		if(!debug_) {
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

	final string humanReadableDescriptionOfPieces(bool filterForEnabled = true) {
		import std.format;

		string result;
		foreach( i, iterationPieceAccessor; pieceAccessors ) {
			if( filterForEnabled && !iterationPieceAccessor.enabled ) {
				continue;
			}

			result ~= "piece #=%s\n".format(i);
			result ~= iterationPieceAccessor.humanReadableDescription;
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

	private final bool wasTerminated() {
		///import std.stdio;
		///writeln("slimRnn wasTerminated = ", map.arr[terminal.coordinate.x], " >= ", terminal.threshold );

		return readAtCoordinateAndCheckForThreshold(terminal);
	}

	private final void calcNextState(PieceCowFacade *piece, size_t pieceIndex) {
		assert( piece.enabled ); // must be the case because we are working with the ready set, and the ready set has to have by definition only enabled elements in it

		if( piece.isCa ) {
			uint[] inputArray, resultArray;
			inputArray.length = piece.inputs.length; // OPTIMIZATION< preallocate and check for need to resize >
			resultArray.length = piece.inputs.length;

			foreach( inputIndex; 0..inputArray.length ) {
				bool activated = readAtCoordinateAndCheckForThreshold(piece.inputs[inputIndex]);
				inputArray[inputIndex] = activated ? 1 : 0;
			}

			assert(piece.caRule <= 255);
			applyCaRule(piece.caRule, inputArray, /*out*/ resultArray);

			bool outputActivation = resultArray[piece.caReadofIndex % resultArray.length] != 0;
			nextOutputs[pieceIndex] = (outputActivation ? /*piece.output.strength*/ 1.0f/* TODO< set this with an SLIM parameter >*/ : 0.0f);
		}
		else if( piece.type == Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE ) {
			float inputActivation = 1.0f;

			if( piece.type == Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE ) {
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
			nextOutputs[pieceIndex] = (inputActivation >= piece.output.threshold ? /*piece.output.value*/ 1.0f/* TODO< set this with an SLIM parameter >*/  : 0.0f);


			// debug
			if( false ) {
				import std.stdio;
				writeln("Piece.EnumType.CLASSICNEURON");
				write("input= ");
				foreach( iterationInput; piece.inputs ) {
					write("[", iterationInput.coordinate.x, "]*", iterationInput.value, "=", (map.arr[iterationInput.coordinate.x] * iterationInput.value), ", ");
				}
				writeln();

				writeln("inputActivation=", inputActivation, " threshold=", piece.output.threshold);
			}
		}
		else if( piece.type == Piece.EnumType.XOR ) {
			

			bool inputActivation = false;

			foreach( iterationInput; piece.inputs ) {
				bool iterationInputActivation = map.arr[iterationInput.coordinate.x] >= iterationInput.value;
				inputActivation ^= iterationInputActivation;
			}

			if( false ) {
				import std.stdio;
				writeln("Piece.EnumType.XOR");
				write("input= ");
				foreach( iterationInput; piece.inputs ) {
					write("[", iterationInput.coordinate.x, "] t=", iterationInput.value, "=", (map.arr[iterationInput.coordinate.x] >= iterationInput.value), ", ");
				}
				writeln();

				writeln("inputActivation=", inputActivation);
			}

			nextOutputs[pieceIndex] = inputActivation ? /*piece.output.value*/1.0f/* TODO< set this with an SLIM parameter >*/  : 0.0f;
		}
	}

	private final void calcNextStates() {
		foreach( pieceIndex, iterationReadySetPieceIndex; readySet ) {
			PieceCowFacade *iterationPieceFacade = &pieceAccessors[iterationReadySetPieceIndex];
			calcNextState(iterationPieceFacade, pieceIndex);
		}
	}

	private final void applyOutputs(out bool executionError) {
		executionError = true;
		foreach( iterationPieceIndex, iterationPieceFacade; pieceAccessors ) {
			if( !iterationPieceFacade.enabled ) {
				continue;
			}

			if( iterationPieceFacade.output.coordinate.x >= map.arr.length ) {
				return; // we return with an execution error
			}

			map.arr[iterationPieceFacade.output.coordinate.x] = nextOutputs[iterationPieceIndex];
		}
		executionError = false;
	}









	/////////////////////
	/// COW area
	/////////////////////

	// (1) set length of opaquePieces and accessors to zero and then to the requested length
	// (2) rewire COW-Facade variables
	private final void cowResetAndSetCountOfPiecesFor(uint countOfPieces) {
		// (1)
		opaquePieces.length = 0; // actually not necessary
		opaquePieces.length = countOfPieces;

		pieceAccessors.length = 0; // actually not necessary
		pieceAccessors.length = countOfPieces;


		// (2)
		foreach( i, ref iterationPieceFacade; pieceAccessors ) {
			iterationPieceFacade.shadowed = null;
			if( parent !is null ) {
				bool isIndexInRangeOfCountOfParentOpaquePieces = i < parent.opaquePieces.length;
				if( isIndexInRangeOfCountOfParentOpaquePieces ) { // we can only point to the parent opaque piece if it exists in the first place
					iterationPieceFacade.shadowed = &parent.opaquePieces[i]; // we just implement one level < TODO< multiple levels of indirection >
				}
			}

			iterationPieceFacade.front = &opaquePieces[i];
		}
	}

	private final void cowFlushOpaquePieces() {
		if( parent is null ) { // we don't flush the opaqueness for the root SLIM-RNN because it doesn't make any sense
			return; 
		}

		foreach( ref iterationPieceFacade; pieceAccessors ) {
			iterationPieceFacade.opaque = false;
		}
	}

	private final void cowSetAllToOpaque() {
		foreach( ref iterationPieceFacade; pieceAccessors ) {
			iterationPieceFacade.opaque = true;
		}
	}
}

unittest { // one neuron  in root
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 5;
	ctorParameters.numberOfPieces = 2;
	SlimRnn root = SlimRnn.makeRoot(ctorParameters);
	assert(root.pieceAccessors.length == 2);

	root.terminal = CoordinateWithThreshold.make(CoordinateType.make(4), 0.1f);


	root.pieceAccessors[0].type = Piece.EnumType.XOR;
	root.pieceAccessors[0].inputs =
	[
		CoordinateWithValue.make(Coordinate.make(0), 0.5f),
		CoordinateWithValue.make(Coordinate.make(1), 0.5f),
	];

	root.pieceAccessors[0].output = CoordinateWithValue.make(Coordinate.make(3), 0.6f);
	root.pieceAccessors[0].enabled = true;

	// termination neuron
	root.pieceAccessors[1].type = Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE;
	root.pieceAccessors[1].inputs =
	[
	];

	root.pieceAccessors[1].output = CoordinateWithValue.make(Coordinate.make(4), 0.6f);
	root.pieceAccessors[1].enabled = true;


	root.compile();

	uint maxIterations = 2;
	uint iterations;
	bool wasTerminated_, executionError;

	root.map.arr[0] = 0.0f;
	root.map.arr[1] = 0.0f;
	root.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(false == (root.map.arr[3] >= 0.5f));

	root.map.arr[0] = 0.0f;
	root.map.arr[1] = 1.0f;
	root.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(true == (root.map.arr[3] >= 0.5f));
}

unittest { // one neuron  overwritten
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 5;
	ctorParameters.numberOfPieces = 2;
	SlimRnn root = SlimRnn.makeRoot(ctorParameters);
	assert(root.pieceAccessors.length == 2);

	root.terminal = CoordinateWithThreshold.make(CoordinateType.make(4), 0.1f);

	root.pieceAccessors[0].type = Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE;
	root.pieceAccessors[0].inputs =
	[
		CoordinateWithValue.make(Coordinate.make(0), 0.5f),
		CoordinateWithValue.make(Coordinate.make(1), 0.5f),
	];

	root.pieceAccessors[0].output = CoordinateWithValue.make(Coordinate.make(3), 0.6f);
	root.pieceAccessors[0].enabled = true;

	// termination neuron
	root.pieceAccessors[1].type = Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE;
	root.pieceAccessors[1].inputs =
	[
	];

	root.pieceAccessors[1].output = CoordinateWithValue.make(Coordinate.make(4), 0.6f);
	root.pieceAccessors[1].enabled = true;



	SlimRnn usedSlimRnn = root.cloneUnderCowAsRoot();

	// overwrite with COW
	usedSlimRnn.pieceAccessors[0].type = Piece.EnumType.XOR;

	usedSlimRnn.compile();

	uint maxIterations = 2;
	uint iterations;
	bool wasTerminated_, executionError;

	usedSlimRnn.map.arr[0] = 0.0f;
	usedSlimRnn.map.arr[1] = 0.0f;
	usedSlimRnn.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(false == (usedSlimRnn.map.arr[3] >= 0.5f));

	usedSlimRnn.map.arr[0] = 0.0f;
	usedSlimRnn.map.arr[1] = 1.0f;
	usedSlimRnn.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(true == (usedSlimRnn.map.arr[3] >= 0.5f));
}
