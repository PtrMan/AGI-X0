// an ExtendedNeuroid has an input for reinforcement and a way to send reinforement information to its children

struct ExtendedNeuroid {
	float inReinforcement; // gets summed over the input
	float outReinforcement;

	E[4] e;
}

// action slot, get's only activated if the neuroid gets actived
struct E {
	float relativePropability; // can be bigger or lower than 1

	enum EnumActionType {
		NOACTION, // no action, no operation
		FIRE, // fire the neuroid
	}

	EnumActionType actionType;

	//@property final float limitedPropability() {
	//	return limit(propability, 0.0f, 1.0f);
	//}
}

struct InterpretationContext {
	float register;
	int ip; // instruction pointer
}

struct TapeElement {
	static TapeElement make(EnumType type, int idx = 0) {
		TapeElement result;
		result.type = type;
		result.idx = idx;
		result.factor = 1.0f;
		return result;
	}

	enum EnumType {
		GOTOIDX, // go to idx in the tape
		READREINFORCEMENT, // read reinforcement into register by amount factor, this allows to split the reinforement into "junks"
		ADDPROPABILITYSLOT, // adds the register to the propability for "e" at idx
		SENDREINFOREMENT,
	}

	EnumType type;
	int idx; // helper variable
	float factor;
}

void interpretLoop(ref ExtendedNeuroid n, ref InterpretationContext interpretationContext, TapeElement[] tapeElements, uint rounds) {
	interpretationContext.ip = 0;

	foreach( i; 0..rounds ) {
		interpret(n, interpretationContext, tapeElements);
	}
}

void interpret(ref ExtendedNeuroid n, ref InterpretationContext interpretationContext, TapeElement[] tapeElements) {
	TapeElement tapeElement = tapeElements[interpretationContext.ip];

	interpretationContext.ip++;

	final switch(tapeElement.type) with (TapeElement.EnumType) {
		case GOTOIDX:
		interpretationContext.ip = cast(int)limit(tapeElement.idx, 0, tapeElements.length-1);
		break;

		case READREINFORCEMENT:
		interpretationContext.register = n.inReinforcement * tapeElement.factor;
		n.inReinforcement = n.inReinforcement - interpretationContext.register;
		break;

		case ADDPROPABILITYSLOT:
		if( tapeElement.idx < 0 || tapeElement.idx >= n.e.length ) {
			return;
		}
		n.e[tapeElement.idx].relativePropability += interpretationContext.register;
		break;

		case SENDREINFOREMENT:
		n.outReinforcement = interpretationContext.register;
		break;
	}
}

import std.algorithm.comparison : min, max;

private Type limit(Type)(Type v, Type min, Type max) {
	return .max(.min(v, max), min);
}



