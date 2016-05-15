module TokenOperators;


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
	public final this(uint readWidth, uint numberOfTokens) {
		this.readWidth = readWidth;
		this.numberOfTokens = numberOfTokens;
	}

	public final IOperatorInstance!TextIndexOrTupleValue createInstance() {
		return new TokenMatcherOperatorInstance(readWidth, numberOfTokens);
	}

	protected uint readWidth, numberOfTokens;
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
	public final this(uint readWidth, uint numberOfTokens) {
		this.readWidth = readWidth;
		this.numberOfTokens = numberOfTokens;
	}

	// configuration
	protected uint readWidth;
	protected uint numberOfTokens;


	// into port A
	int portAOffsetDeltas[3];

	
	uint tokenComperators[2*3]; // contains the numbers of the tokens to compare to, the firings of the results get ored together, only active tokens can fire
	bool activatedTokenComperators[2*3];

	// picks out the words from portA to put it into the result, if it is -1 it is ignored
	int portAResultSelectorIndices[2];

	public final void decodeSlicedGene(uint[] slicedGene) {
		uint runningI = 0;

		portAOffsetDeltas[0] = (cast(int)slicedGene[runningI] % (readWidth*2+1)) - readWidth; runningI++;
		portAOffsetDeltas[1] = (cast(int)slicedGene[runningI] % (readWidth*2+1)) - readWidth; runningI++;
		portAOffsetDeltas[2] = (cast(int)slicedGene[runningI] % (readWidth*2+1)) - readWidth; runningI++;
		
		tokenComperators[0 + 0] = slicedGene[runningI] % numberOfTokens; runningI++;
		tokenComperators[0 + 1] = slicedGene[runningI] % numberOfTokens; runningI++;
		tokenComperators[0 + 2] = slicedGene[runningI] % numberOfTokens; runningI++;
		tokenComperators[3 + 0] = slicedGene[runningI] % numberOfTokens; runningI++;
		tokenComperators[3 + 1] = slicedGene[runningI] % numberOfTokens; runningI++;
		tokenComperators[3 + 2] = slicedGene[runningI] % numberOfTokens; runningI++;

		activatedTokenComperators[0 + 0] = (slicedGene[runningI] & 1) != 0; runningI++;
		activatedTokenComperators[0 + 1] = (slicedGene[runningI] & 1) != 0; runningI++;
		activatedTokenComperators[0 + 2] = (slicedGene[runningI] & 1) != 0; runningI++;
		activatedTokenComperators[3 + 0] = (slicedGene[runningI] & 1) != 0; runningI++;
		activatedTokenComperators[3 + 1] = (slicedGene[runningI] & 1) != 0; runningI++;
		activatedTokenComperators[3 + 2] = (slicedGene[runningI] & 1) != 0; runningI++;

		portAResultSelectorIndices[0] = cast(int)slicedGene[runningI] -1; runningI++;
		portAResultSelectorIndices[1] = cast(int)slicedGene[runningI] -1; runningI++;

		assert(runningI == getGeneSliceWidth());
	}

	public final uint getGeneSliceWidth() {
		return 
			portAOffsetDeltas.length + 
			tokenComperators.length +
			activatedTokenComperators.length +
			portAResultSelectorIndices.length;
	}

	public final uint getNumberOfInputConnections() {
		return 1;
	}

	public final uint getInputGeneIndexForConnection(uint connectionIndex) {
		assert(false, "Should never get called!");
	}

	public final TextIndexOrTupleValue calculateResult(TextIndexOrTupleValue[] inputs) {
		uint readIndex = 0; // just for testing

		// -1 means invalid
		int[portAOffsetDeltas.length] portA;
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


		uint[portA.length] portB;
		
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

						if( !activatedTokenComperators[variantI*3 + portIndex] ) {
							continue;
						}

						portActivated = true;

						bool isPortSet = portA[portIndex] != -1;
						if( !isPortSet ) {
							continue;
						}


						if( tokenComperators[variantI*3 + portIndex] == portA[portIndex] ) {
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

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.activatedTokenComperators[0] = true;
	tokenMatcherOperatorInstance.tokenComperators[0*3 + 0] = 1;

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

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.activatedTokenComperators[0*3 + 0] = true;
	tokenMatcherOperatorInstance.tokenComperators[0*3 + 0] = 1;

	tokenMatcherOperatorInstance.activatedTokenComperators[1*3 + 0] = true;
	tokenMatcherOperatorInstance.tokenComperators[1*3 + 0] = 2;

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

	TokenMatcherOperatorInstance tokenMatcherOperatorInstance = new TokenMatcherOperatorInstance(readWidth, numberOfTokens);

	tokenMatcherOperatorInstance.portAOffsetDeltas[0] = 0;
	tokenMatcherOperatorInstance.activatedTokenComperators[0*3 + 0] = true;
	tokenMatcherOperatorInstance.tokenComperators[0*3 + 0] = 1;

	tokenMatcherOperatorInstance.activatedTokenComperators[1*3 + 0] = true;
	tokenMatcherOperatorInstance.tokenComperators[1*3 + 0] = 2;


	tokenMatcherOperatorInstance.portAOffsetDeltas[1] = 1;
	tokenMatcherOperatorInstance.activatedTokenComperators[0*3 + 1] = true;
	tokenMatcherOperatorInstance.tokenComperators[0*3 + 1] = 3;

	tokenMatcherOperatorInstance.activatedTokenComperators[1*3 + 1] = true;
	tokenMatcherOperatorInstance.tokenComperators[1*3 + 1] = 4;




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
