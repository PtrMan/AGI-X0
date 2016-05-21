module TokenOperators;

// TODO< wildcard for the comperator, which is -2 >

import std.algorithm : min;

import CgpException : CgpException;
import ValueMatrix : ValueMatrix;

import Permutation : Permutation;

class TextIndexOrTupleValue {
	public enum EnumType {
		NOTSET,
		TOKEN,
		TUPLE
	}

	protected EnumType type;
	public uint tokenIndex;

	public uint[] tuple;

	protected final this(EnumType type) {
		this.type = type;
	}

	public static TextIndexOrTupleValue makeDefaultValue() {
		return new TextIndexOrTupleValue(EnumType.NOTSET);
	}

	public static TextIndexOrTupleValue makeToken(uint tokenIndex) {
		TextIndexOrTupleValue result = new TextIndexOrTupleValue(EnumType.NOTSET);
		result.tokenIndex = tokenIndex;
		return result;
	}

	public static TextIndexOrTupleValue makeTuple(uint[] tuple) {
		TextIndexOrTupleValue result = new TextIndexOrTupleValue(EnumType.TUPLE);
		result.tuple = tuple;
		return result;
	}

	public final @property isSet() {
		return type != EnumType.NOTSET;
	}

	public final @property isToken() {
		return type == EnumType.TOKEN;
	}

	public final @property isTuple() {
		return type == EnumType.TUPLE;
	}

}






interface IOperatorInstancePrototype(ValueType) {
	// typeId : which type of operator instance should be created?
	// TODO< describe this with an descriptor object which contains the configuration >
	IOperatorInstance!ValueType createInstance(uint typeId);
}

class TokenMatcherOperatorInstancePrototype : IOperatorInstancePrototype!TextIndexOrTupleValue {
	public final this(
		uint matcherReadWidth, uint matcherNumberOfTokens,
		uint matcherNumberOfComperators, uint matcherNumberOfVariants,
		Permutation[] matcherPermutations,

		uint selectorNumberOfInputConnections
	) {
		this.matcherReadWidth = matcherReadWidth;
		this.matcherNumberOfTokens = matcherNumberOfTokens;
		this.matcherPermutations = matcherPermutations;

		this.matcherNumberOfComperators = matcherNumberOfComperators;
		this.matcherNumberOfVariants = matcherNumberOfVariants;

		this.selectorNumberOfInputConnections = selectorNumberOfInputConnections;

	}

	public final IOperatorInstance!TextIndexOrTupleValue createInstance(uint typeId) {
		if( typeId == 0 ) {
			return new TokenMatcherOperatorInstance(matcherReadWidth, matcherNumberOfTokens,  matcherNumberOfComperators, matcherNumberOfVariants,  matcherPermutations);
		}
		else if( typeId == 1 ) {
			return new SelectorOperatorInstance(selectorNumberOfInputConnections);
		}
		else {
			throw new CgpException("Internal error: typeId is not recognized!");
		}
	}

	protected uint
		matcherReadWidth, matcherNumberOfTokens,
		matcherNumberOfComperators, matcherNumberOfVariants;
	protected Permutation[] matcherPermutations;

	protected uint
		selectorNumberOfInputConnections;
}



interface IOperatorInstance(ValueType) {
	// returns number of gene elements which need to be sliced for one node
	uint getGeneSliceWidth();

	void decodeSlicedGene(uint[] slicedGene);
	
	uint getNumberOfInputConnections();
	
	// numberOfOperatorsBeforeColumn : each operator in every column can just select the operators before this column as inputs, so we need to know how many operator there are to choose from
	// returns the index (where the range is defined by the inputs followed by the ouputs of the operators) for the connection
	uint getInputIndexForConnection(uint connectionIndex, uint numberOfOperatorsBeforeColumn);

	ValueType calculateResult(ValueType[] inputs)
	in {
		assert(inputs.length == getNumberOfInputConnections());
	}

	string getDebugMathematica(uint numberOfOperatorsBeforeColumn);
}

class SelectorOperatorInstance : IOperatorInstance!TextIndexOrTupleValue {
	public final this(uint numberOfInputConnections) {
		this.numberOfInputConnections = numberOfInputConnections;
	}

	public final uint getGeneSliceWidth() {
		return numberOfInputConnections;
	}

	// [the selectors for the connections] followed by [the selector for the input]
	public final void decodeSlicedGene(uint[] slicedGene) {
		slicedGeneForConnections = slicedGene[0..numberOfInputConnections];
		//currentSelector = slicedGene[numberOfInputConnections] % numberOfInputConnections;
	}
	
	public final uint getNumberOfInputConnections() {
		return numberOfInputConnections;
	}

	public final uint getInputIndexForConnection(uint connectionIndex, uint numberOfOperatorsBeforeColumn) {
		assert(numberOfInputConnections > 0);
		assert(slicedGeneForConnections.length == numberOfInputConnections);
		return slicedGeneForConnections[connectionIndex] % numberOfOperatorsBeforeColumn;
	}

