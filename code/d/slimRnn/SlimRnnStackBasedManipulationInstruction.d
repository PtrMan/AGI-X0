module slimRnn.SlimRnnStackBasedManipulationInstruction;

private alias SlimRnnStackBasedManipulationInstruction Instruction;

// stack based instruction to manipulate the slim RNN
struct SlimRnnStackBasedManipulationInstruction {
	enum EnumType {
		ACTIVATENEURON, // sets the activation variable for the neuron at the index, requires argument
		DEACTIVATENEURON, // resets the activation variable for the neuron at the index, , requires argument
		ACTIVATENEURONATSTACKTOP,// sets the activation variable for the neuron at the top of the stack
		DEACTIVATENEURONATSTACKTOP,// sets the activation variable for the neuron at the top of the stack

		COMMIT, // commits all stored changes to all neurons

		// stack manipulation
		DUPLICATESTACKTOP,
		POP,
		POP2,
		PUSHNEXTNEURONINDEX,
		PUSHCONSTANT, // requires argument
		SWAPSTACK,                  // swaps stack(top) and stack(top-1)
		                            // if just one element is on the stack it has no effect and causes no execution error


		NOP,
		RESETNEURONACTIVATIONFORNEURONATTOP,
		

		SETOUTPUTHSTRENGTHTOVALUEFORNEURONATSTACKTOP,
		SETTYPEVARIABLEFORNEURONATSTACKTOP,
		SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP,
		SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP,

		LINKOUTPUTOFNEURONTOSLIMRNNOUTPUTFORNEURONATSTACKTOP,

		// instructions to be revised

		SETINPUTTHRESHOLDFORPIECEATSTACKTOP, // set threshold of input index to #top
		
		SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP,
		
		RESETNEURONACTIVATION,
		

	}

	EnumType type;

	// optional
	union {
		uint typeVariable, inputIndex, valueToPush, functionType, outputIndex, neuronIndex;
	}

	// optional
	union {
		uint switchboardIndex;
	}

	// optional
	bool activateFlag;

	// optional
	union {
		float strength, threshold;
	}

	static Instruction makeActivateNeuron(uint neuronIndex) {
		Instruction result;
		result.type = EnumType.ACTIVATENEURON;
		result.neuronIndex = neuronIndex;
		return result;
	}

	static Instruction makeDeactivateNeuron(uint neuronIndex) {
		Instruction result;
		result.type = EnumType.DEACTIVATENEURON;
		result.neuronIndex = neuronIndex;
		return result;
	}

	static Instruction makeActivateNeuronAtStackTop() {
		Instruction result;
		result.type = EnumType.ACTIVATENEURONATSTACKTOP;
		return result;
	}

	static Instruction makeDeactivateNeuronAtStackTop() {
		Instruction result;
		result.type = EnumType.DEACTIVATENEURONATSTACKTOP;
		return result;
	}

	static Instruction makeCommit() {
		Instruction result;
		result.type = EnumType.COMMIT;
		return result;
	}

	static Instruction makeDuplicateStackTop() {
		Instruction result;
		result.type = EnumType.DUPLICATESTACKTOP;
		return result;
	}

	static Instruction makePushConstant(uint value) {
		Instruction result;
		result.type = EnumType.PUSHCONSTANT;
		result.valueToPush = value;
		return result;
	}

	static Instruction makePop() {
		Instruction result;
		result.type = EnumType.POP;
		return result;
	}

	static Instruction makePop2() {
		Instruction result;
		result.type = EnumType.POP2;
		return result;
	}

	static Instruction makePushNextNeuronIndex() {
		Instruction result;
		result.type = EnumType.PUSHNEXTNEURONINDEX;
		return result;
	}

	static Instruction makeSwapStack() {
		Instruction result;
		result.type = EnumType.SWAPSTACK;
		return result;
	}

	static Instruction makeNop() {
		Instruction result;
		result.type = EnumType.NOP;
		return result;
	}

	static Instruction makeResetNeuronActivationForNeuronAtTop() {
		Instruction result;
		result.type = EnumType.RESETNEURONACTIVATIONFORNEURONATTOP;
		return result;
	}

	static Instruction makeSetOutputStrengthForNeuronAtStackTop(float strength) {
		Instruction result;
		result.type = EnumType.SETOUTPUTHSTRENGTHTOVALUEFORNEURONATSTACKTOP;
		result.strength = strength;
		return result;
	}

	static Instruction makeSetTypeVariableForPieceAtStackTop(uint typeVariable) {
		Instruction result;
		result.type = EnumType.SETTYPEVARIABLEFORNEURONATSTACKTOP;
		result.typeVariable = typeVariable;
		return result;
	}


	static Instruction makeSetSwitchboardIndexForOutputForPieceAtStackTop(uint outputIndex) {
		Instruction result;
		result.type = EnumType.SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP;
		result.outputIndex = outputIndex;
		return result;
	}

