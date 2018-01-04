import Pattern;
import Match;

struct Decoration {
	enum EnumType {
		VARIABLETEMPLATEMATCHING, // list of templates which can be matched against variables
		SEQUENCE, // sequences can be interpreted linearly
		TUPLE2, // used for matching, pair of template and code (sequence) which will be executed if it matches
	}

	EnumType type;
}

alias Pattern!Decoration PatternWithDecoration;

struct InterpretationResult {
	enum EnumResultType {
		OK,
		INTERRUPTED, // interpretation was interrupted because of some mismatch, is not a fault
		ERRORVARIABLENOTFOUND, // a variable was not found while matching, hard error
	}

	EnumResultType resultType;

	final @property isInterruptedOrError() {
		return resultType != EnumResultType.OK;
	}
}

struct InterpretationContext {
	PatternWithDecoration*[UniqueIdType] valueByVariable;


}

private void interpretSequence(PatternWithDecoration *patternWithDecoration, InterpretationContext *context, InterpretationResult *iterpretationResult) {
	assert(patternWithDecoration.decoration.type == Decoration.EnumType.SEQUENCE);
	assert(patternWithDecoration.isBranch); // TODO< enforce >

	foreach( i; 0..patternWithDecoration.referenced.length ) {
		interpretationDispatch(patternWithDecoration.referenced[i], context, interpretationResult);
		if( interpretationResult.isInterruptedOrError ) {
			return;
		}
	}
}

private void interpretVariableTemplateMatching(PatternWithDecoration *patternWithDecoration, InterpretationContext *context, InterpretationResult *interpretationResult) {
	assert(patternWithDecoration.decoration.type == Decoration.EnumType.VARIABLETEMPLATEMATCHING);
	assert(patternWithDecoration.isBranch); // TODO< enforce >

	PatternWithDecoration *patterToMatch = patternWithDecoration.referenced[0];

	foreach( i; 1..patternWithDecoration.referenced.length ) {
		assert(patternWithDecoration.referenced[i].decoration.type == Decoration.EnumType.TUPLE2); // TODO< enforce >
		assert(patternWithDecoration.referenced[i].type == Decoration.EnumType.BRANCH);
		
		PatternWithDecoration *templatePattern = patternWithDecoration.referenced[i].referenced[0];
		PatternWithDecoration *actionPattern = patternWithDecoration.referenced[i].referenced[1];

		MatchArguments arguments;
	

		PatternWithDecoration*[UniqueIdType] matches = context.valueByVariable.dup;// let it know the current variable assignments
		bool isSame;

		arguments.bidirectional = false;
		arguments.templatePattern = templatePattern;
		arguments.matchingPattern = patterToMatch;
		bool isMatching = match(arguments, matches, isSame);

		if( !isMatching ) {
			continue;
		}
		// we are here if it does match

		// excute body
		interpretationDispatch(actionPattern, context, interpretationResult);
		return; // we return because we don't need to match the other possibilities
	}
}

void interpretationDispatch(PatternWithDecoration *patternWithDecoration, InterpretationContext *context, InterpretationResult *iterpretationResult) {
	if( patternWithDecoration.decoration.type == Decoration.EnumType.SEQUENCE ) {
		interpretSequence(patternWithDecoration, context, /*out*/interpretationInterrupted);
	}
	else if( patternWithDecoration.decoration.type == Decoration.EnumType.VARIABLETEMPLATEMATCHING ) {
		interpretVariableTemplateMatching(patternWithDecoration, context, interpretationResult);
	}
	// TODO< template matching >
}

/+
// example for matching
@match{
	##0

	@tuple2{
		{a} @seq{
			printA;
		}
	}

	@tuple2{
		{b} @seq{
			printB;
		}
	}
}
+/