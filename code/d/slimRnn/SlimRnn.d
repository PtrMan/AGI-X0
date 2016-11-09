module slimRnn.SlimRnn;

// TODO< finish WTA >

import std.stdint;

bool applyCaRule(uint rule, bool[3] values) {
	uint value = values[2]*4 + values[1]*2 + values[0]*1;
	return ((rule >> value) & 1) != 0;
}

unittest {
	assert(!applyCaRule(254, [false, false, false]));
	assert(applyCaRule(254, [false, false, true]));
	assert(applyCaRule(254, [false, true, false]));
	assert(applyCaRule(254, [false, true, true]));
	assert(applyCaRule(254, [true, false, false]));
	assert(applyCaRule(254, [true, false, true]));
	assert(applyCaRule(254, [true, true, false]));
	assert(applyCaRule(254, [true, true, true]));
}

/+
void applyCaRuleOnUintArray(uint rule, uint[] inputArray, uint *resultArray) {
	assert(inputArray.length >= 2); // the optimization wasn't done for less elements

	static uint calcResultFirst(uint rule, uint[] inputArray) {
		bool[3] values;
		values[2] = inputArray[inputArray.length-1] != 0;
		values[1] = inputArray[0/* % inputArray.length*/] != 0;
		values[0] = inputArray[(0 + 1)] != 0;
		return applyCaRule(rule, values);
	}

	static uint calcResultLast(uint rule, uint[] inputArray) {
		bool[3] values;
		values[2] = inputArray[inputArray.length-2] != 0;
		values[1] = inputArray[inputArray.length-1] != 0;
		values[0] = inputArray[0] != 0;
		return applyCaRule(rule, values);
	}

	resultArray[0] = calcResultFirst(rule, inputArray);
	resultArray[inputArray.length-1] = calcResultLast(rule, inputArray);
	for( int i = 1; i < inputArray.length - 1; i++ ) {
		bool[3] values;
		values[2] = inputArray[i - 1] != 0;
		values[1] = inputArray[i    ] != 0;
		values[0] = inputArray[i + 1] != 0;
		resultArray[i] = applyCaRule(rule, values);
	}
}+/

// TODO< unittest >
bool applyCaRuleOnBoolArraySingle(uint rule, bool[] inputArray, size_t readoffIndex) {
	assert(inputArray.length >= 2); // the optimization wasn't done for less elements

	if( inputArray.length == 2 ) {
		assert(readoffIndex < 2);

		if( readoffIndex == 0 ) {
			return applyCaRule(rule, [inputArray[1], inputArray[0], inputArray[1]]);
		}
		else {
			return applyCaRule(rule, [inputArray[0], inputArray[1], inputArray[0]]);
		}
	}
	else {
		if( readoffIndex == 0 ) {
			return applyCaRule(rule, 
				[
					inputArray[(0 + 1)],
					inputArray[0],
					inputArray[inputArray.length-1],
				]
			);
		}
		else if( readoffIndex == inputArray.length-1 ) {
			return applyCaRule(rule, 
				[
					inputArray[0],
					inputArray[inputArray.length-1],
					inputArray[inputArray.length-2],
				]
			);
		}
		else {
			bool[3] staticInputArray;
			staticInputArray[0] = inputArray[readoffIndex-1];
			staticInputArray[1] = inputArray[readoffIndex];
			staticInputArray[2] = inputArray[readoffIndex+1];
			return applyCaRule(rule, staticInputArray);
		}
	}
}