	// if the # of inputs is higher or equal to inputIndex then the instruction adds the required # of inputs and rewires all added inputs to the defaultInput
	static Instruction makeSetInputThresholdForPieceAtStackTop(uint inputIndex) {
		Instruction result;
		result.type = EnumType.SETINPUTTHRESHOLDFORPIECEATSTACKTOP;
		result.inputIndex = inputIndex;
		return result;
	}


	// if the # of inputs is higher or equal to inputIndex then the instruction adds the required # of inputs and rewires all added inputs to the defaultInput
	static Instruction makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(uint inputIndex, uint switchboardIndex) {
		Instruction result;
		result.type = EnumType.SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP;
		result.inputIndex = inputIndex;
		result.switchboardIndex = switchboardIndex;
		return result;
	}

	// links the output of the neuron at stack(top) to the output slot of the SLIM-RNN to the outputIndex
	static Instruction makeLinkOutputOfNeuronToSlimRnnOutputForNeuronAtStackTop(uint outputIndex) {
		Instruction result;
		result.type = EnumType.LINKOUTPUTOFNEURONTOSLIMRNNOUTPUTFORNEURONATSTACKTOP;
		result.outputIndex = outputIndex;
		return result;
	}










	// if the # of inputs is higher or equal to inputIndex then the instruction adds the required # of inputs and rewires all added inputs to the defaultInput
	static Instruction makeSetThresholdForInputIndexForPieceAtStackTop(uint inputIndex, float threshold) {
		Instruction result;
		result.type = EnumType.SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP;
		result.inputIndex = inputIndex;
		result.threshold = threshold;
		return result;
	}

	static Instruction makeResetNeuronActivation() {
		Instruction result;
		result.type = EnumType.RESETNEURONACTIVATION;
		return result;
	}


	// TODO< overhaul >
	final @property string humanReadableDescription() {
		import std.format;

		with(EnumType) {
			if( type == ACTIVATENEURON ) {
				return "activateNeuron neuronIndex=%s".format(neuronIndex);
			}
			else if( type == DEACTIVATENEURON ) {
				return "deactivateNeuron neuronIndex=%s".format(neuronIndex);
			}
			else if( type == ACTIVATENEURONATSTACKTOP ) {
				return "activateNeuronAtStackTop";
			}
			else if( type == DEACTIVATENEURONATSTACKTOP ) {
				return "deactivateNeuronAtStackTop";
			}
			else if( type == COMMIT ) {
				return "commit";
			}
			else if( type == DUPLICATESTACKTOP ) {
				return "duplicateStackTop";
			}
			else if( type == POP ) {
				return "pop";
			}
			else if( type == POP2 ) {
				return "pop2";
			}
			else if( type == PUSHNEXTNEURONINDEX ) {
				return "pushNextNeuronIndex";
			}
			else if( type == PUSHCONSTANT ) {
				return "pushValue valueToPush=%s".format(valueToPush);
			}
			else if( type == SWAPSTACK ) {
				return "swapStack";
			}
			else if( type == NOP ) {
				return "nop";
			}
			else if( type == RESETNEURONACTIVATIONFORNEURONATTOP ) {
				return "resetNeuronActivationForNeuronAtTop";
			}
			else if( type == SETOUTPUTHSTRENGTHTOVALUEFORNEURONATSTACKTOP ) {
				return "SetOutputStrengthForNeuronAtStackTop strength=%s".format(strength);
			}
			else if( type == SETTYPEVARIABLEFORNEURONATSTACKTOP ) {
				return "SetTypeVariableForNeuronAtStackTop typeVariable=%s".format(typeVariable);
			}
			else if( type == SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP ) {
				return "SetSwitchboardIndexForOutputForPieceAtStackTop outputIndex=%s".format(outputIndex);
			}
			else if( type == SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP ) {
				return "SetSwitchboardIndexForInputIndexAndPieceAtStackTop inputIndex=%s switchboardIndex=%s".format(inputIndex, switchboardIndex);
			}
			else if( type == LINKOUTPUTOFNEURONTOSLIMRNNOUTPUTFORNEURONATSTACKTOP ) {
				return "LinkOutputOfNeuronToSlimRnnOutputForNeuronAtStackTop outputIndex=%s".format(outputIndex);
			}
			else if( type == SETINPUTTHRESHOLDFORPIECEATSTACKTOP ) {
				return "SetInputThresholdForPieceAtStackTop inputIndex=%s".format(inputIndex);
			}
			else if( type == SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP ) {
				return "SetThresholdForInputIndexForPieceAtStackTop inputIndex=%s threshold=%s".format(inputIndex, threshold);
			}
			else if( type == RESETNEURONACTIVATION ) {
				return "ResetNeuronActivation";
			}
			else {
				return "<unknown>";
			}
		}
	}
}
