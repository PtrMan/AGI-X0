module slimRnn.SlimRnnStackBasedInterpreter;

import memoryLowlevel.StackAllocator;

import slimRnn.SlimRnnStackBasedManipulationInstruction;
import slimRnn.SlimRnn;

// privdes an interpreter for the stack based SLIM-RNN instructions
// and the context

struct SlimRnnStackBasedInterpretationContext {
	static struct PieceExecution {
		bool flagEnableNeuron;
		bool enableNeuronValue;

		bool flagSetType;
		uint typeValue;
	}

	int pieceIndex;

	uint stack[];

	PieceExecution[] pieceExecutions; // stores the actions to to on the pieces as a map
	                                  // with flushing commands these get executed on the pieces

	final void reset(size_t numberOfPieces) {
		pieceIndex = -1;
		stack.length = 0;
		
		if( pieceExecutions.length != numberOfPieces ) {
			pieceExecutions.length = numberOfPieces;
		}
		foreach( ref iterationPiece; pieceExecutions ) {
			iterationPiece = PieceExecution.init;
		}
	}
}

struct SlimRnnStackBasedInterpreter {
	// context is carried around as an optimization, should be externally carried around too, but it is not necessary
	static void interpret(ref SlimRnnStackBasedInterpretationContext context, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) instructions, SlimRnn slimRnn, out bool success) {
		success = false;

		context.reset(slimRnn.pieceAccessors.length);

		foreach( instructionIndex; 0..instructions.length ) {
			auto iterationInstruction = instructions.getAtIndex(instructionIndex);

			final switch( iterationInstruction.type ) with (SlimRnnStackBasedManipulationInstruction.EnumType) {
				case PUSHVALUE: interpretInstructionPushValue(iterationInstruction, context, slimRnn, success); break;
				case POP: interpretInstructionPop(iterationInstruction, context, slimRnn, success); break;
				case SETINPUTTHRESHOLDFORPIECEATSTACKTOP: interpretInstructionSetInputThresholdForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETOUPUTSTRENGTHFORPIECEATSTACKTOP: interpretInstructionSetOutputStrengthForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETACTIVATEFORPIECEACTIVATIONVARIABLEATSTACKTOP: interpretInstructionSetActivateForPieceActivationVariableStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETTYPEVARIABLEFORPIECEATSTACKTOP: interpretInstructionSetTypeVariableForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP: interpretInstructionSetThresholdForInputIndexForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case RESETNEURONACTIVATION: interpretInstructionResetNeuronActivation(iterationInstruction, context, slimRnn, success); break;
				case SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP: interpretInstructionSetSwitchboardIndexForInputIndexAndPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP: interpretInstructionSetSwitchboardIndexForOutputForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
			}
		}

		commitAllPieceExecutions(context, slimRnn);
	}

	private static void interpretInstructionPushValue(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		context.stack.push(instruction.valueToPush);
		success = true;
	}

	private static void interpretInstructionPop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		context.stack.pop();
		success = true;
	}

	private static void interpretInstructionSetInputThresholdForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		rewireInputsToDefaultInputIfRequired(slimRnn, pieceIndex, instruction.inputIndex);
		
		auto scratchpadInput = slimRnn.pieceAccessors[pieceIndex].inputs[instruction.inputIndex];
		scratchpadInput.value = instruction.threshold;
		slimRnn.pieceAccessors[pieceIndex].setInputAt(instruction.inputIndex, scratchpadInput);
		success = true;
	}

	private static void interpretInstructionSetOutputStrengthForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		auto scratchpadOutput = slimRnn.pieceAccessors[pieceIndex].output;
		scratchpadOutput.value = instruction.strength;
		slimRnn.pieceAccessors[pieceIndex].output = scratchpadOutput;
		success = true;
	}

	private static void interpretInstructionSetActivateForPieceActivationVariableStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		context.pieceExecutions[pieceIndex].flagEnableNeuron = true;
		context.pieceExecutions[pieceIndex].enableNeuronValue = instruction.activateFlag;
		success = true;
	}

	private static void interpretInstructionSetTypeVariableForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		context.pieceExecutions[pieceIndex].flagSetType = true;
		context.pieceExecutions[pieceIndex].typeValue = instruction.typeVariable;
		success = true;
	}

	private static void interpretInstructionSetThresholdForInputIndexForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		auto scratchpadInput = slimRnn.pieceAccessors[pieceIndex].inputs[instruction.inputIndex];
		scratchpadInput.value = instruction.threshold;
		slimRnn.pieceAccessors[pieceIndex].setInputAt(instruction.inputIndex, scratchpadInput);
		success = true;
	}

	private static void interpretInstructionResetNeuronActivation(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		context.pieceExecutions[pieceIndex].flagEnableNeuron = false;
		success = true;
	}

	private static void interpretInstructionSetSwitchboardIndexForInputIndexAndPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		rewireInputsToDefaultInputIfRequired(slimRnn, pieceIndex, instruction.inputIndex);
		auto scratchpadInput = slimRnn.pieceAccessors[pieceIndex].inputs[instruction.inputIndex];
		scratchpadInput.coordinate.x = instruction.switchboardIndex;
		slimRnn.pieceAccessors[pieceIndex].setInputAt(instruction.inputIndex, scratchpadInput);
		success = true;
	}

	private static void interpretInstructionSetSwitchboardIndexForOutputForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		slimRnn.pieceAccessors[pieceIndex].output.coordinate.x = instruction.outputIndex;

		success = true;
	}



	// helper which checks if the # of inputs is <= to the required index and if so then it adds the new inputs and rewires the inputs to the defaultInputIndex
	private static void rewireInputsToDefaultInputIfRequired(SlimRnn slimRnn, uint pieceIndex, uint requestedIndex) {
		if( slimRnn.pieceAccessors[pieceIndex].inputs.length > requestedIndex ) {
			return;
		}

		size_t oldLength = slimRnn.pieceAccessors[pieceIndex].inputs.length;

		slimRnn.pieceAccessors[pieceIndex].setInputLength(requestedIndex+1);

		foreach( i; oldLength..slimRnn.pieceAccessors[pieceIndex].inputs.length ) {
			auto scratchpadInput = slimRnn.pieceAccessors[pieceIndex].inputs[i];
			scratchpadInput.coordinate.x = slimRnn.defaultInputSwitchboardIndex;
			slimRnn.pieceAccessors[pieceIndex].setInputAt(i, scratchpadInput);
		}
	}



	// executes the piece execution for all pieces of the SLIM-RNN
	// a piece execution changes some variables of the piece
	private static void commitAllPieceExecutions(ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn) {
		foreach( i; 0..slimRnn.pieceAccessors.length ) {
			commitPieceExecution(context, slimRnn, i);
		}
	}

	private static void commitPieceExecution(ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, size_t pieceIndex) {
		if( context.pieceExecutions[pieceIndex].flagEnableNeuron ) {
			slimRnn.pieceAccessors[pieceIndex].enabled = context.pieceExecutions[pieceIndex].enableNeuronValue;
		}

		if( context.pieceExecutions[pieceIndex].flagSetType ) {
			slimRnn.pieceAccessors[pieceIndex].type = cast(Piece.EnumType)context.pieceExecutions[pieceIndex].typeValue;
		}
	}
	
}



// stack helper, belongs into misced

bool isEmpty(uint[] arr) {
	return arr.length == 0;
}

uint pop(ref uint[] arr) {
	assert(arr.length >= 1);
	uint result = arr[$-1];
	arr.length = arr.length-1;
	return result;
}

void push(ref uint[] arr, uint value) {
	arr ~= value;
}

uint top(uint[] arr) {
	return arr[$-1];
}
