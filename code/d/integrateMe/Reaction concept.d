// slots -> reaction

struct ReactionSlot {
	size_t checkIndex;

	enum EnumReactionType {
		ACTIVATION // element gets activated if check is successful
	}

	EnumReactionType reactionType;
	bool wasActivated;
}

struct Check {
	enum INPUTSSIZE = 2;

	enum EnumType {
		GREATER,
		SMALLER,
	}

	EnumType type;
	size_t[INPUTSSIZE] inputs; 
}

struct Element {

}

struct Global {
	Check[] checks;
	ReactionSlot[] reactionSlots;
	Element[] elements;
	float[] values;

	final float getValue(size_t index) {
		return values[index];
	}

	final bool compare(ref Check check) {
		final switch(check.type) with (Check.EnumType) {
			case GREATER:
			return getValue(check.inputs[0]) > getValue(check.inputs[1]);
			case SMALLER:
			return getValue(check.inputs[0]) < getValue(check.inputs[1]);
		}
	}

	final void calcReactions() {
		foreach(ref ReactionSlot iterationReactionSlot; reactionSlots ) {
			calcReaction(iterationReactionSlot);
		}
	}

	private final void calcReaction(ref ReactionSlot reactionSlot) {
		bool compareResult = compare(checks[reactionSlot.checkIndex]);
		reactionSlot.wasActivated = compareResult;
	}
}

