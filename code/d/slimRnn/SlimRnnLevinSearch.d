module slimRnn.SlimRnnLevinSearch;

import std.stdint;

import memoryLowlevel.StackAllocator;

import slimRnn.SlimRnn;
import search.levin.LevinSearch;

import slimRnn.SlimRnnStackBasedManipulationInstruction;
static import slimRnn.programEncoding.Vliw2;                    // stack based "very large instruction word"-style instructions on which levin search can operate
import slimRnn.SlimRnnStackBasedInterpreter;

/*
struct StackBasedInstructionInterpretationContext {
	int pieceIndex;

	uint[] stack;

	final void reset() {
		pieceIndex = -1;
		stack.length = 0;
	}
}
*/





struct Stack6BitInstructionEncodingStack1 {



	/+ uncommented because its outdate, but the encoding is still useful in the future
	static void decodeAndExecute(ref InstructionInterpretationContext context, SlimRnn slimRnn, uint instruction, out bool unknownEncoding, out bool executionSuccess) {
		Instruction instructionDecoded = decode(instruction, /*out*/unknownEncoding);
		interpret(context, slimRnn, instructionDecoded, /*out*/executionSuccess);
	}


/*
this instruction set works with 5 bits for the neuron id's and other values
it operates on a context with an stack and the current pieceIndex

- push working value (1xxxxx)
- set neuron index to #top, [optionally] activate neuron #top, set type of neuron to either CA or XOR(0-10-aa-t)
   - aa : 10 : disable neuron
   - aa : 11 : enable neuron
   - aa : 00 : don't touch
   - aa : 01 : NOT USED ENCODING

   - t : 0 : set type to CA
   - t : 1 : set type to XOR

- set neuron index to #top, [optionally] activate neuron #top, set type of neuron to either CLASSICAL or CA(0-11-aa-t)
   - aa : 10 : disable neuron
   - aa : 11 : enable neuron
   - aa : 00 : don't touch
   - aa : 01 : NOT USED ENCODING

   - t : 0 : set type to CLASSIC
   - t : 1 : set type to <NOT USED ENCODING, do not use this >
- set threshold of input index 'a' to #top (0-00-00-a)
- set threshold of input index 'aa' to #top (0-00-1-aa)
- set output strength to #top (0-01-100)
                               # ## ###

*/

	// decodes an instruction to a "Instruction" structure
	static private Instruction decode(uint instruction, out bool unknownEncoding) {
		unknownEncoding = true;

		if( ((instruction >> 5) & 1) == 1 ) {
			uint valueToPush = instruction & 0x1f;
			unknownEncoding = false;
			return Instruction.makePush(valueToPush);
		}
		else {
			uint followingEncoding = (instruction >> 3) & 3;
			if( followingEncoding == 2 ) { // set neuron index to #top, [optionally] activate neuron #top, set type of neuron to either CA or XOR
				bool activateFlagActive = (instruction & (1 << 2)) != 0;
				bool activateFlag = (instruction & (1 << 1)) != 0;
				
				if( !activateFlagActive & activateFlag ) {
					return Instruction.init;
				}

				bool typeFlag = (instruction & 1) != 0;

				if( typeFlag == true ) {
					// not valid encoding because its not used
					return Instruction.init;
				}

				Piece.EnumType setNeuronTypeToType = typeFlag ? Piece.EnumType.CLASSICNEURON : Piece.EnumType.CLASSICNEURON;

				unknownEncoding = false;
				return  Instruction.makeSetNeuronIndexToTopActivateNeuronSetTypeOfNeuron(activateFlagActive, activateFlag, setNeuronTypeToType);
			}
			else if( followingEncoding == 3 ) {
				bool activateFlagActive = (instruction & (1 << 2)) != 0;
				bool activateFlag = (instruction & (1 << 1)) != 0;
				
				if( !activateFlagActive & activateFlag ) {
					return Instruction.init;
				}

				bool typeFlag = (instruction & 1) != 0;
				Piece.EnumType setNeuronTypeToType = typeFlag ? Piece.EnumType.CA : Piece.EnumType.XOR;

				unknownEncoding = false;
				return Instruction.makeSetNeuronIndexToTopActivateNeuronSetTypeOfNeuron(activateFlagActive, activateFlag, setNeuronTypeToType);
			}
			else if( followingEncoding == 0 ) {
				bool followingEncoding = (instruction & (1 << 2)) != 0;
				if( followingEncoding ) {
					// the size of the input index is two bits
					uint inputIndex = instruction & 3;
					unknownEncoding = false;
					return Instruction.makeSetInputThreshold(inputIndex);
				}
				else {
					// TODO< there is an unused encoding possibility here >

					// the size of the input index is one bit
					uint inputIndex = instruction & 1;
					unknownEncoding = false;
					return Instruction.makeSetInputThreshold(inputIndex);
				}
			}
			else {
				if( instruction == 0xC ) {
					unknownEncoding = false;
					return Instruction.makeSetOutputStrengthToTop();
				}
				// else we are here

				// not used/valid encoding
				return Instruction.init;
			}
		}
	}

