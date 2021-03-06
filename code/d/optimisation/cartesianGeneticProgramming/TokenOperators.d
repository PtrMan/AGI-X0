module optimisation.cartesianGeneticProgramming.TokenOperators;

// TODO< wildcard for the comperator, which is -2 >

import std.algorithm : min;

import optimisation.cartesianGeneticProgramming.CgpException : CgpException;
import optimisation.cartesianGeneticProgramming.ValueMatrix : ValueMatrix;

import optimisation.cartesianGeneticProgramming.Permutation : Permutation;

class TextIndexOrTupleValue {
	public enum EnumType {
		NOTSET,
		TOKEN,
		TUPLE
	}

	/*
	public static struct TupleElement {
		public static TupleElement makeValue(uint value) {
			TupleElement result;
			result.protectedValue = value;
			return result;
		}

		public static TupleElement makeNotSet() {
			TupleElement result;
			result.protectedIsNotSet = true;
			return result;
		}

		protected final this() {
		}

		public final @property bool isSet() {
			return !protectedIsNotSet;
		}

		public final @property uint getValue() {
			if( protectedIsNotSet ) {
				throw new NotSetException();
			}

			return protectedValue;
		}

		protected uint protectedValue;
		protected bool protectedIsNotSet;
	}
	*/

	protected EnumType type;
	public uint tokenIndex;

	//public TupleElement[] tuple;
	public uint[] tuple;

	// array with the information about the matching of the input
	// used by the rating to rate partial matches even if it didn't match completly
	public bool[] inputMatched;

	/*
	public final bool isPartialySet() {
		foreach( iterationElement; tuple ) {
			if( !iterationElement.isSet() ) {
				return true;
			}
		}

		return false;
	}*/

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
		uint matcherNumberOfTokens,
		uint matcherNumberOfComperators, uint matcherNumberOfVariants,
		Permutation[] matcherPermutations,
		float matcherWildcardPropability,

		uint selectorNumberOfInputConnections,
		uint selectorNumberOfNetworkInputs,
		bool selectorSelectFromInputs
	) {
		this.matcherNumberOfTokens = matcherNumberOfTokens;
		this.matcherPermutations = matcherPermutations;
		this.matcherWildcardPropability = matcherWildcardPropability;

		this.matcherNumberOfComperators = matcherNumberOfComperators;
		this.matcherNumberOfVariants = matcherNumberOfVariants;


		this.selectorNumberOfInputConnections = selectorNumberOfInputConnections;
		this.selectorNumberOfNetworkInputs = selectorNumberOfNetworkInputs;
		this.selectorSelectFromInputs = selectorSelectFromInputs;
	}

	public final IOperatorInstance!TextIndexOrTupleValue createInstance(uint typeId) {
		if( typeId == 0 ) {
			return new TokenMatcherOperatorInstance(matcherNumberOfTokens,  matcherNumberOfComperators, matcherNumberOfVariants,  matcherPermutations, matcherWildcardPropability);
		}
		else if( typeId == 1 ) {
			return new SelectorOperatorInstance(selectorNumberOfInputConnections, selectorNumberOfNetworkInputs, selectorSelectFromInputs);
		}
		else {
			throw new CgpException("Internal error: typeId is not recognized!");
		}
	}

	protected uint
		matcherNumberOfTokens,
		matcherNumberOfComperators, matcherNumberOfVariants;
	protected Permutation[] matcherPermutations;
	protected float matcherWildcardPropability;

	protected uint
		selectorNumberOfInputConnections;
	protected uint selectorNumberOfNetworkInputs;
	protected bool selectorSelectFromInputs;
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
	public final this(uint numberOfInputConnections, uint numberOfNetworkInputs, bool selectFromInputs) {
		this.numberOfInputConnections = numberOfInputConnections;
		this.numberOfNetworkInputs = numberOfNetworkInputs;
		this.selectFromInputs = selectFromInputs;
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

		uint resultInputIndexForConnection;

		if( selectFromInputs ) {
			resultInputIndexForConnection = slicedGeneForConnections[connectionIndex] % (numberOfOperatorsBeforeColumn + numberOfNetworkInputs);
		}
		else {
			resultInputIndexForConnection = numberOfNetworkInputs + (slicedGeneForConnections[connectionIndex] % numberOfOperatorsBeforeColumn);
		}

		return resultInputIndexForConnection;
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

	protected uint numberOfInputConnections;
	protected uint numberOfNetworkInputs;
	protected bool selectFromInputs;
}

class TokenMatcherOperatorInstance : IOperatorInstance!TextIndexOrTupleValue {
	public final this(uint numberOfTokens,  uint parameterNumberOfComperators, uint parameterNumberOfVariants,  Permutation[] permutations, float wildcardPropability) {
		this.settingNumberOfTokens = numberOfTokens;
		this.settingPermutations = permutations;
		this.settingWildcardPropability = wildcardPropability;

		this.geneTokenComperatorMatrix = new ValueMatrix!int(parameterNumberOfComperators, parameterNumberOfVariants);
		disableAllComperators();
	}