	public final TextIndexOrTupleValue calculateResult(TextIndexOrTupleValue[] inputs) {
		assert(inputs.length == numberOfInputConnections);

		// search the first result which is set and return it
		foreach( TextIndexOrTupleValue iterationInput; inputs ) {
			if( iterationInput.isSet ) {
				return iterationInput;
			}
		}

		return TextIndexOrTupleValue.makeDefaultValue();
	}

	public final string getDebugMathematica(uint numberOfOperatorsBeforeColumn) {
		string result;

		foreach( i; 0..getNumberOfInputConnections() ) {
			import std.format : format;
			result ~= format("%d ", slicedGeneForConnections[i] % numberOfOperatorsBeforeColumn);
		}

		return result;
	}

	protected uint[] slicedGeneForConnections;
	//protected uint currentSelector;

	protected uint numberOfInputConnections; //, numberOfOperatorsToChoose;
}

class TokenMatcherOperatorInstance : IOperatorInstance!TextIndexOrTupleValue {
	public final this(uint readWidth, uint numberOfTokens,  uint parameterNumberOfComperators, uint parameterNumberOfVariants,  Permutation[] permutations) {
		this.settingReadWidth = readWidth;
		this.settingNumberOfTokens = numberOfTokens;
		this.settingPermutations = permutations;

		this.geneTokenComperatorMatrix = new ValueMatrix!int(parameterNumberOfComperators, parameterNumberOfVariants);
		disableAllComperators();
	}

	// configuration
	protected uint settingReadWidth;
	protected uint settingNumberOfTokens;
	protected Permutation[] settingPermutations;

	protected uint genePermutationIndex;
	protected ValueMatrix!int geneTokenComperatorMatrix;

	protected final @property uint numberOfComperators() {
		return geneTokenComperatorMatrix.width;
	}

	protected final @property uint numberOfVariants() {
		return geneTokenComperatorMatrix.height;
	}

	protected final bool isTokenComperatorActivated(uint portIndex, uint variantIndex) {
		return geneTokenComperatorMatrix[variantIndex, portIndex] != -1;
	}

	protected final int getTokenComperator(uint portIndex, uint variantIndex) {
		return geneTokenComperatorMatrix[variantIndex, portIndex];
	}

	// for unittesting
	public final void setTokenComperator(uint portIndex, uint variantIndex, int value) {
		geneTokenComperatorMatrix[variantIndex, portIndex] = value;
	}

	protected final void disableAllComperators() {
		foreach( variantIndex; 0..numberOfVariants ) {
			foreach( comperatorIndex; 0..numberOfComperators ) {
				geneTokenComperatorMatrix[variantIndex, comperatorIndex] = -1;
			}
		}
	}



	public final void decodeSlicedGene(uint[] slicedGene) {
		uint runningI = 0;

		uint pullValueAndIncrementIndex() {
			uint result = slicedGene[runningI];
			runningI++;
			return result;
		}

		int pullIntValueAndIncrementIndex() {
			uint valueAsUint = pullValueAndIncrementIndex();
			uint valueMasked = valueAsUint & (uint.max >> 1); // we do this to not screw with th sign when we do the cast
			return cast(int)valueMasked;
		}

		genePermutationIndex = pullValueAndIncrementIndex() % settingPermutations.length;
		
		foreach( variantIndex; 0..numberOfVariants ) {
			foreach( comperatorIndex; 0..numberOfComperators ) {
				geneTokenComperatorMatrix[variantIndex, comperatorIndex] = (pullIntValueAndIncrementIndex() % (settingNumberOfTokens+1)) - 1;
			}
		}
		

		assert(runningI == getGeneSliceWidth());
	}

	public final uint getGeneSliceWidth() {
		return 
			1 + // one gene for the selector of the permutation index
			geneTokenComperatorMatrix.width*geneTokenComperatorMatrix.height;
	}

	public final uint getNumberOfInputConnections() {
		return 1;
	}

	public final uint getInputIndexForConnection(uint connectionIndex, uint numberOfOperatorsBeforeColumn) {
		return 0; // always the input (just for testing)
	}