	static private void interpret(ref InstructionInterpretationContext context, SlimRnn slimRnn, Instruction instruction, out bool executionSuccess) {
		executionSuccess = false;

		if( instruction.type == Instruction.EnumType.PUSHVALUE ) {
			context.stack.push(instruction.value);

			executionSuccess = true;
		}
		else if( instruction.type == Instruction.EnumType.SETNEURONINDEXTOTOPACTIVATENEURONSETTYPEOFNEURON ) {
			if( context.stack.isEmpty ) { // not valid to execute this instruction with an empty stack
				return;
			}

			// set the pieceIndex
			context.pieceIndex = context.stack.top();

			// enable
			if( instruction.activateFlagActive ) {
				slimRnn.pieces[context.pieceIndex].enabled = instruction.activateFlag;
			}

			// set type
			slimRnn.pieces[context.pieceIndex].type = instruction.setNeuronTypeToType;

			executionSuccess = true;
		}
		else if( instruction.type == Instruction.EnumType.SETINPUTTHRESHOLD ) {
			if( context.pieceIndex == -1 ) { // can't execute this instruction of the pieceIndex is not set
				return;
			}

			if( context.stack.isEmpty ) { // can't execute this instruction on an empty stack
				return;
			}

			if( slimRnn.pieces[context.pieceIndex].inputs.length >= instruction.inputIndex ) { // can't execute of input index doesn't match up
				return;
			}

			slimRnn.pieces[context.pieceIndex].inputs[instruction.inputIndex].value = (1.0f/cast(float)(1 << 5)) * cast(float)context.stack.top;

			executionSuccess = true;
		}
	}+/
}
	



/+
uncommented because encoding can still be useful in the future

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
+/

// we do search for configurations of some parameters of the SLIM RNN via levin search
// as described in schmidhurs SLIM RNN paper

final class SlimRnnLevinProblem : LevinProblem {
	static struct TestSetElement {
		bool input[2];
		bool expectedResult;

		static TestSetElement make(bool input[2], bool expectedResult) {
			TestSetElement result;
			result.input = input;
			result.expectedResult = expectedResult;
			return result;
		}
	}

	TestSetElement[] testset;

	final this(SlimRnn slimRnn) {
		this.slimRnn = slimRnn;

		workingSlimRnn = slimRnn.cloneUnderCowAsRoot();
	}

	private SlimRnn slimRnn, workingSlimRnn;

	private SlimRnnStackBasedInterpretationContext slimRnnStackBasedInterpretationContext;

	final void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
		hasHalted = false;

		// "fast" copy, we don't allocate the class, we don't allocate all arrays
		workingSlimRnn.flushOpaquePieces();

		uint[] levinProgram = program.instructions;

		// (1) decode levin program instructions to SlimRnn instructions
		// (2) execute/interpret instructions on SLIM-RNN
		if(true){
			bool invalidEncoding;
			StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) stackBasedInstructionsStack;
			translateLevinProgramInstructionsToStackBasedInstructions(levinProgram, stackBasedInstructionsStack, /*out*/ invalidEncoding);

