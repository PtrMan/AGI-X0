module CgpExample;




alias uint GeneIndex;


import CartesianGeneticProgramming;


import TokenOperators;















class TestRating : IRating {
	public final this() {
	}

	public final void resetRating(ChromosomeWithState chromosomeWithState) {
		chromosomeWithState.rating = 100.0f;
	}

	public final void rate(ChromosomeWithState chromosomeWithState) {		
		ValueType result = chromosomeWithState.getValueOfOutput(0 /* of output 0 just for testing */);
		
		if( trainingSampleType == TrainingSample.EnumType.NEGATIVE ) {
			if( result.isSet ) {
				chromosomeWithState.rating -= 5.0f;
				return;
			}
			else {
				chromosomeWithState.rating += 5.0f;
				return;
			}
		}
		else {

			// rate partial hits
			foreach( iterationInputMatched; result.inputMatched ) {
				if( iterationInputMatched ) {
					chromosomeWithState.rating += 1.0f;
				}
				else {
					chromosomeWithState.rating -= 0.8f;
				}
			}


			if( !result.isSet ) {
				return;
			}

			assert(result.tuple.length == trainingResultTokens.length);

			foreach( i; 0..trainingResultTokens.length ) {
				if( result.tuple[i] == trainingResultTokens[i] ) {
					chromosomeWithState.rating += 1.0f;
				}
				else {
					chromosomeWithState.rating -= 1.0f;
				}
			}
		}
	}

	public TrainingSample.EnumType trainingSampleType;
	public uint[] trainingResultTokens; // tokens which are correct

	//protected TokenRegister tokenRegister;
}







class TokenRegister {
	public final static class TokenException : Exception {
	    public final this () {
	        super("TokenException") ;
	    }
	}

	public final uint[] register(string[] tokens) {
		uint[] result;
		foreach( token; tokens ) {
			result ~= addGetToken(token);
		}
		return result;
	}

	public final uint getToken(string token) {
		foreach( i; 0..tokenDatabase.length ) {
			if( tokenDatabase[i] == token ) {
				return i;
			}
		}

		throw new TokenException();
	}

	// for debugging
	public final void debugDump() {
		import std.stdio;
		writeln("tokens:");
		foreach( i; 0..tokenDatabase.length ) {
			writeln(i, " ", tokenDatabase[i]);
		}
	}

	public final @property numberOfTokens() {
		return tokenDatabase.length;
	}

	protected final uint addGetToken(string token) {
		foreach( i; 0..tokenDatabase.length ) {
			if( tokenDatabase[i] == token ) {
				return i;
			}
		}

		// if we are here the token was not found, we add it
		tokenDatabase ~= token;
		return tokenDatabase.length-1;
	}

	protected string[] tokenDatabase;
}


import std.regex;

class Tokenizer {
	public final string[] tokenize(string input) {
		string[] resultTokens;

		string remainingInput = input;

		for(;;) {
			import std.stdio;
			//writeln(remainingInput);

			auto matchToken = matchFirst(remainingInput, regexToken);
			
			if( !matchToken ) {
				break;
			}

			string token = matchToken[1];

			remainingInput = remainingInput[token.length..$];

			if( token == " ") {
				continue;
			}
			
			//writeln(token, "<--");
			resultTokens ~= token;
			
		}
		
		return resultTokens;
	}

	protected auto regexToken = regex(`^([a-zA-Z0-9]+|[ <>\(\)\?!=\,;\{\}\[\]\.\$])`);
}



class TrainingSample {
	enum EnumType : bool {
		POSITIVE,
		NEGATIVE
	}

	public uint[] tokens;
	public EnumType type;

	public final this(uint[] tokens, EnumType type) {
		this.tokens = tokens;
		this.type = type;
	}
}

import Permutation : Permutation;

import std.random : uniform, Random, unpredictableSeed;
import std.stdio : writeln, write;

