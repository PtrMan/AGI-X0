module TokenOperators;

import ValueMatrix : ValueMatrix;

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
	IOperatorInstance!ValueType createInstance();
}

class TokenMatcherOperatorInstancePrototype : IOperatorInstancePrototype!TextIndexOrTupleValue {
	public final this(uint readWidth, uint numberOfTokens,  uint numberOfComperators, uint numberOfVariants) {
		this.readWidth = readWidth;
		this.numberOfTokens = numberOfTokens;

		this.numberOfComperators = numberOfComperators;
		this.numberOfVariants = numberOfVariants;
	}

	public final IOperatorInstance!TextIndexOrTupleValue createInstance() {
		return new TokenMatcherOperatorInstance(readWidth, numberOfTokens,  numberOfComperators, numberOfVariants);
	}

	protected uint
		readWidth, numberOfTokens,
		numberOfComperators, numberOfVariants;
}



interface IOperatorInstance(ValueType) {
	// returns number of gene elements which need to be sliced for one node
	uint getGeneSliceWidth();

	void decodeSlicedGene(uint[] slicedGene);
	
	uint getNumberOfInputConnections();
	uint getInputGeneIndexForConnection(uint connectionIndex);

	ValueType calculateResult(ValueType[] inputs)
	in {
		assert(inputs.length == getNumberOfInputConnections());
	}
}

class TokenMatcherOperatorInstance : IOperatorInstance!TextIndexOrTupleValue {
	public final this(uint readWidth, uint numberOfTokens,  uint parameterNumberOfComperators, uint parameterNumberOfVariants) {
		this.readWidth = readWidth;
		this.numberOfTokens = numberOfTokens;

		portAOffsetDeltas.length = parameterNumberOfComperators;
		this.tokenComperatorMatrix = new ValueMatrix!int(parameterNumberOfComperators, parameterNumberOfVariants);
		disableAllComperators();
	}

	// configuration
	protected uint readWidth;
	protected uint numberOfTokens;

	protected final @property uint numberOfComperators() {
		return tokenComperatorMatrix.width;
	}

	protected final @property uint numberOfVariants() {
		return tokenComperatorMatrix.height;
	}

	protected final bool isTokenComperatorActivated(uint portIndex, uint variantIndex) {
		return tokenComperatorMatrix[variantIndex, portIndex] != -1;
	}

	protected final int getTokenComperator(uint portIndex, uint variantIndex) {
		return tokenComperatorMatrix[variantIndex, portIndex];
	}

	// for unittesting
	public final void setTokenComperator(uint portIndex, uint variantIndex, int value) {
		tokenComperatorMatrix[variantIndex, portIndex] = value;
	}

	protected final void disableAllComperators() {
		foreach( variantIndex; 0..numberOfVariants ) {
			foreach( comperatorIndex; 0..numberOfComperators ) {
				tokenComperatorMatrix[variantIndex, comperatorIndex] = -1;
			}
		}
	}


	protected ValueMatrix!int tokenComperatorMatrix;


	// into port A
	int[] portAOffsetDeltas;

	
	//uint tokenComperators[2*3]; // contains the numbers of the tokens to compare to, the firings of the results get ored together, only active tokens can fire
	//bool activatedTokenComperators[2*3];

	// picks out the words from portA to put it into the result, if it is -1 it is ignored
	int portAResultSelectorIndices[2];

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

		foreach( ref iterationPartAOffsetDelta; portAOffsetDeltas ) {
			iterationPartAOffsetDelta = (pullIntValueAndIncrementIndex() % (readWidth*2+1)) - readWidth;
		}
		
		foreach( variantIndex; 0..numberOfVariants ) {
			foreach( comperatorIndex; 0..numberOfComperators ) {
				tokenComperatorMatrix[variantIndex, comperatorIndex] = (pullIntValueAndIncrementIndex() % (numberOfTokens+1)) - 1;
			}
		}
		
		portAResultSelectorIndices[0] = (pullIntValueAndIncrementIndex() % (3+1)) -1;
		portAResultSelectorIndices[1] = (pullIntValueAndIncrementIndex() % (3+1)) -1;

		assert(runningI == getGeneSliceWidth());
	}

	public final uint getGeneSliceWidth() {
		return 
			portAOffsetDeltas.length + 
			tokenComperatorMatrix.width*tokenComperatorMatrix.height +
			portAResultSelectorIndices.length;
	}

	public final uint getNumberOfInputConnections() {
		return 1;
	}

	public final uint getInputGeneIndexForConnection(uint connectionIndex) {
		return 0; // always the input (just for testing)
	}

	public final TextIndexOrTupleValue calculateResult(TextIndexOrTupleValue[] inputs) {
		assert(inputs[0].tuple.length != 0);

		uint readIndex = 0; // just for testing

		// -1 means invalid
		int[] portA;
		portA.length = portAOffsetDeltas.length;
		foreach( i; 0..portA.length ) {
			portA[i] = -1;
		}


		// read to port A
		{
			foreach( i; 0..portA.length ) {
				int index = readIndex + portAOffsetDeltas[i];
				if( index < 0 || index >= inputs[0].tuple.length ) {
					continue;
				}

				portA[index] = inputs[0].tuple[index];
			}
		}


		uint[] portB;
		portB.length = portA.length;

		bool portBActivated = true;
		uint portBActivationCounter = 0, portBEnabledPortCounter = 0;

		bool match = false;
		
		// select from portA into portB
		// if there is a mismatch of an activated possibility portB gets disabled
		{
			foreach( portIndex; 0..3 ) {
				void doesPortMatch(out bool portFired, out uint result) {
					portFired = false;
					bool portActivated = false;
					
					foreach( variantI; 0..2 ) {
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

		if( portBActivationCounter == 0 || portBActivationCounter != portBEnabledPortCounter ) {
			return TextIndexOrTupleValue.makeDefaultValue();
		}

		// else we are fine and we build the result tuple
		{
			uint[] resultTuple;

			foreach( iterationPortAResultSelectorIndex; portAResultSelectorIndices ) {
				if( iterationPortAResultSelectorIndex == -1 ) {
					resultTuple ~= -1;
				}
				else {
					resultTuple ~= portB[iterationPortAResultSelectorIndex];
				}
			}

			return TextIndexOrTupleValue.makeTuple(resultTuple);
		}
	}
}

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