			// overwrite stack based instruction with our program which solves the problem
			if( invalidEncoding ) {
				return; // we immediatly return because the encoding is invalid, this is not an hard error
			}

			bool interpretationSuccess;
			SlimRnnStackBasedInterpreter.interpret(slimRnnStackBasedInterpretationContext, stackBasedInstructionsStack, workingSlimRnn, /*out*/ interpretationSuccess);
			if( !interpretationSuccess ) {
				return; // it is an hard error if we can't interpret the instructions
			}
		}

		// debugging : print levin program, instructions and resulting SLIM-RNN
		if(false) {
			import std.stdio;
			writeln("levinProgram=", levinProgram);

			// debug instructions
			writeln("instructions:");

			bool invalidEncoding;
			StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) stackBasedInstructionsStack;
			translateLevinProgramInstructionsToStackBasedInstructions(levinProgram, stackBasedInstructionsStack, /*out*/ invalidEncoding);
			if( invalidEncoding ) {
				writeln("<invalid encoding>");
			}
			else {
				foreach(i;0..stackBasedInstructionsStack.length) {
					SlimRnnStackBasedManipulationInstruction iterationInstruction = stackBasedInstructionsStack.getAtIndex(i);
					writeln(iterationInstruction.humanReadableDescription());
				}
			}

				

			writeln("=> SLIM-RNN:");
			writeln(workingSlimRnn.humanReadableDescriptionOfPieces());





		}

		/*
		{
			workingSlimRnn.pieces[0].type = Piece.EnumType.XOR;

	workingSlimRnn.pieces[0].inputs = 
	[
		CoordinateWithValue.make(CoordinateType.make(0), 0.5f),
		CoordinateWithValue.make(CoordinateType.make(1), 0.5f)
	];

	workingSlimRnn.pieces[0].output = CoordinateWithValue.make(CoordinateType.make(2), 0.5f);
	workingSlimRnn.pieces[0].enabled = true;
		}
		*/
		
		
		// (3) prepare SLIM-RNN
		bool isValidNetwork;
		workingSlimRnn.compile(/*out*/ isValidNetwork);
		if( !isValidNetwork ) { // check for compilation error because of an invalid network structure
			return;
		}

		// (4) execute SLIM-RNN



		// we check the network for the testset
		foreach( testsetElement; testset ) {
			// called before returning in case of failiure
			void innerFnReportFailure() {
				return;

				import std.stdio;
				writeln("DEBUG innerFnReportFailure()");
				throw new Exception("X");
			}

			uint maxIterations = maxNumberOfStepsToExecute;
			uint iterations;
			bool wasTerminated, executionError;

			// init start state
			workingSlimRnn.resetMap();
			workingSlimRnn.map.arr[0] = testsetElement.input[0] ? 1.0f : 0.0f;
			workingSlimRnn.map.arr[1] = testsetElement.input[1] ? 1.0f : 0.0f;

			workingSlimRnn.run(maxIterations, /*out*/iterations, /*out*/wasTerminated, /*out*/executionError);

			

			if( executionError ) {
				// is an hard error if an execution error happend
				innerFnReportFailure();
				return;
			}

			if( !wasTerminated ) {
				// if the SLIM-RNN didn't terminate we just have to continue the search
				innerFnReportFailure();
				return;
			}

			// DEBUG
			///import std.stdio;
			///writeln("DEBUG executeProgram()   networkResult=", workingSlimRnn.map.arr[2]);

			// read out result
			bool networkResult = workingSlimRnn.map.arr[2] > 0.5f;
			if( networkResult != testsetElement.expectedResult ) {
				// result is wrong
				innerFnReportFailure();
				return; // continue the search 
			}
		}

		// if we are here the network does what is requested
		
		// debug found program
		import std.stdio;
		bool invalidEncoding;
		StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) stackBasedInstructionsStack;
    	translateLevinProgramInstructionsToStackBasedInstructions(levinProgram, stackBasedInstructionsStack, /*out*/ invalidEncoding);

    	writeln("levin program=", levinProgram, ", translated to VLIW1 instruction-set:");
    	foreach( instructionIndex; 0..stackBasedInstructionsStack.length ) {
    		auto iterationInstruction = stackBasedInstructionsStack.getAtIndex(instructionIndex);

    		writeln("\t" ~ iterationInstruction.humanReadableDescription());
    	}

		hasHalted = true;
	}
}

