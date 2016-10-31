module slimRnn.SlimRnnLevinSearch;

import slimRnn.SlimRnn;
import search.levin.LevinSearch;

struct InstructionInterpretationContext {
	uint pieceIndex;
	uint inputIndex;
}

struct InstructionEncoding {
	// 8 bits

	// 0 - set new inputIndex (2) and (conditionally, if 1111 then it's disabled) set CA rule index (5), dependent on [pieceIndex]

	// 10 - set piece index (6)
	///// 1100 - set CA rule index (4), dependent on [pieceIndex] and [inputIndex]
	// 11010 - set CA readoff index (3), dependent on [pieceIndex]
	// 1101100x - set enabled of piece to 'x' , dependent on [pieceIndex]
	static void executeInstructionOnSlimRnn(ref InstructionInterpretationContext context, SlimRnn slimRnn, uint instruction, out bool unknownEncoding) {
		unknownEncoding = true;

		uint encoding[2];

		encoding[0] = instruction & 1;
		if( encoding[0] == 0 ) {
			unknownEncoding = false;

			context.inputIndex = (instruction >> 1) & 3;
			uint caRuleIndex = (instruction >> 3) & 0x1f;
			if( caRuleIndex != 0x1f-1 ) { // check if not disabled
				slimRnn.pieces[context.pieceIndex].ca.rule = caRuleIndex;
			}
		}
		else if( encoding[0] == 1 ) {
			encoding[1] = (instruction >> 1) & 1;
			if( encoding[1] == 0 ) {
				unknownEncoding = false;

				context.pieceIndex = (instruction >> 2) & 0x3f;
			}
			else {
				uint prefix1 = (instruction >> 2) & 7;
				if( prefix1 == 2 ) {
					unknownEncoding = false;

					slimRnn.pieces[context.pieceIndex].ca.readofIndex = (instruction >> (8-3)) & 7;
				}
				else if( prefix1 == 3 ) {
					unknownEncoding = false;

					slimRnn.pieces[context.pieceIndex].enabled = ((instruction >> 7) & 1) != 0;
				}
			}
		}
	}

	static void debugInstructionToConsole(uint instruction) {
		import std.stdio : writeln;
		import std.format : format;

		bool unknownEncoding;

		uint encoding[2];

		encoding[0] = instruction & 1;
		if( encoding[0] == 0 ) {
			unknownEncoding = false;

			string caRuleIndexString = "<disabled>";

			uint inputIndex = (instruction >> 1) & 3;
			uint caRuleIndex = (instruction >> 3) & 0x1f;
			if( caRuleIndex != 0x1f-1 ) { // check if not disabled
				caRuleIndexString = "%s".format(caRuleIndex);
			}

			writeln("instruction - set inputIndex and (conditionally) set CA rule index : inputIndex=%s, caRuleIndex=%s".format(inputIndex, caRuleIndexString));
		}
		else if( encoding[0] == 1 ) {
			unknownEncoding = false;

			encoding[1] = (instruction >> 1) & 1;
			if( encoding[1] == 0 ) {
				uint pieceIndex = (instruction >> 2) & 0x3f;
				writeln("instruction - set piece index                                     : pieceIndex=%s".format(pieceIndex));
			}
			else {
				uint prefix1 = (instruction >> 2) & 7;
				if( prefix1 == 2 ) {
					unknownEncoding = false;

					uint readofIndex = (instruction >> (8-3)) & 7;
					writeln("instruction - set CA readoff index                                     : readoffIndex=%s".format(readofIndex));
				}
				else if( prefix1 == 3 ) {
					unknownEncoding = false;

					bool enabled = ((instruction >> 7) & 1) != 0;
					writeln("instruction - enable piece                                     : enabled=%s".format(enabled));
				}
			}
		}

		if( unknownEncoding ) {
			writeln("instruction - <unknown encoding>");
		}
	}
}

// converts a bitcount to the mask to mask these bits out
uint bitsToMask(uint bits) {
	return (1 << (bits+1))-1;
}

// we do search for configurations of some parameters of the SLIM RNN via levin search
// as described in schmidhurs SLIM RNN paper

final class SlimRnnLevinProblem : LevinProblem {
	final this(SlimRnn slimRnn) {
		this.slimRnn = slimRnn;
	}

	private SlimRnn slimRnn;

	final void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
		hasHalted = false;

		SlimRnn workingSlimRnn = slimRnn.clone();

		InstructionInterpretationContext instructionInterpretationContext;

		import std.stdio;
		writeln("[debug]SlimRnnLevinProblem   :  program.instructions=", program.instructions, " maxNumberOfStepsToExecute=", maxNumberOfStepsToExecute);

		foreach( iterationInstruction; program.instructions ) {
			// TODO< exeute instruction on SlimRnn >

			bool unknownEncoding;
			InstructionEncoding.executeInstructionOnSlimRnn(instructionInterpretationContext, workingSlimRnn, iterationInstruction, /*out*/ unknownEncoding);
			if( unknownEncoding ) {
				return; // we immediatly return because the encoding is invalid, this is not an hard error
			}
		}






		// init start state
		workingSlimRnn.map.arr[0] = 0.0f;
		workingSlimRnn.map.arr[1] = 0.5f;
		workingSlimRnn.map.arr[2] = 0.5f;

		uint maxIterations = maxNumberOfStepsToExecute;
		uint iterations;
		bool wasTerminated;
		workingSlimRnn.loop(maxIterations, /*out*/ iterations, /*out*/ wasTerminated);

		hasHalted = wasTerminated;
	}
}

// some rules are not used because we don't have enough space for them
static const uint[] CARULES = 
[30, 62, 90, 102, 110, 126, 150, 158, ] ~ // the usual extremly useful rules
[2, 4, 6, 9, /*14,*/ 18, 20, 22, 24, 25, 26, 28, 37, 41, 45, /*47,*/ 57, 61, 86, 89, 107, 121, // followed by the more chaotic ones 
60, // see animation http://mathworld.wolfram.com/Rule60.html 
90, // see animation http://mathworld.wolfram.com/Rule90.html
102, // see animation http://mathworld.wolfram.com/Rule102.html
150, // see animation http://mathworld.wolfram.com/Rule150.html

]; // rules for the celular automata
static const uint TRESHOLDQUANTISATION = 8;

static const uint BITSFORCARULES = 5;

static assert((1 << BITSFORCARULES) >= CARULES.length);

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

    levinSearch.numberOfInstructions = 1 << 8;
    levinSearch.c = 0.1;
    levinSearch.maxIterations = 5000;

    uint programLength = 3; // equivalent to 24 bit

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
	slimRnn.pieces[0].enabled = false;


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
	slimRnn.pieces[1].enabled = false;





    LevinProblem levinProblem = new SlimRnnLevinProblem(slimRnn);

    uint numberOfIterations = 50000;
    bool done;
    Solution solution = levinSearch.iterate(levinProblem, numberOfIterations, /*out*/ done);
    if( done ) {
        import std.stdio;
    	writeln("done=", done);

    	// debug program
    	foreach( iterationInstruction; solution.program.instructions ) {
    		InstructionEncoding.debugInstructionToConsole(iterationInstruction);
    	}
    }



    import std.stdio;
    writeln("done=", done);
}
