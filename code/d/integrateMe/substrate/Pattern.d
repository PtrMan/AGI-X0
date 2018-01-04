module Pattern;

import std.stdint;

struct Pattern(DecorationType) {
	DecorationType decoration;

	enum EnumType {
		SYMBOL,
		VARIABLE,

		BRANCH, // a branch in the tree
	}

	EnumType type;

	union {
		SymbolType symbol;
		Pattern!DecorationType*[] referenced;
	}

	final @property bool isBranch() pure {
		return type == EnumType.BRANCH;
	}
  
	final bool is_(EnumType other) pure {
		return this.type == other;
	}

	/* uncommented because not needed
	final @property bool isLeaf() pure {
		return !isBranch;
	}
	*/

	// used for fast matching
	// VARIABLE must have the same variableId
	final UniqueIdType uniqueId() pure {
		return privateUniqueId;
	}

	@property final uint64_t variableId() pure {
		return privateUniqueId; // the variableId is equal to the unique id
	}

	private UniqueIdType privateUniqueId;
	                       

	static Pattern makeSymbol(SymbolType symbol, UniqueIdType uniqueId) {
		Pattern result;
		result.type = EnumType.SYMBOL;
		result.symbol = symbol;
		result.privateUniqueId = uniqueId;
		return result;
	}

	static Pattern makeVariable(UniqueIdType uniqueId) {
		Pattern result;
		result.type = EnumType.VARIABLE;
		result.privateUniqueId = uniqueId;
		return result;
	}

	static Pattern makeBranch(Pattern*[] children, UniqueIdType uniqueId) {
		Pattern result;
		result.type = EnumType.BRANCH;
		result.referenced = children;
		result.privateUniqueId = uniqueId;
		return result;
	}
}

// helper
// creates a new OOP-object and copies the value into it
Pattern!DecorationType *newPattern(DecorationType)(Pattern!DecorationType value) {
	Pattern!DecorationType *result = new Pattern!DecorationType;
	*result = value;
	return result;
}


alias void* SymbolType;
alias uint64_t UniqueIdType; // TODO< make typedef

// defines the sematic for the symbol comparision
bool isSameSymbol(SymbolType a, SymbolType b) {
	return a is b;
}





/* outcommented because it's just an inspiration from replicode
		// type of the object
	enum EnumObjectType {
		SYS, // system object, see [ikonFlux2] page 7
		INS, // instance, see [ikonFlux2] page 7
		PGM, // program, product rule, see [ikonFlux2] page 7, 8
		//ERW  // equation rewrite rule, with the expressivness of first order logic, see [ikonflux2] page 8
		// EVA, RED - forward chaining see [ikonflux2] page 8
		// INV, CTX - backward chaining see [ikonflux2] page 8
		// backward chaining is for support for planning, but the executive is not a planning system itself,
		// see [ikonflux2] page 8

		// SPV : dedicated instance of the executive, see [ikonflux2] page 9
		// DEV : external sub-systems, see [ikonflux2] page 9

		// RLM : realm, used for measuring progress/utility to a goal, ses [ikonflux2] page 9

	}

*/