// some rules are not used because we don't have enough space for them
static const uint[] CARULES = [
254, // implements the OR function
204, // identity

// the usual extremly useful rules
30, 62, 90, 102, 110, 126, 150, 158,


//2,   not used because it can be used for memory, but the implementation will use stateless CA's
//4,   the same as above
//6,   the same as above
//9,   the same as above
//20,  the same as above
//24,  the same as above

//14,  redundant
//47,  redundant 
18, 22, 25, 26, 28, 37, 41, 
45,
57, 61, 86, 89, 107, 121,



// followed by the more chaotic ones 
60, // see animation http://mathworld.wolfram.com/Rule60.html 
90, // see animation http://mathworld.wolfram.com/Rule90.html
102, // see animation http://mathworld.wolfram.com/Rule102.html
150, // see animation http://mathworld.wolfram.com/Rule150.html
]; // rules for the celular automata
static const uint TRESHOLDQUANTISATION = 8;

static const uint BITSFORCARULES = 5;

//                                    we add CASTART because the table has the Piece/Neuron rules followed by the CA rules
static assert((1 << BITSFORCARULES) + Piece.EnumType._CASTART >= CARULES.length);

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

    import core.time;

    uint reportingTriedProgramCounter = 0; // used to measure how many programs get tried per second
    uint reportingNoTimecheckSinceTriedPrograms = 0;
    uint reportingNumberOfTriedPrograms = 0;
    MonoTime timeOfLastReport = MonoTime.currTime;
    levinSearch.reportProgramExecution = (LevinProgram currentProgram) {
    	import std.stdio : writeln;

    	reportingTriedProgramCounter++;
    	reportingNoTimecheckSinceTriedPrograms++;
    	reportingNumberOfTriedPrograms++;

    	if( reportingNoTimecheckSinceTriedPrograms >= 10000 ) {
    		
    		import std.conv : to;
    		TickDuration durationSinceLastReport = (MonoTime.currTime - timeOfLastReport).to!TickDuration; // TickDuration is deprecated but we don't care
    		if( durationSinceLastReport.msecs >= 5000 ) {
    			double programsTriedPerSec = cast(double)reportingTriedProgramCounter / (cast(double)durationSinceLastReport.usecs / 1000000.0);
    			writeln("[debug]main : programs/sec = ",  programsTriedPerSec, " tried since last report ", reportingTriedProgramCounter, " programs, ##=", reportingNumberOfTriedPrograms);

    			reportingTriedProgramCounter = 0;
    			timeOfLastReport = MonoTime.currTime;
    		}

    		reportingNoTimecheckSinceTriedPrograms = 0;
    	}



    	// report program
    	/// uncommented because it results in too much output
    	///bool invalidEncoding;
    	///SlimRnnStackBasedManipulationInstruction[] stackBasedInstructions = translateLevinProgramInstructionsToStackBasedInstructions(currentProgram.instructions, /*out*/ invalidEncoding);

    	///writeln("levin program=", currentProgram.instructions, ", translated to VLIW1 instruction-set:");
    	///foreach( SlimRnnStackBasedManipulationInstruction iterationInstruction; stackBasedInstructions ) {
    	///	writeln("\t" ~ iterationInstruction.humanReadableDescription());
    	///}
    };

    // not correct number just for testing
    levinSearch.numberOfInstructions = 1 << 12; // 12 bit because we use LVIW1 levin-instruction encoding
    levinSearch.c = 0.1;
    levinSearch.maxIterations = 50; // not a lot because the function is simple

    uint programLength = 2;//DEBUG 2; // equivalent to 24 bit,  is enougth to find the program to acomplish the XOR problem

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








    uint numberOfPieces = 1 << 5;
	SlimRnnCtorParameters ctorParameters;
	ctorParameters.mapSize[0] = 10;
	ctorParameters.numberOfPieces = numberOfPieces;




	SlimRnn slimRnn = SlimRnn.makeRoot(ctorParameters);
	slimRnn.resetPiecesToTypeByCount(numberOfPieces, Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE);

	slimRnn.terminal.coordinate.x = ctorParameters.mapSize[0]-cast(size_t)1; // last map element is the terminal
	slimRnn.terminal.value = 0.1f;
	slimRnn.defaultInputSwitchboardIndex = ctorParameters.mapSize[0]-cast(size_t)2; // switchboard element before the last elements which is the terminal, is not zero because the programs should generalize better
	// position length-3 is used as a delay for the termination signal

	// add an neuron which sends the termination after an delay

	
	slimRnn.pieceAccessors[numberOfPieces-2].type = Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE;

	slimRnn.pieceAccessors[numberOfPieces-2].inputs = 
	[
	];

	slimRnn.pieceAccessors[numberOfPieces-2].output = CoordinateWithValue.make(CoordinateType.make(ctorParameters.mapSize[0]-3), 0.5f);
	slimRnn.pieceAccessors[numberOfPieces-2].enabled = true;
	

	slimRnn.pieceAccessors[numberOfPieces-1].type = Piece.EnumType.CLASSICNEURON_MULTIPLICATIVE;

	slimRnn.pieceAccessors[numberOfPieces-1].inputs = 
	[
		CoordinateWithValue.make(CoordinateType.make(ctorParameters.mapSize[0]-cast(size_t)3), 0.3f)
	];

	slimRnn.pieceAccessors[numberOfPieces-1].output = CoordinateWithValue.make(CoordinateType.make(ctorParameters.mapSize[0]-1), 0.2f);
	slimRnn.pieceAccessors[numberOfPieces-1].enabled = true; 


	SlimRnnLevinProblem slimRnnLevinProblem = new SlimRnnLevinProblem(slimRnn);
	slimRnnLevinProblem.testset = [
		SlimRnnLevinProblem.TestSetElement.make([false, false], false),
		SlimRnnLevinProblem.TestSetElement.make([false, true], true),
		SlimRnnLevinProblem.TestSetElement.make([true, false], true),
		SlimRnnLevinProblem.TestSetElement.make([true, true], false),
	];


    LevinProblem levinProblem = slimRnnLevinProblem;

    uint numberOfIterations = programLength;


	bool done;
    Solution solution = levinSearch.iterate(levinProblem, numberOfIterations, /*out*/ done);
    if( done ) {
        import std.stdio;
    	writeln("done=", done);

    	// debug program
    	foreach( iterationInstruction; solution.program.instructions ) {
    		// TODO< >
    		// uncommented because its the old code InstructionEncoding.debugInstructionToConsole(iterationInstruction);
    	}
    }



    import std.stdio;
    writeln("done=", done);
    writeln("##=", reportingNumberOfTriedPrograms);
}

// uses the VLIW1 encoding scheme for decoding
private void translateLevinProgramInstructionsToStackBasedInstructions(uint[] instructions, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) resultInstructionStack, out bool invalidEncoding) {
	invalidEncoding = false;


	uint translateCellularAutomataIndexToCellularAutomataRule(uint index) {
		if(index < CARULES.length) {
			return CARULES[index];
		}
		else { // else we default to the rule 110 because its the most useful
			return 110;
		}
	}

	// this converts from the index of the type (to be set for the neuron/piece) to the value which gets emitted as the instruction
	uint translateTypeIndexOfPieceToType(uint type) {
		if( type < Piece.EnumType._CASTART ) { // if it is not an CA type then we just take the raw value
			return type;
		}
		else {
			return translateCellularAutomataIndexToCellularAutomataRule(type - Piece.EnumType._CASTART);
		}
	}

	foreach( uint iterationInstruction; instructions ) {
		.slimRnn.programEncoding.Vliw2.vliw2emitInstructions(iterationInstruction, &translateTypeIndexOfPieceToType, resultInstructionStack, /*out*/ invalidEncoding);
		if( invalidEncoding ) {
			return;
		}
	}

	return;
}
