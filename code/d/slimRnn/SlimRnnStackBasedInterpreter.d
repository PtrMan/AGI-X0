module slimRnn.SlimRnnStackBasedInterpreter;

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
		
		pieceExecutions.length = numberOfPieces;
		foreach( ref iterationPiece; pieceExecutions ) {
			iterationPiece = PieceExecution.init;
		}
	}
}

struct SlimRnnStackBasedInterpreter {
	static void interpret(SlimRnnStackBasedManipulationInstruction[] instructions, SlimRnn slimRnn, out bool success) {
		success = false;

		SlimRnnStackBasedInterpretationContext context;
		context.reset(slimRnn.pieces.length);

		foreach( iterationInstruction; instructions ) {
			final switch( iterationInstruction.type ) with (SlimRnnStackBasedManipulationInstruction.EnumType) {
				case PUSHVALUE: interpretInstructionPushValue(iterationInstruction, context, slimRnn, success); break;
				case POP: interpretInstructionPop(iterationInstruction, context, slimRnn, success); break;
				case SETINPUTTHRESHOLDFORPIECEATSTACKTOP: interpretInstructionSetInputThresholdForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETOUPUTSTRENGTHFORPIECEATSTACKTOP: interpretInstructionSetOutputStrengthForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETACTIVATEFORPIECEACTIVATIONVARIABLEATSTACKTOP: interpretInstructionSetActivateForPieceActivationVariableStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETTYPEVARIABLEFORPIECEATSTACKTOP: interpretInstructionSetTypeVariableForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP: interpretInstructionSetThresholdForInputIndexForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
				case RESETNEURONACTIVATION: interpretInstructionResetNeuronActivation(iterationInstruction, context, slimRnn, success); break;
				case SETFUNCTIONTYPEFORPIECEATSTACKTOP: interpretInstructionSetFunctionTypeForPieceAtStackTop(iterationInstruction, context, slimRnn, success); break;
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
		slimRnn.pieces[pieceIndex].inputs[instruction.inputIndex].value = instruction.threshold;
		success = true;
	}

	private static void interpretInstructionSetOutputStrengthForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		slimRnn.pieces[pieceIndex].output.value = instruction.strength;
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

		rewireInputsToDefaultInputIfRequired(slimRnn, pieceIndex, instruction.inputIndex);
		slimRnn.pieces[pieceIndex].inputs[instruction.inputIndex].value = instruction.threshold;
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

	private static void interpretInstructionSetFunctionTypeForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		slimRnn.pieces[pieceIndex].functionType = instruction.functionType;
		success = true;
	}

	private static void interpretInstructionSetSwitchboardIndexForInputIndexAndPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		rewireInputsToDefaultInputIfRequired(slimRnn, pieceIndex, instruction.inputIndex);
		slimRnn.pieces[pieceIndex].inputs[instruction.inputIndex].coordinate.x = instruction.switchboardIndex;
		success = true;
	}

	private static void interpretInstructionSetSwitchboardIndexForOutputForPieceAtStackTop(SlimRnnStackBasedManipulationInstruction instruction, ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, out bool success) {
		success = false;
		if( context.stack.isEmpty ) {
			return;
		}
		uint pieceIndex = context.stack.top;

		slimRnn.pieces[pieceIndex].output.coordinate.x = instruction.outputIndex;

		success = true;
	}



	// helper which checks if the # of inputs is <= to the required index and if so then it adds the new inputs and rewires the inputs to the defaultInputIndex
	private static void rewireInputsToDefaultInputIfRequired(SlimRnn slimRnn, uint pieceIndex, uint requestedIndex) {
		if( slimRnn.pieces[pieceIndex].inputs.length > requestedIndex ) {
			return;
		}

		size_t oldLength = slimRnn.pieces[pieceIndex].inputs.length;

		slimRnn.pieces[pieceIndex].inputs.length = requestedIndex+1;

		foreach( i; oldLength..slimRnn.pieces[pieceIndex].inputs.length ) {
			slimRnn.pieces[pieceIndex].inputs[i].coordinate.x = slimRnn.defaultInputSwitchboardIndex;
		}
	}



	// executes the piece execution for all pieces of the SLIM-RNN
	// a piece execution changes some variables of the piece
	private static void commitAllPieceExecutions(ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn) {
		foreach( i; 0..slimRnn.pieces.length ) {
			commitPieceExecution(context, slimRnn, i);
		}
	}

	private static void commitPieceExecution(ref SlimRnnStackBasedInterpretationContext context, SlimRnn slimRnn, uint pieceIndex) {
		if( context.pieceExecutions[pieceIndex].flagEnableNeuron ) {
			slimRnn.pieces[pieceIndex].enabled = context.pieceExecutions[pieceIndex].enableNeuronValue;
		}

		if( context.pieceExecutions[pieceIndex].flagSetType ) {
			slimRnn.pieces[pieceIndex].type = cast(Piece.EnumType)context.pieceExecutions[pieceIndex].typeValue;
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