void main() {
	Tokenizer tokenizer = new Tokenizer();
	TokenRegister tokenRegister = new TokenRegister();


	uint generationReportInterval = 5000;



	uint matcherNumberOfComperatorsPerOperator = 10;
	uint matcherNumberOfVariants = 2; /// 2

	float matcherWildcardPropability = 0.5f;


	uint
		selectorNumberOfInputConnections = 3;
	uint selectorNumberOfNetworkInputs = 1;
	bool selectorSelectFromInputs = false;




	// dummy token to train for negative candidates
	uint dummyToken = tokenRegister.register(tokenizer.tokenize("$"))[0];


	// words for replacement
	string[] wordsForReplacement = [
		"human", "animal", "animals", "humans", "frog", "frogs", "duck", "ducks", "ant", "ants"
	];

	uint[] tokensForReplacement;

	foreach( iterationWordForReplacement; wordsForReplacement ) {
		tokensForReplacement ~= tokenRegister.register(tokenizer.tokenize(iterationWordForReplacement))[0];
	}

	





	Permutation[] permutations = [];
	permutations ~= Permutation.calcPermutation(tokenRegister.register(tokenizer.tokenize("humans are the only extant members of hominina clade,")), tokenRegister.register(tokenizer.tokenize("humans only extant members hominina clade")));


	TrainingSample[] trainingSamples;

	string[] positiveTrainingCandidatesAsString = [
		"humans are the only extant members of hominina clade,",
		"humans are the only extant members of hominina clade.",
	];

	foreach( iterationPositiveTrainingCandidateAsString; positiveTrainingCandidatesAsString ) {
		uint[] tokens = tokenRegister.register(tokenizer.tokenize(iterationPositiveTrainingCandidateAsString));

		trainingSamples ~= new TrainingSample(tokens, TrainingSample.EnumType.POSITIVE);


		class Tokenized {
			public final this(uint[] tokens) {
				this.tokens = tokens;
			}

			public uint[] tokens;
		}

		// numberOfVersionsForEachToken how many versions should exist for each replacement?
		Tokenized[] replaceTokensAtPositionsWithTokensForReplacement(uint[] tokens, uint[] indicesToReplace, uint numberOfVersionsForEachToken) {
			Tokenized[] result;

			foreach( iterationIndexToReplace; indicesToReplace ) {
				
				foreach( replacementToken; tokensForReplacement[$-1-numberOfVersionsForEachToken..$-1] ) {
					uint[] replacedTokens;
					// deep copy
					replacedTokens.length = tokens.length;
					foreach( i; 0..replacedTokens.length ) {
						replacedTokens[i] = tokens[i];
					}

					replacedTokens[iterationIndexToReplace] = replacementToken;
					result ~= new Tokenized(replacedTokens);
				}
			
			}

			return result;
		}

		// TODO< replace indices to replace with the indices of the example derived from the calculated permutation >
		uint[] indicesToReplace = [0, 3, 4, 5, 7, 8];
		uint numberOfVersionsForEachToken = matcherNumberOfVariants*1 + 1; // we add 1 to make it impossible for the matchers to create specialcases
		Tokenized[] tokenizedStringsOfPositiveExamples = replaceTokensAtPositionsWithTokensForReplacement(tokens, indicesToReplace, numberOfVersionsForEachToken);

		foreach( iterationTokenizedString; tokenizedStringsOfPositiveExamples ) {
			trainingSamples ~= new TrainingSample(iterationTokenizedString.tokens, TrainingSample.EnumType.POSITIVE);
		}
	}


	//trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans are the only extant members of hominina clade,"))   , TrainingSample.EnumType.POSITIVE);
	//trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans are the only extant members of hominina clade."))   , TrainingSample.EnumType.POSITIVE);

	trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans $ the only extant members of hominina clade."))   , TrainingSample.EnumType.NEGATIVE);
	trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans are $ only extant members of hominina clade."))   , TrainingSample.EnumType.NEGATIVE);
	trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans are the only extant members $ hominina clade."))   , TrainingSample.EnumType.NEGATIVE);

	//trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("humans are the only extant members of hominina clade."))   , TrainingSample.EnumType.POSITIVE);
	//trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("animals are animals"))   , TrainingSample.EnumType.POSITIVE);
	//trainingSamples ~= new TrainingSample(tokenRegister.register(tokenizer.tokenize("penguin is octopus"))   , TrainingSample.EnumType.NEGATIVE);


	IOperatorInstancePrototype!ValueType operatorInstancePrototype = new TokenMatcherOperatorInstancePrototype(
		tokenRegister.numberOfTokens,
		matcherNumberOfComperatorsPerOperator, matcherNumberOfVariants,
		permutations,
		matcherWildcardPropability,

		selectorNumberOfInputConnections,
		selectorNumberOfNetworkInputs,
		selectorSelectFromInputs
	);

	ulong numberOfGenerations = 10000;




	

	/// uint[][] typeIdsOfOperatorsToCreate = [[0, 0], [1, 1]];
	uint[][] typeIdsOfOperatorsToCreate = [[0], [1]];

	Random gen = Random(); //Random(44);
	gen.seed(unpredictableSeed);

	Parameters parameters = new Parameters();
	parameters.numberOfInputs = 1;
	parameters.numberOfOutputs = 1;
	Context context = Context.make(parameters, operatorInstancePrototype, typeIdsOfOperatorsToCreate, gen);




	tokenRegister.debugDump();



	TestRating ratingImplementation = new TestRating();
	ratingImplementation.trainingResultTokens = tokenRegister.register(tokenizer.tokenize("humans only extant members hominina clade"));




	void ratingDelegate(IRating rating, ChromosomeWithState chromosome, Context context) {
		TestRating castedRating = cast(TestRating)rating;

		foreach( iterationTrainingSample; trainingSamples ) {
			castedRating.trainingSampleType = iterationTrainingSample.type;

			context.executeGraph(chromosome, [TextIndexOrTupleValue.makeTuple(iterationTrainingSample.tokens)]);
			castedRating.rate(chromosome);
		}
	}

	uint numberOfMutations = 2;
	uint numberOfCandidates = 5; // 4 + 1  evolutionary strategy 
	EvolutionState evolutionState = new EvolutionState(numberOfCandidates, numberOfMutations, &ratingDelegate, context, ratingImplementation, parameters);



	foreach( generation; 0..numberOfGenerations ) {
		bool reportCurrentGeneration = false, selectionReportBestRatingChange;
		float selectionReportBestRating;

		reportCurrentGeneration |= ((generation % cast(typeof(generation))generationReportInterval) == 0);



		evolutionState.generation(selectionReportBestRating, selectionReportBestRatingChange);

		


		// report
		reportCurrentGeneration |= selectionReportBestRatingChange;

		bool reported = reportCurrentGeneration;

		if( reported ) {
			if( reportCurrentGeneration ) {
				write("gen=", generation, " ");
			}

			if( selectionReportBestRatingChange ) {
				write("bestRating=", selectionReportBestRating, " ");
			}

			writeln();



			// debug
			bool debugEnabled = true;
			if( debugEnabled ) {
				evolutionState.chromosomesWithStates[0].genotype.transcribeToOperatorsForNodes();
				import std.stdio;
				writeln(evolutionState.chromosomesWithStates[0].genotype.getDebugMathematica());
			}
		}
	}
}

