module slimRnn.SlimRnnStackBasedManipulationInstruction;

private alias SlimRnnStackBasedManipulationInstruction Instruction;

// stack based instruction to manipulate the slim RNN
struct SlimRnnStackBasedManipulationInstruction {
	enum EnumType {
		PUSHVALUE,
		POP,
		//SETNEURONINDEXTOTOPACTIVATENEURONSETTYPEOFNEURON,
		SETINPUTTHRESHOLDFORPIECEATSTACKTOP, // set threshold of input index to #top
		SETOUPUTSTRENGTHFORPIECEATSTACKTOP,
		SETACTIVATEFORPIECEACTIVATIONVARIABLEATSTACKTOP,
		SETTYPEVARIABLEFORPIECEATSTACKTOP,
		SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP,
		RESETNEURONACTIVATION,
		SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP,
		SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP,
	}

	EnumType type;

	// optional
	union {
		uint typeVariable, inputIndex, valueToPush, functionType, outputIndex;
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
	

	static Instruction makePush(uint value) {
		Instruction result;
		result.type = EnumType.PUSHVALUE;
		result.valueToPush = value;
		return result;
	}

	static Instruction makePop() {
		Instruction result;
		result.type = EnumType.POP;
		return result;
	}

	/*
	static Instruction makeSetNeuronIndexToTopActivateNeuronSetTypeOfNeuron(bool activateFlagActive, bool activateFlag, Piece.EnumType setNeuronTypeToType) {
		Instruction result;
		result.type = EnumType.SETNEURONINDEXTOTOPACTIVATENEURONSETTYPEOFNEURON;
		result.activateFlagActive = activateFlagActive;
		result.activateFlag = activateFlag;
		result.setNeuronTypeToType = setNeuronTypeToType;
		return result;
	}
	*/

	// if the # of inputs is higher or equal to inputIndex then the instruction adds the required # of inputs and rewires all added inputs to the defaultInput
	static Instruction makeSetInputThresholdForPieceAtStackTop(uint inputIndex) {
		Instruction result;
		result.type = EnumType.SETINPUTTHRESHOLDFORPIECEATSTACKTOP;
		result.inputIndex = inputIndex;
		return result;
	}

	static Instruction makeSetOutputStrengthForPieceAtStackTop(float strength) {
		Instruction result;
		result.type = EnumType.SETOUPUTSTRENGTHFORPIECEATSTACKTOP;
		result.strength = strength;
		return result;
	}

	// sets the activation variable to activateFlag for piece/neuron at stack(top)
	static Instruction makeSetActivateForPieceActivationVariableAtStackTop(bool activateFlag) {
		Instruction result;
		result.type = EnumType.SETACTIVATEFORPIECEACTIVATIONVARIABLEATSTACKTOP;
		result.activateFlag = activateFlag;
		return result;
	}

	static Instruction makeSetTypeVariableForPieceAtStackTop(uint typeVariable) {
		Instruction result;
		result.type = EnumType.SETTYPEVARIABLEFORPIECEATSTACKTOP;
		result.typeVariable = typeVariable;
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

	// if the # of inputs is higher or equal to inputIndex then the instruction adds the required # of inputs and rewires all added inputs to the defaultInput
	static Instruction makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(uint inputIndex, uint switchboardIndex) {
		Instruction result;
		result.type = EnumType.SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP;
		result.inputIndex = inputIndex;
		result.switchboardIndex = switchboardIndex;
		return result;
	}

	static Instruction makeSetSwitchboardIndexForOutputForPieceAtStackTop(uint outputIndex) {
		Instruction result;
		result.type = EnumType.SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP;
		result.outputIndex = outputIndex;
		return result;
	}


	final @property string humanReadableDescription() {
		import std.format;

		with(EnumType) {
			if( type == PUSHVALUE ) {
				return "PushValue valueToPush=%s".format(valueToPush);
			}
			else if( type == POP ) {
				return "Pop";
			}
			else if( type == SETINPUTTHRESHOLDFORPIECEATSTACKTOP ) {
				return "SetInputThresholdForPieceAtStackTop inputIndex=%s".format(inputIndex);
			}
			else if( type == SETOUPUTSTRENGTHFORPIECEATSTACKTOP ) {
				return "SetOutputStrengthForPieceAtStackTop strength=%s".format(strength);
			}
			else if( type == SETACTIVATEFORPIECEACTIVATIONVARIABLEATSTACKTOP ) {
				return "SetActivateForPieceActivationVariableAtStackTop activateFlag=%s".format(activateFlag);
			}
			else if( type == SETTYPEVARIABLEFORPIECEATSTACKTOP ) {
				return "SetTypeVariableForPieceAtStackTop typeVariable=%s".format(typeVariable);
			}
			else if( type == SETTHRESHOLDFORINPUTINDEXFORPIECEATSTACKTOP ) {
				return "SetThresholdForInputIndexForPieceAtStackTop inputIndex=%s threshold=%s".format(inputIndex, threshold);
			}
			else if( type == RESETNEURONACTIVATION ) {
				return "ResetNeuronActivation";
			}
			else if( type == SETSWITCHBOARDINDEXFORINPUTINDEXANDPIECEATSTACKTOP ) {
				return "SetSwitchboardIndexForInputIndexAndPieceAtStackTop inputIndex=%s switchboardIndex=%s".format(inputIndex, switchboardIndex);
			}
			else if( type == SETSWITCHBOARDINDEXFOROUTPUTFORPIECEATSTACKTOP ) {
				return "SetSwitchboardIndexForOutputForPieceAtStackTop outputIndex=%s".format(outputIndex);
			}
			else {
				return "<unknown>";
			}
		}
	}
}