	public final TextIndexOrTupleValue calculateResult(TextIndexOrTupleValue[] inputs) {
		assert(inputs[0].tuple.length != 0);

		// -1 means invalid
		int[] portA;
		portA.length = numberOfComperators;
		foreach( i; 0..portA.length ) {
			portA[i] = -1;
		}

		// read to port A
		portA[0..min(numberOfComperators, $)] = cast(int[])inputs[0].tuple[0..min(numberOfComperators, $)];


		uint[] portB;
		portB.length = portA.length;

		bool portBActivated = true;
		uint portBActivationCounter = 0, portBEnabledPortCounter = 0;

		bool match = false;
		
		// select from portA into portB
		// if there is a mismatch of an activated possibility portB gets disabled
		{
			foreach( portIndex; 0..numberOfComperators ) {
				void doesPortMatch(out bool portFired, out uint result) {
					portFired = false;
					bool portActivated = false;
					
					foreach( variantI; 0..numberOfVariants ) {
						bool variantFired = false;

						if( !isTokenComperatorActivated(portIndex, variantI) ) {
							continue;
						}

						portActivated = true;

						bool isPortSet = portA[portIndex] != -1;
						if( !isPortSet ) {
							continue;
						}


						if( getTokenComperator(portIndex, variantI) == portA[portIndex] ) {
							variantFired = true;
							result = portA[portIndex];
							//match = true;
						}

						portFired |= variantFired;
					}

					if( portActivated ) {
						portBEnabledPortCounter++;
					}
				}
				
				bool portFired;
				doesPortMatch(portFired, portB[portIndex]);

				if( portFired ) {
					portBActivationCounter++;
				}
			}
		}

		if( portBActivationCounter == 0/* || portBActivationCounter != portBEnabledPortCounter*/ ) {
			return TextIndexOrTupleValue.makeDefaultValue();
		}

		bool calleeResult;
		uint[] resultTuple = applyGenePermutationToInput(inputs[0].tuple, calleeResult);
		if( !calleeResult ) {
			return TextIndexOrTupleValue.makeDefaultValue();
		}

		return TextIndexOrTupleValue.makeTuple(resultTuple);
	}

	protected final uint[] applyGenePermutationToInput(uint[] input, out bool success) {
		Permutation chosenPermutation = settingPermutations[genePermutationIndex];
		return chosenPermutation.apply(input, success);
	}

	public final string getDebugMathematica(uint numberOfOperatorsBeforeColumn) {
		import std.format : format;

		string result;

		result ~= "{" ;

		result ~= format("%d,  ", genePermutationIndex);

		// token comperators
		foreach( variantI; 0..numberOfVariants ) {
			result ~= "  {  " ;

			foreach( portIndex; 0..numberOfComperators ) {
				result ~= format("%d,", getTokenComperator(portIndex, variantI));
			}

			result ~= "  },  " ;

		}

		result ~= "}" ;

		result ~= "(* permutation index, followed by the list of token comperators for the different ports *)";

		return result;
	}
}

/+ outdated
// test for just one port wih one possibility
unittest {
	uint readWidth = 3;
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens,  numberOfComperators, numberOfVariants);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.setTokenComperator(0, 0, 1);

	tokenMatcherOperatorInstance.portAResultSelectorIndices[0] = 0;
	tokenMatcherOperatorInstance.portAResultSelectorIndices[1] = -1;

	TextIndexOrTupleValue result = tokenMatcherOperatorInstance.calculateResult([TextIndexOrTupleValue.makeTuple([1])]);
	assert(result.isTuple);

	assert(result.tuple.length == 2);
	assert(result.tuple[0] == 1);
	assert(result.tuple[1] == -1);
}

// test for just one port wih two possibilities
unittest {
	uint readWidth = 3;
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens,  numberOfComperators, numberOfVariants);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.setTokenComperator(0, 0, 1);
	tokenMatcherOperatorInstance.setTokenComperator(0, 1, 2);

	tokenMatcherOperatorInstance.portAResultSelectorIndices[0] = 0;
	tokenMatcherOperatorInstance.portAResultSelectorIndices[1] = -1;

	{
		TextIndexOrTupleValue result = tokenMatcherOperatorInstance.calculateResult([TextIndexOrTupleValue.makeTuple([1])]);
		assert(result.isTuple);

		assert(result.tuple.length == 2);
		assert(result.tuple[0] == 1);
		assert(result.tuple[1] == -1);
	}

	{
		TextIndexOrTupleValue result = tokenMatcherOperatorInstance.calculateResult([TextIndexOrTupleValue.makeTuple([2])]);
		assert(result.isTuple);

		assert(result.tuple.length == 2);
		assert(result.tuple[0] == 2);
		assert(result.tuple[1] == -1);
	}	
}


// test for a pair with two possibilities
unittest {
	uint readWidth = 3;
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens,  numberOfComperators, numberOfVariants);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.setTokenComperator(0, 0,  1);
	tokenMatcherOperatorInstance.setTokenComperator(0, 1,  2);

	tokenMatcherOperatorInstance.portAOffsetDeltas[1] = 1;
	tokenMatcherOperatorInstance.setTokenComperator(1, 0,  3);
	tokenMatcherOperatorInstance.setTokenComperator(1, 1,  4);



	tokenMatcherOperatorInstance.portAResultSelectorIndices[0] = 0;
	tokenMatcherOperatorInstance.portAResultSelectorIndices[1] = 1;

	{
		TextIndexOrTupleValue result = tokenMatcherOperatorInstance.calculateResult([TextIndexOrTupleValue.makeTuple([1, 3])]);
		assert(result.isTuple);

		assert(result.tuple.length == 2);
		assert(result.tuple[0] == 1);
		assert(result.tuple[1] == 3);
	}

	{
		TextIndexOrTupleValue result = tokenMatcherOperatorInstance.calculateResult([TextIndexOrTupleValue.makeTuple([2, 4])]);
		assert(result.isTuple);

		assert(result.tuple.length == 2);
		assert(result.tuple[0] == 2);
		assert(result.tuple[1] == 4);
	}	
}
+/