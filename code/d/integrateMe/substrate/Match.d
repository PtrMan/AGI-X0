import std.stdint;

import Pattern : Pattern, newPattern,      isSameSymbol, UniqueIdType, SymbolType;

// prototype of matching functionality for self referential AI

struct MatchArguments {
	Pattern *templatePattern;
	Pattern *matchingPattern;
	bool bidirectional;
}

// tries to match the 'matchingPattern' to the 'templatePattern'
// 'matches' doesn't get reseted on entry, this simplifies iterated(recursive) matching
bool match(MatchArguments arguments, ref Pattern*[UniqueIdType] matches, out bool isSame) {
	bool innerFnMatchVariable(Pattern *variable, Pattern *toMatch) {
		// it is possible that the variable is already in the dictionary
		// if this is the case compare the uniqueId to identify a missmatch

		if( variable.uniqueId in matches &&
			variable !is matches[variable.uniqueId]
		) {
			return false;
		}

		matches[variable.uniqueId] = toMatch;

		return true;
	}

	isSame = arguments.templatePattern.uniqueId == arguments.matchingPattern.uniqueId; // OPTIMIZATION
	with(Pattern.EnumType) {
		if( isSame ) {
			return true;
		}
		else if( isSame && arguments.templatePattern.is_(VARIABLE) && arguments.matchingPattern.is_(VARIABLE) ) { // OPTIMIZATION< we can do this because uniqueId == variableId
			assert(arguments.templatePattern.variableId == arguments.matchingPattern.variableId); // verify
			return true;
		}
	}

	with(Pattern.EnumType) {
		if(
			arguments.templatePattern.is_(SYMBOL) &&
			arguments.matchingPattern.is_(SYMBOL)
		) {
			return isSameSymbol(arguments.templatePattern.symbol, arguments.matchingPattern.symbol);
		}
		else if( arguments.templatePattern.is_(VARIABLE) ) {
			return innerFnMatchVariable(arguments.templatePattern, arguments.matchingPattern);
		}
		// we match in the other direction too if the flag is set
		else if( arguments.bidirectional && arguments.matchingPattern.is_(VARIABLE) ) {
			return innerFnMatchVariable(arguments.matchingPattern, arguments.templatePattern);
		}
		else if( arguments.templatePattern.isBranch() && arguments.matchingPattern.isBranch() ) { // recursivly match
			if( arguments.templatePattern.referenced.length != arguments.matchingPattern.referenced.length ) { // OPTIMIZATION
				return false;
			}

			size_t numberOfChildren = arguments.templatePattern.referenced.length;
			foreach( i; 0..numberOfChildren ) {
				bool calledIsSame;
				MatchArguments calledArguments;
				calledArguments.templatePattern = arguments.templatePattern.referenced[i];
				calledArguments.matchingPattern = arguments.matchingPattern.referenced[i];
				calledArguments.bidirectional = arguments.bidirectional;

				if( !match(calledArguments, matches, calledIsSame) ) {
					return false;
				}
			}

			return true;
		}
		else {
			// if all possibilities failed we return a missmatch
			return false;
		}
	}
}

unittest { // test symbol comparision
	MatchArguments arguments;
	arguments.bidirectional = true;

	Pattern*[UniqueIdType] matches;
	bool isSame;

	// same unique ids
	arguments.templatePattern = newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0));
	arguments.matchingPattern = newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0));
	assert(match(arguments, matches, isSame));
	assert(isSame);

	// different uniqueIds, same symbol
	arguments.templatePattern = newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0));
	arguments.matchingPattern = newPattern(Pattern.makeSymbol(cast(SymbolType)1, 1));
	assert(match(arguments, matches, isSame));
	assert(!isSame);

	// different symbol
	arguments.templatePattern = newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0));
	arguments.matchingPattern = newPattern(Pattern.makeSymbol(cast(SymbolType)2, 1));
	assert(!match(arguments, matches, isSame));
	assert(!isSame);
}

unittest { // test primitive variable matching
	MatchArguments arguments;
	

	Pattern*[UniqueIdType] matches;
	bool isSame;

	// not bidirectional
	matches.clear;
	arguments.bidirectional = false;
	arguments.templatePattern = newPattern(Pattern.makeVariable(0));
	arguments.matchingPattern = newPattern(Pattern.makeSymbol(cast(SymbolType)3, 1));
	assert(match(arguments, matches, isSame));
	assert(!isSame);
	assert(0 in matches);
	assert(matches[0] is arguments.matchingPattern);

	matches.clear;
	arguments.bidirectional = false;
	arguments.templatePattern = newPattern(Pattern.makeSymbol(cast(SymbolType)3, 1));
	arguments.matchingPattern = newPattern(Pattern.makeVariable(0));
	assert(!match(arguments, matches, isSame));

	// bidirectional
	matches.clear;
	arguments.bidirectional = true;
	arguments.templatePattern = newPattern(Pattern.makeSymbol(cast(SymbolType)3, 1));
	arguments.matchingPattern = newPattern(Pattern.makeVariable(0));
	assert(match(arguments, matches, isSame));
	assert(!isSame);
	assert(0 in matches);
	assert(matches[0] is arguments.templatePattern);
}

unittest { // recursive comparision
	MatchArguments arguments;

	Pattern*[UniqueIdType] matches;
	bool isSame;

	// not bidirectional
	matches.clear;

	// different uniqueIds, same symbol
	arguments.templatePattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0)),  newPattern(Pattern.makeSymbol(cast(SymbolType)11, 10))   ], 20));
	arguments.matchingPattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)1, 1)),  newPattern(Pattern.makeSymbol(cast(SymbolType)11, 11))   ], 21));
	assert(match(arguments, matches, isSame));
	assert(!isSame);

	// different symbol
	arguments.templatePattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0)),  newPattern(Pattern.makeSymbol(cast(SymbolType)11, 10))   ], 20));
	arguments.matchingPattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)2, 1)),  newPattern(Pattern.makeSymbol(cast(SymbolType)11, 11))   ], 21));
	assert(!match(arguments, matches, isSame));
	assert(!isSame);

	arguments.templatePattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)1, 0)),  newPattern(Pattern.makeSymbol(cast(SymbolType)11, 10))   ], 20));
	arguments.matchingPattern = newPattern(Pattern.makeBranch([   newPattern(Pattern.makeSymbol(cast(SymbolType)1, 1)),  newPattern(Pattern.makeSymbol(cast(SymbolType)12, 11))   ], 21));
	assert(!match(arguments, matches, isSame));
	assert(!isSame);
}

unittest { // recursive variable matching
	// TODO
}