	// configuration
	protected uint settingNumberOfTokens;
	protected Permutation[] settingPermutations;
	protected float settingWildcardPropability;

	protected uint genePermutationIndex;
	protected ValueMatrix!int geneTokenComperatorMatrix;

	protected final @property uint numberOfComperators() {
		return geneTokenComperatorMatrix.width;
	}

	protected final @property uint numberOfVariants() {
		return geneTokenComperatorMatrix.height;
	}

	version(unittest) {
		public final void setTokenComperator(uint portIndex, uint variantIndex, int value) {
			geneTokenComperatorMatrix[variantIndex, portIndex] = value;
		}
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

		/*
		int pullIntValueAndIncrementIndex() {
			uint valueAsUint = pullValueAndIncrementIndex();
			uint valueMasked = valueAsUint & (uint.max >> 1); // we do this to not screw with th sign when we do the cast
			return cast(int)valueMasked;
		}
		*/

		genePermutationIndex = pullValueAndIncrementIndex() % settingPermutations.length;
		


		foreach( variantIndex; 0..numberOfVariants ) {
			foreach( comperatorIndex; 0..numberOfComperators ) {
				bool wasWildcardChosen;
				uint chosenToken;

				uint gene = pullValueAndIncrementIndex();
				rangeWithSpecilChoice(gene, settingNumberOfTokens, settingWildcardPropability, chosenToken, wasWildcardChosen);

				if( wasWildcardChosen ) {
					geneTokenComperatorMatrix[variantIndex, comperatorIndex] = -2;
				}
				else {
					geneTokenComperatorMatrix[variantIndex, comperatorIndex] = chosenToken;
				}
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
		uint[] applyGenePermutationToInput(uint[] input, out bool success) {
			Permutation chosenPermutation = settingPermutations[genePermutationIndex];
			return chosenPermutation.apply(input, success);
		}

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

		bool[] inputMatched;
		inputMatched.length = min(numberOfComperators, portA.length);
		
		// select from portA into portB
		// if there is a mismatch of an activated possibility portB gets disabled
		{
			foreach( portIndex; 0..numberOfComperators ) {
				bool hasPortWildCard(uint portIndex) {
					bool isTokenComperatorWildcard(uint portIndex, uint variantIndex) {
						return geneTokenComperatorMatrix[variantIndex, portIndex] == -2;
					}

					foreach( variantIndex; 0..numberOfVariants ) {
						if( isTokenComperatorWildcard(portIndex, variantIndex) ) {
							return true;
						}
					}

					return false;
				}

				void doesPortMatch(out bool portFired, out uint result) {
					bool isTokenComperatorActivated(uint portIndex, uint variantIndex) {
						return geneTokenComperatorMatrix[variantIndex, portIndex] >= 0;
					}

					

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
				if( hasPortWildCard(portIndex) ) {
					portFired = true; // if its a wildcard we fire the port whatever its matched with
					portB[portIndex] = portA[portIndex];
					portBEnabledPortCounter++;
				}
				else {
					doesPortMatch(portFired, portB[portIndex]);
				}
				
				if( portFired ) {
					portBActivationCounter++;
				}

				if( portFired ) {
					inputMatched[portIndex] = true;
				}
			}
		}

		if( portBActivationCounter == 0 || portBActivationCounter != portBEnabledPortCounter ) {
			return TextIndexOrTupleValue.makeDefaultValue();
		}



		bool calleeResult;
		uint[] resultTuple = applyGenePermutationToInput(inputs[0].tuple, calleeResult);
		TextIndexOrTupleValue result;
		if( !calleeResult ) {
			result = TextIndexOrTupleValue.makeDefaultValue();
		}
		else {
			result = TextIndexOrTupleValue.makeTuple(resultTuple);
		}

		result.inputMatched = inputMatched;
		return result;
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

	protected final int getTokenComperator(uint portIndex, uint variantIndex) {
		return geneTokenComperatorMatrix[variantIndex, portIndex];
	}
}

/+ outdated
// test for just one port wih one possibility
unittest {
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(numberOfTokens,  numberOfComperators, numberOfVariants);

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
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(numberOfTokens,  numberOfComperators, numberOfVariants);

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
	uint numberOfTokens = 5;

	uint numberOfComperators = 3;
	uint numberOfVariants = 2;

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(numberOfTokens,  numberOfComperators, numberOfVariants);

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


// helpers
uint calcRangeForSpecialChoice(uint numberOfDiscrete, float specialChoicePropability) {
	return cast(uint)(cast(float)numberOfDiscrete * (1.0f / specialChoicePropability));
}

void rangeWithSpecilChoice(uint number, uint numberOfDiscrete, float specialChoicePropability, out uint discreteChoice, out bool specialChoice) {
	uint numberOfDiscreteChoices = calcRangeForSpecialChoice(numberOfDiscrete, specialChoicePropability);

	uint numberMod = number % numberOfDiscreteChoices;

	specialChoice = numberMod >= numberOfDiscrete;
	if( !specialChoice ) {
		discreteChoice = numberMod;
	}
}




