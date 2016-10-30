module slimRnn.SlimRnnLevinSearch;

import slimRnn.SlimRnn;
import search.levin.LevinSearch;


// we do search for configurations of some parameters of the SLIM RNN via levin search
// as described in schmidhurs SLIM RNN paper

final class SlimRnnLevinProblem : LevinProblem {
	final this(SlimRnn slimRnn) {
		this.slimRnn = slimRnn;
	}

	private SlimRnn slimRnn;

	final void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
		hasHalted = false;

		// skip over programs with too few instructions 
		if( program.instructions.length < /* for termination index and threshold */2 + SlimRnnLevinProblem.NUMBEROFINSTRUCTIONSFORSINGLEELEMENT*2 ) {
			import std.stdio;
			writeln(program.instructions.length);
			return;
		}

		// check for redudant/invalid combinations
		if( getTerminationIndex(program) >= slimRnn.map.arr.length ) {
			return;
		}
		if( program.instructions[1] >= TRESHOLDQUANTISATION ) {
			return;
		}

		foreach( pieceIndex, iterationPiece ; slimRnn.pieces ) {
			// if the caReadoff index is out of bouns we just return immediatly, because we save a lot of computation time this way and 
			if( iterationPiece.ca.readofIndex >= iterationPiece.getCaWidth ) {
				return;
			}
		}


		// transcribe to SLIM RNN
		slimRnn.terminal.coordinate.x = getTerminationIndex(program);

		foreach( pieceIndex, iterationPiece ; slimRnn.pieces ) {
			// this is just done for the CA, TODO< other types too >
			iterationPiece.ca.rule = getCaRuleOfLevinProgram(program, pieceIndex);
			iterationPiece.ca.readofIndex = getCaReadOffIndex(program, pieceIndex);

			// we read of the positions from the (levin) program
			foreach( inputIndex, ref iterationInput; iterationPiece.inputs ) {
				iterationInput.coordinate.x = getInputIndex(program, pieceIndex, inputIndex);
			}


			// TODO< read of thresholds ? >
		}




		// init start state
		slimRnn.map.arr[0] = 0.0f;
		slimRnn.map.arr[1] = 0.5f;
		slimRnn.map.arr[2] = 0.5f;

		uint maxIterations = maxNumberOfStepsToExecute;
		uint iterations;
		bool wasTerminated;
		slimRnn.loop(maxIterations, /*out*/ iterations, /*out*/ wasTerminated);

		hasHalted = wasTerminated;
	}

	// returns the instruction of the levin program which encodes the Ca rule
	private static uint getCaRuleOfLevinProgram(LevinProgram program, size_t elementIndex) {
		return CARULES[program.instructions[2 + elementIndex*NUMBEROFINSTRUCTIONSFORSINGLEELEMENT+0]];
	}

	private static float getCaThresholdOfLevinProgram(LevinProgram program, size_t elementIndex) {
		return convertFromFromInstructionToThreshold(program.instructions[2 + elementIndex*NUMBEROFINSTRUCTIONSFORSINGLEELEMENT+1]);
	}

	private static uint getCaReadOffIndex(LevinProgram program, size_t elementIndex) {
		return program.instructions[2 + elementIndex*NUMBEROFINSTRUCTIONSFORSINGLEELEMENT+2];
	}

	private static uint getInputIndex(LevinProgram program, size_t elementIndex, size_t inputIndex) {
		assert(inputIndex < MAXNUMBEROFINPUTS);
		return program.instructions[2 + elementIndex*NUMBEROFINSTRUCTIONSFORSINGLEELEMENT+3+inputIndex];
	}

	private static uint getTerminationIndex(LevinProgram program) {
		// index 0 is reserved for the termination index
		return program.instructions[0];
	}

	private static float getTerminationThreshold(LevinProgram program) {
		// index 0 is reserved for the termination threshold
		return convertFromFromInstructionToThreshold(program.instructions[1]);
	}


	private static float convertFromFromInstructionToThreshold(uint value) {
		assert(value < TRESHOLDQUANTISATION);
		return cast(float)value * (1.0f/(cast(float)TRESHOLDQUANTISATION));
	}


	static const size_t MAXNUMBEROFINPUTS = 3;
	static const size_t NUMBEROFINSTRUCTIONSFORSINGLEELEMENT = 3+MAXNUMBEROFINPUTS; // ca rule and threshold and careadoffindex    +  number of input(indices)
}

static const uint[] CARULES = 
[30, 60, 62, 90, 102, 110, 126, 150, 158, ] ~ // the usual extremly useful rules
[2, 4, 6, 9, 14, 18, 20, 22, 24, 25, 26, 28, 37, 41, 45, 47, 57, 61, 86, 89, 107, 121, // followed by the more chaotic ones 
60, // see animation http://mathworld.wolfram.com/Rule60.html 
90, // see animation http://mathworld.wolfram.com/Rule90.html
102, // see animation http://mathworld.wolfram.com/Rule102.html
105, // see animation http://mathworld.wolfram.com/Rule150.html

]; // rules for the celular automata
static const uint TRESHOLDQUANTISATION = 8;

uint max(uint[] values) {
	uint maxValue = values[0];
	foreach(v; values) {
		if( v > maxValue ) {
			maxValue = v;
		}
	}
	return maxValue;
}

void main() {
    LevinSearch levinSearch = new LevinSearch();

    levinSearch.numberOfInstructions = max([CARULES.length/* number of selected CA rules */, TRESHOLDQUANTISATION]);
    levinSearch.c = 0.3;
    levinSearch.maxIterations = 50000;

    uint programLength = /* for termination index and threshold */2 + SlimRnnLevinProblem.NUMBEROFINSTRUCTIONSFORSINGLEELEMENT*2;

    levinSearch.instructionPropabilityMatrix.length = programLength;
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
    	iterationArray.length = levinSearch.numberOfInstructions;
    }

    // set initial propaility matrix
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
    	foreach( i; 0..iterationArray.length ) {
    		iterationArray[i] = 1.0f;
    	}
    }









	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 10;

	SlimRnn slimRnn = new SlimRnn(ctorParameters);

	slimRnn.terminal.coordinate = 3;
	slimRnn.terminal.value = 0.5f;

	slimRnn.pieces ~= Piece();
	slimRnn.pieces[0].type = Piece.EnumType.CA;
	slimRnn.pieces[0].ca.rule = 0;
	slimRnn.pieces[0].inputs = 
	[
		CoordinateWithValue.make([0, 0, 0], 0.1f),
		CoordinateWithValue.make([0, 0, 0], 0.1f),
		CoordinateWithValue.make([0, 0, 0], 0.1f),
	];

	slimRnn.pieces[0].output = CoordinateWithValue.make([3, 0, 0], 0.6f);


	slimRnn.pieces ~= Piece();
	slimRnn.pieces[1].type = Piece.EnumType.CA;
	slimRnn.pieces[1].ca.rule = 0;
	slimRnn.pieces[1].inputs = 
	[
		CoordinateWithValue.make([0, 0, 0], 0.1f),
		CoordinateWithValue.make([0, 0, 0], 0.1f),
		CoordinateWithValue.make([0, 0, 0], 0.1f),
	];

	slimRnn.pieces[1].output = CoordinateWithValue.make([4, 0, 0], 0.6f);





    LevinProblem levinProblem = new SlimRnnLevinProblem(slimRnn);

    uint numberOfIterations = 50000;
    bool done;
    levinSearch.iterate(levinProblem, numberOfIterations, /*out*/ done);
    if( done ) {
        // TODO
    }

    import std.stdio;
    writeln("done=", done);
}