// TODO< unittest with rule 110 >




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

	final uint64_t calcHash() {
		if( opaque ) {
			return front.calcHash();
		}
		else {
			return shadowed.calcHash();
		}
	}

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
	mixin(ctfeSetGetAccessors("int", "wtaGroup")); 


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

	// the only way to resize inputs
	final void setInputLength(size_t length) {
		if( opaque ) {
			front.inputs.length = length;
		}
		else {
			copyShadowedToFront();
			opaque = true;
			setInputLength(length);
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

	// should only be used for copying the piece!
	final Piece *getPiece() {
		if( opaque ) {
			return front;
		}
		return shadowed;
	}
	


	private void copyShadowedToFront() {
		copy(front, shadowed);
	}

	static private void copy(Piece *destination, Piece *source) {
		assert(destination !is null);
		assert(source !is null);

		destination.enabled = source.enabled;
		destination.type = source.type;
		destination.caReadofIndex = source.caReadofIndex;
		destination.wtaGroup = source.wtaGroup;

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

uint64_t calcHash(Piece *neuron, uint64_t inputHash = 0) {
	void addHash(uint64_t value) {
		inputHash = (inputHash << 13) | (inputHash >> (64-13));
		inputHash ^= value;
	}

	addHash(neuron.enabled);
	addHash(neuron.type);
	addHash(neuron.caReadofIndex);
	addHash(neuron.caReadofIndex);
	addHash(cast(uint32_t)neuron.wtaGroup);
	addHash(neuron.inputs.length);
	foreach( ref iterationInput; neuron.inputs ) {
		addHash(iterationInput.coordinate.x);
		addHash(*(cast(uint32_t*)&iterationInput.value));
	}
	addHash(neuron.output.coordinate.x);
	addHash(*(cast(uint32_t*)&neuron.output.value));
	return inputHash;
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

	int wtaGroup = -1; // index of the winner-takes-all group
	
	CoordinateWithAttribute[] inputs; 
	CoordinateWithStrength output;

	final Piece deepCopy() {
		Piece result;
		result.enabled = enabled;
		result.type = type;
		result.caReadofIndex = caReadofIndex;
		result.wtaGroup = wtaGroup;
		
		if( result.inputs.length != inputs.length ) {
			result.inputs.length = inputs.length;
		}

		foreach( i; 0..inputs.length) {
			result.inputs[i].coordinate.x = inputs[i].coordinate.x;
			result.inputs[i].value = inputs[i].value;
		}

		result.output.coordinate.x = output.coordinate.x;
		result.output.value = output.value;

		return result;
	}

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
		if( isCa ) {
			resultString ~= "\tcaReadofIndex=%s\n".format(caReadofIndex);
		}
		resultString ~= "\twtaGroup=%s\n".format(wtaGroup);

		// TODO< other >

		return resultString;
	}
}

struct Map1d {
	float[] arr;
}

struct SlimRnnCtorParameters {
	size_t[1] mapSize;
	size_t numberOfPieces;
	size_t numberOfWtaGroups;
	size_t numberOfOutputs; // how many outputs has the network which feed back signals into the environment
}

import std.exception : enforce;

uint64_t calcHash(SlimRnn slimRnn) {
	uint64_t hash;

	void addHash(uint64_t value) {
		hash = (hash << 13) | (hash >> (64-13));
		hash ^= value;
	}

	addHash(slimRnn.pieceAccessors.length);
	foreach( iterationNeuronCowFacade; slimRnn.pieceAccessors ) {
		addHash(iterationNeuronCowFacade.calcHash());
	}

	addHash(slimRnn.defaultInputSwitchboardIndex);

	addHash(slimRnn.terminal.coordinate.x);
	addHash(*(cast(uint32_t*)&slimRnn.terminal.value));

	assert(false, "TODO - read output from neuron index");

	return hash;
}

// inspired by paper
// J. Schmidhuber. Self-delimiting neural networks. Technical Report IDSIA-08-12, arXiv:1210.0118v1 [cs.NE], IDSIA, 2012.

class SlimRnn {
	//Piece[] pieces;
	private Piece[] opaquePieces;
	private SlimRnn *parent; // for COW, can be null if it is the root

	// length should not be changed
	PieceCowFacade[] pieceAccessors;

	private float[] nextNeuronOutputs; // next outputs of pieces
	                                   // not under COW

	private static struct WtaWinnerIndexAndValue {
		int winnerNeuronIndex = -1; // index of the current winner
		float winnerOutputValue = 0; // output value of the current winner
	}

	private WtaWinnerIndexAndValue[] wta; // wta groups, has the length of the wta groups
	                                      // not under COW

	float[] outputValues; // output array in which the values get written for the neurons which do have an output index set
	                      // not under COW

	private int[] readOutputFromNeuronIndices; // values can be -1 if the are not connected to an neuron
	                                            // under COW, if it gets changed it will be copied and set to the copied array, else it is null

	// precalculated ca-readoff-index % inputs.length for each piece
	private size_t[] compiledPieceCaReadoffIndices;

	// precalculated flag if an piece/neuron is an CA
	private bool[] compiledIsCa;

	Map1d map; // not under COW

	CoordinateWithThreshold terminal; // not under COW

	size_t defaultInputSwitchboardIndex = 0; // index of the switchboard to which created inputs are automatically set to
	                                         // not under COW

	// creates a identical Slim-RNN out of this rnn
	final SlimRnn flatten() {
		assert(outputValues.length == cowGetReadOutoutFromNeuronIndices().length);

		SlimRnnCtorParameters parameters;
		parameters.mapSize[0] = map.arr.length;
		parameters.numberOfPieces = pieceAccessors.length;
		parameters.numberOfOutputs = outputValues.length;
		SlimRnn result = new SlimRnn(parameters);

		result.terminal = terminal;
		result.defaultInputSwitchboardIndex = defaultInputSwitchboardIndex;

		result.readOutputFromNeuronIndices = readOutputFromNeuronIndices.dup; // dup is important!

		result.cowResetAndSetCountOfPiecesFor(parameters.numberOfPieces);
		result.wta.length = parameters.numberOfWtaGroups;
		result.wtaReset();
		result.cowSetAllToOpaque();

		// deep copy pieces
		foreach( i, ref iterationOpaquePiece; result.opaquePieces ) {
			iterationOpaquePiece = (*(pieceAccessors[i].getPiece)).deepCopy();
		}

		return result;		
	}

	static SlimRnn makeRoot(SlimRnnCtorParameters parameters) {
		SlimRnn result = new SlimRnn(parameters);
		result.cowResetAndSetCountOfPiecesFor(parameters.numberOfPieces);
		result.wta.length = parameters.numberOfWtaGroups;
		result.wtaReset();
		result.cowSetAllToOpaque();

		result.readOutputFromNeuronIndices = new int[parameters.numberOfOutputs];
		foreach( ref o; result.readOutputFromNeuronIndices ) {
			o = -1; // set the index so it doesn't read anything
		}

		return result;
	}

	final @property size_t numberOfWtaGroups() {
		return wta.length;
	}

	final private this(SlimRnnCtorParameters parameters) {
		assert(parameters.numberOfPieces > 0);
		enforce(parameters.numberOfOutputs > 0); // must have an output

		map.arr.length = parameters.mapSize[0];
		nextNeuronOutputs.length = parameters.numberOfPieces;

		outputValues.length = parameters.numberOfOutputs;

		readOutputFromNeuronIndices = null;
	}

	final void resizePieces(uint countOfPieces) {
		cowResetAndSetCountOfPiecesFor(countOfPieces);
		nextNeuronOutputs.length = countOfPieces;
	}

	// method to set output index, which does COW if required
	final void setOutputIndex(size_t neuronIndex, int outputIndex, out bool success) {
		success = false;

		if( neuronIndex >= pieceAccessors.length ) {
			return;
		}

		if( outputIndex >= outputValues.length || outputIndex < -1 ) {
			return;
		}

		cowOutputDoCowForWriteIfRequired();
		readOutputFromNeuronIndices[outputIndex] = neuronIndex;

		success = true;
	}



	// array of piece indices which are ready to be executed
	// is updated by the user with compile()
	private size_t[] entryReadySet;

	private size_t[] readySet;


	// sets up acceleration datastructures
	final void compile(out bool valid) {
		// some checks
		assert(opaquePieces.length == pieceAccessors.length);
		assert(opaquePieces.length == nextNeuronOutputs.length);

		valid = true;

		if( !compileCheckOuputs() ) {
			valid = false;
			return;
		}

		if( !compileCheckInputs() ) {
			valid = false;
			return;
		}

		compileEntryReadySet();
		compileCa(valid);
	}

	private final bool compileCheckOuputs() {
		if( outputValues.length == 0 ) { // networks without an output are not valid
			return false;
		}

		int[] usedReadOutputFromNeuronIndices = cowGetReadOutoutFromNeuronIndices();

		assert( usedReadOutputFromNeuronIndices !is null ); // must be set, the root has to allocate this on setup
		assert( usedReadOutputFromNeuronIndices.length == outputValues.length );

		foreach( int iterationReadoffIndex; usedReadOutputFromNeuronIndices ) {
			if( iterationReadoffIndex == -1 ) { // if the readoff index is not set then we continue
				continue;
			}

			if( iterationReadoffIndex >= pieceAccessors.length ) { // check for index out of bounds
				return false;
			}
		}

		return true;
	}

	// checks the inputs to see if the indices are in range
	private final bool compileCheckInputs() {
		foreach( ref iterationPieceAccessor; pieceAccessors ) {
			foreach( iterationInput; iterationPieceAccessor.inputs ) {
				if( iterationInput.coordinate.x >= map.arr.length ) {
					return false;
				}
			}
		}

		return true;
	}

	final private void compileEntryReadySet() {
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

	// compiles all CA related variables to faster preprocessed variables
	final private void compileCa(ref bool valid) {
		compiledPieceCaReadoffIndices.length = pieceAccessors.length;
		compiledIsCa.length = pieceAccessors.length;
		foreach( i, ref iterationPieceAccessor; pieceAccessors ) {
			compiledIsCa[i] = iterationPieceAccessor.isCa;

			if( iterationPieceAccessor.isCa ) {
				if( iterationPieceAccessor.inputs.length == 0 ) {
					valid = false;
				}

				compiledPieceCaReadoffIndices[i] = iterationPieceAccessor.caReadofIndex % iterationPieceAccessor.inputs.length;
			}
		}
	}

	// resets all opaque pieces
	final void flushOpaquePieces() {
		cowFlushOpaquePieces();
	}

	// clones it and handles this SlimRnn as if it were the root SlimRnn
	final SlimRnn cloneUnderCowAsRoot() {
		assert(parent is null, "assumption is that this SlimRnn is the root, having a parent breaks this assumption");
		assert(outputValues.length == cowGetReadOutoutFromNeuronIndices().length);

		SlimRnnCtorParameters parameters;
		parameters.mapSize[0] = map.arr.length;
		parameters.numberOfPieces = pieceAccessors.length;
		parameters.numberOfOutputs = outputValues.length;
		SlimRnn result = new SlimRnn(parameters);
		result.parent = &this;

		result.terminal = terminal;
		result.defaultInputSwitchboardIndex = defaultInputSwitchboardIndex;

		result.readOutputFromNeuronIndices = readOutputFromNeuronIndices;

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
			];

			iterationPieceCowFacade.output = CoordinateWithValue.make(Coordinate.make(2), 0.6f);
			iterationPieceCowFacade.enabled = false;
		}
	}

	private size_t currentWalkerNeuronIndex = 0;

	// finds the next neuron which is not enabled
	final size_t findNextNeuronIndex(out bool success) {
		success = false;

		for(;;) {
			if( currentWalkerNeuronIndex >= pieceAccessors.length ) {
				return 0;
			}
		
			if( !pieceAccessors[currentWalkerNeuronIndex].enabled ) {
				success = true;
				return currentWalkerNeuronIndex;
			}

			currentWalkerNeuronIndex++;
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
		wtaReset();
		calcNextStates();
		applyOutputs(/*out*/executionError);

		// TODO< figure which neurons take WTA >

		transferToOutputs(); // propagate the result to the output pins
	}

	private final void transferToOutputs() {
		int[] usedReadOutputFromNeuronIndices = cowGetReadOutoutFromNeuronIndices();
		foreach( i, int readOutputFromNeuronIndex; usedReadOutputFromNeuronIndices ) {
			if( readOutputFromNeuronIndex == -1 ) {
				continue;
			}

			outputValues[i] = nextNeuronOutputs[readOutputFromNeuronIndex];
		}
	}

	private final bool readAtCoordinateAndCheckForThreshold(ref CoordinateWithValue coordinateWithValue) {
		return map.arr[coordinateWithValue.coordinate.x] >= coordinateWithValue.threshold;
	}

	private final bool wasTerminated() {
		///import std.stdio;
		///writeln("slimRnn wasTerminated = ", map.arr[terminal.coordinate.x], " >= ", terminal.threshold );

		return readAtCoordinateAndCheckForThreshold(terminal);
	}

	private final void calcNextStates() {
		foreach( iterationReadySetPieceIndex; readySet ) {
			PieceCowFacade *iterationPieceFacade = &pieceAccessors[iterationReadySetPieceIndex];
			calcNextState(iterationPieceFacade, iterationReadySetPieceIndex);
		}
	}

	private final void calcNextState(PieceCowFacade *piece, size_t neuronIndex) {
		assert( piece.enabled ); // must be the case because we are working with the ready set, and the ready set has to have by definition only enabled elements in it

		if( compiledIsCa[neuronIndex] ) {
			assert(piece.isCa);

			static const size_t CASTATICSIZE = 16;
			enforce(piece.inputs.length <= CASTATICSIZE); // we don't have currently a logic for dynamic resizing of an dynamic array implemented

			bool[CASTATICSIZE] staticInputArray;

			foreach( inputIndex; 0..piece.inputs.length ) {
				staticInputArray[inputIndex] = readAtCoordinateAndCheckForThreshold(piece.inputs[inputIndex]);
			}

			assert(piece.caRule <= 255);
			size_t compiledPieceCaReadoffIndex = compiledPieceCaReadoffIndices[neuronIndex];
			bool outputActivation = applyCaRuleOnBoolArraySingle(piece.caRule, staticInputArray[0..piece.inputs.length], compiledPieceCaReadoffIndex);
			nextNeuronOutputs[neuronIndex] = (outputActivation ? /*piece.output.strength*/ 1.0f/* TODO< set this with an SLIM parameter >*/ : 0.0f);
			wtaUpdateForNeuronActivation(piece.wtaGroup, neuronIndex, nextNeuronOutputs[neuronIndex]);
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

			// TODO< add activation function >

			// note< check for equivalence is important, because it allows us to activate a Neuron if the input is zero for neurons which have to be all the time on >
			nextNeuronOutputs[neuronIndex] = (inputActivation >= piece.output.threshold ? /*piece.output.value*/ 1.0f/* TODO< set this with an SLIM parameter >*/  : 0.0f);
			wtaUpdateForNeuronActivation(piece.wtaGroup, neuronIndex, nextNeuronOutputs[neuronIndex]);

			if( false ) {
				import std.stdio;
				writeln("nextNeuronOutputs[", neuronIndex, "]=", nextNeuronOutputs[neuronIndex]);
			}

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

			nextNeuronOutputs[neuronIndex] = inputActivation ? /*piece.output.value*/1.0f/* TODO< set this with an SLIM parameter >*/  : 0.0f;
			wtaUpdateForNeuronActivation(piece.wtaGroup, neuronIndex, nextNeuronOutputs[neuronIndex]);

			if( false ) {
				import std.stdio;
				writeln("nextNeuronOutputs[neuronIndex]=", nextNeuronOutputs[neuronIndex]);
			}
		}
	}

	private final void applyOutputs(out bool executionError) {
		executionError = false;

		foreach( iterationReadySetPieceIndex; readySet ) {
			PieceCowFacade *iterationPieceFacade = &pieceAccessors[iterationReadySetPieceIndex];
			applyOutput(iterationPieceFacade, iterationReadySetPieceIndex, executionError);
		}
	}

	private final void applyOutput(PieceCowFacade *piece, size_t pieceIndex, ref bool executionError) {
		assert(piece.enabled); // only enabled pieces/neurons can be in the ready set

		if( piece.output.coordinate.x >= map.arr.length ) {
			executionError = true;
			return; // we return with an execution error
		}

		///import std.stdio;
		///writeln("applyOutputs() write map.arr[", piece.output.coordinate.x, "] = ", nextNeuronOutputs[iterationPieceIndex]);

		map.arr[piece.output.coordinate.x] = nextNeuronOutputs[pieceIndex];
	}




	/////////////////////
	// WTA
	/////////////////////

	private final void wtaReset() {
		foreach( ref WtaWinnerIndexAndValue iterationWta; wta ) {
			iterationWta.winnerOutputValue = 0;
			iterationWta.winnerOutputValue = -1;
		}
	}

	private final void wtaUpdateForNeuronActivation(int wtaGroup, size_t neuronIndex, float neuronOutputValue) {
		if( wtaGroup == -1 ) { // special value which means that the neuron has no WTA group
			return;
		}

		if( wta[wtaGroup].winnerOutputValue < neuronOutputValue ) {
			wta[wtaGroup].winnerOutputValue = neuronOutputValue;
			wta[wtaGroup].winnerNeuronIndex = cast(int)neuronIndex;
		}
	}

	// checks if an neuron is the winner
	private final bool wtaIsNeuronWinner(int wtaGroup, size_t neuronIndex, float neuronOutputValue) {
		if(wtaGroup == -1) {
			return true;
		}
		return
			(wta[wtaGroup].winnerNeuronIndex == neuronIndex) ||     // normal case
			(wta[wtaGroup].winnerOutputValue == neuronOutputValue); // case for multiple neurons with the same result value, then we don't have a winner and just fire all of them
	}



	/////////////////////
	/// COW area
	/////////////////////

	// (1) set length of opaquePieces and accessors to zero and then to the requested length
	// (2) rewire COW-Facade variables
	private final void cowResetAndSetCountOfPiecesFor(size_t countOfPieces) {
		// (1)
		opaquePieces.length = 0; // actually not necessary
		opaquePieces.length = countOfPieces;

		pieceAccessors.length = 0; // actually not necessary
		pieceAccessors.length = countOfPieces;


		// (2)
		foreach( i, ref iterationPieceFacade; pieceAccessors ) {
			iterationPieceFacade.shadowed = null;
			if( parent is null ) {
				iterationPieceFacade.opaque = true;
			}
			else {
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

	// returns the array for the neuron indices from which the RNN output values get fetched
	private final int[] cowGetReadOutoutFromNeuronIndices() {
		int[] resultReadOutputFromNeuronIndices;

		// TODO< do this for a parent depth of > 1 >
		resultReadOutputFromNeuronIndices = readOutputFromNeuronIndices;
		if( resultReadOutputFromNeuronIndices is null ) {
			resultReadOutputFromNeuronIndices = parent.readOutputFromNeuronIndices;

			// cases can lead to a failiure of this check
			// (a) if the parent depth is > 1
			// (b) if the root has not set this to a non-null pointer, this is invalid and it means that the construction mechanism of the root SlimRnn is broken
			assert(resultReadOutputFromNeuronIndices !is null);
		}

		// cases can lead to a failiure of this check
		// (a) if the parent depth is > 1
		// (b) if the root has not set this to a non-null pointer, this is invalid and it means that the construction mechanism of the root SlimRnn is broken
		assert(resultReadOutputFromNeuronIndices !is null);

		return resultReadOutputFromNeuronIndices;
	}

	private final void cowOutputDoCowForWriteIfRequired() {
		// if we are at root, then the readOutputFromNeuronIndices array must be set
		if( parent is null ) { // if it is the root, then readOutputFromNeuronIndices must not be null
			assert(readOutputFromNeuronIndices !is null);
			return;
		}

		if( readOutputFromNeuronIndices !is null ) { // check if we have nothing to do
			return;
		}

		// if we are here we have to deep copy
		int[] copyFrom = cowGetReadOutoutFromNeuronIndices();

		assert(outputValues.length == pieceAccessors.length);
		readOutputFromNeuronIndices = new int[pieceAccessors.length];
		readOutputFromNeuronIndices = copyFrom.dup;
	}
}

unittest { // one neuron  in root
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 5;
	ctorParameters.numberOfPieces = 2;
	ctorParameters.numberOfOutputs = 1;
	SlimRnn root = SlimRnn.makeRoot(ctorParameters);
	assert(root.pieceAccessors.length == 2);

	root.terminal = CoordinateWithThreshold.make(CoordinateType.make(4), 0.1f);

	bool calleeSuccess;
	root.setOutputIndex(0, 0, /*out*/ calleeSuccess);
	assert(calleeSuccess);


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


	bool isValidNetwork;
	root.compile(/*out*/ isValidNetwork);
	assert(isValidNetwork);

	uint maxIterations = 2;
	uint iterations;
	bool wasTerminated_, executionError;

	root.map.arr[0] = 0.0f;
	root.map.arr[1] = 0.0f;
	root.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(false == (root.outputValues[0] >= 0.5f));

	root.map.arr[0] = 0.0f;
	root.map.arr[1] = 1.0f;
	root.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(true == (root.outputValues[0] >= 0.5f));
}

unittest { // one neuron  overwritten
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 5;
	ctorParameters.numberOfPieces = 2;
	ctorParameters.numberOfOutputs = 1;
	SlimRnn root = SlimRnn.makeRoot(ctorParameters);
	assert(root.pieceAccessors.length == 2);

	root.terminal = CoordinateWithThreshold.make(CoordinateType.make(4), 0.1f);

	bool calleeSuccess;
	root.setOutputIndex(0, 0, /*out*/ calleeSuccess);
	assert(calleeSuccess);

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

	bool isValidNetwork;
	usedSlimRnn.compile(/*out*/ isValidNetwork);
	assert(isValidNetwork);

	uint maxIterations = 2;
	uint iterations;
	bool wasTerminated_, executionError;

	usedSlimRnn.map.arr[0] = 0.0f;
	usedSlimRnn.map.arr[1] = 0.0f;
	usedSlimRnn.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(false == (usedSlimRnn.outputValues[0] >= 0.5f));

	usedSlimRnn.map.arr[0] = 0.0f;
	usedSlimRnn.map.arr[1] = 1.0f;
	usedSlimRnn.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(true == (usedSlimRnn.outputValues[0] >= 0.5f));
}

unittest { // unittest for flatten
	SlimRnn flattened;

	{
		SlimRnnCtorParameters ctorParameters;
		ctorParameters.mapSize[0] = 5;
		ctorParameters.numberOfPieces = 2;
		ctorParameters.numberOfOutputs = 1;
		SlimRnn root = SlimRnn.makeRoot(ctorParameters);
		assert(root.pieceAccessors.length == 2);

		root.terminal = CoordinateWithThreshold.make(CoordinateType.make(4), 0.1f);

		bool calleeSuccess;
		root.setOutputIndex(0, 0, /*out*/ calleeSuccess);
		assert(calleeSuccess);

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

		flattened = usedSlimRnn.flatten();

	}

	assert(flattened.pieceAccessors[0].type == Piece.EnumType.XOR);
	assert(flattened.pieceAccessors[1].type == Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE);


	bool isValidNetwork;
	flattened.compile(/*out*/ isValidNetwork);
	assert(isValidNetwork);

	uint maxIterations = 2;
	uint iterations;
	bool wasTerminated_, executionError;

	flattened.map.arr[0] = 0.0f;
	flattened.map.arr[1] = 0.0f;
	flattened.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(false == (flattened.outputValues[0] >= 0.5f));

	flattened.map.arr[0] = 0.0f;
	flattened.map.arr[1] = 1.0f;
	flattened.run(maxIterations, /*out*/ iterations, /*out*/ wasTerminated_, /*out*/ executionError);
	assert(!executionError);
	assert(wasTerminated_);

	assert(true == (flattened.outputValues[0] >= 0.5f));
}



// TODO< unittest WTA >