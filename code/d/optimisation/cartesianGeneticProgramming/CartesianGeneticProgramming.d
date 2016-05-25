module CartesianGeneticProgramming;

import TokenOperators;
import CgpException : CgpException;
import Permutation : Permutation;

import std.random : uniform, Random, unpredictableSeed;

alias uint GeneIndex;

alias TextIndexOrTupleValue ValueType;


class Parameters {
	public uint numberOfInputs, numberOfOutputs;

	public IOperatorInstancePrototype!ValueType operatorInstancePrototype;
}

class ConnectionAdress {
	public enum EnumType {
		INPUT,
		NODE
	}

	public static void translateIndexToTypeAndIndexOfType(uint index, out EnumType type, out uint typeIndex, uint numberOfInputs) {
		// input adresses are followed by node adresses

		if( index < numberOfInputs ) {
			type = EnumType.INPUT;
			typeIndex = index;
			return;
		}
		else {
			type = EnumType.NODE;
			typeIndex = index - numberOfInputs;
			return;
		}
	}
}


struct OperatorAdress {
	uint column, row;
}


/*
 * maps between the linear adress of an operator to the operator
 *
 * the operators are ordered in columns
 *
 * column   column
 *  |         |
 *  V         V
 * 
 * [op]
 * [op]      [op]
 * [op]      [op]
 * [op]
 */
class OperatorMapping(ValueType) {
	
	public final OperatorAdress getOperatorAdressByLinearIndex(uint index) {
		assert(index < adressesByLinearIndex.length);

		return adressesByLinearIndex[index];
	}

	// [column][row]
	public final void calcMapping(uint[][] typeIdsOfOperatorsToCreate) {
		uint countInstances() {
			return sum(map!(iterationColumn => iterationColumn.length)(typeIdsOfOperatorsToCreate), 0);
		}

		adressesByLinearIndex.length = countInstances();
		uint adressesIndex = 0;

		uint column = 0;

		foreach( iterationColumn; typeIdsOfOperatorsToCreate ) {
			uint row = 0;
			foreach( iterationOperator; iterationColumn ) {
				adressesByLinearIndex[adressesIndex].column = column;
				adressesByLinearIndex[adressesIndex].row = row;

				row++;

				adressesIndex++;
			}

			column++;
		}
	}

	protected OperatorAdress[] adressesByLinearIndex;
}






struct EvaluationState {
	bool toEvaluate;
	ValueType output;
}

class EvaluationStates {
	public static EvaluationStates createBySize(uint length) {
		EvaluationStates result = new EvaluationStates();
		result.statesOfNodes.length = length;
		return result;
	}

	public EvaluationState[] statesOfNodes;

	public final void resetToEvaluate() {
		foreach( ref iterationState; statesOfNodes ) {
			iterationState.toEvaluate = false;
		}
	}

	public final void resetOutputValues(ValueType value) {
		foreach( ref iterationState; statesOfNodes ) {
			iterationState.output = value;
		}
	}

}



class ChromosomeWithState {
	public Genotype genotype;
	public EvaluationStates evalutionStates;

	public static ChromosomeWithState createFromGenotype(Genotype genotype, uint numberOfInputs) {
		ChromosomeWithState result = new ChromosomeWithState();
		result.genotype = genotype;
		result.evalutionStates = EvaluationStates.createBySize(numberOfInputs + genotype.numberOfOperatorInstances());
		
		result.cachedNumberOfInputs = numberOfInputs;
		return result;
	}

	// helper
	public final ValueType getValueOfOutput(uint outputIndex) {
		ConnectionAdress.EnumType connectionType;
		uint translatedConnectionIndex;

		ConnectionAdress.translateIndexToTypeAndIndexOfType(genotype.getOutputGene(outputIndex), connectionType, translatedConnectionIndex, cachedNumberOfInputs);

		if( connectionType == ConnectionAdress.EnumType.NODE ) {
			return evalutionStates.statesOfNodes[translatedConnectionIndex].output;
		}

		throw new CgpException("Internal error: Output node is input node, is not allowed");
	}

	public final void copyChromosomeToDestination(ChromosomeWithState destination) {
		assert(destination.genotype.genes.length == genotype.genes.length);
		foreach( geneI; 0..destination.genotype.genes.length ) {
			destination.genotype.genes[geneI] = genotype.genes[geneI];
		}
	}

	public float rating = 0.0f;

	protected uint cachedNumberOfInputs;
}

import std.algorithm.iteration : sum, map;

class Genotype {
	static public class OperatorInstanceWithInfo {
		public IOperatorInstance!ValueType operatorInstance;
		public uint genomeIndex;
		public uint numberOfOperatorsBeforeThisColumn;
	}

	public alias uint Gene;

	// function genes followed by connection genes followed by output genes
	public Gene[] genes;

	public final string getDebugMathematica() {
		string result;

		uint column = 0;

		foreach( iterationColumn; operatorInstancesWithInfo2 ) {
			import std.format : format;
			result ~= format("column %d\n", column);

			uint row = 0;
			foreach( iterationOperator; iterationColumn ) {
				result ~= (format("\t[%d]\t", row) ~ iterationOperator.operatorInstance.getDebugMathematica(iterationOperator.numberOfOperatorsBeforeThisColumn)  ~ "\n");

				row++;
				
			}

			column++;
		}

		return result;
	}

	public final OperatorInstanceWithInfo getOperatorInstanceWithInfoByLinearIndex(uint index) {
		OperatorAdress adress = operatorMapping.getOperatorAdressByLinearIndex(index);
		return operatorInstancesWithInfo2[adress.column][adress.row];
	}

	public final this(IOperatorInstancePrototype!ValueType operatorInstancePrototype, uint[][] typeIdsOfOperatorsToCreate,   uint cachedNumberOfInputs, uint cachedNumberOfOutputs) {
		uint countNumberOfOperatorInstances() {
			uint numberOfOperatorInstances = sum(map!(iterationColumn => iterationColumn.length)(operatorInstancesWithInfo2));
			assert( numberOfOperatorInstances > 0);
			return numberOfOperatorInstances;
		}

		void initOperatorMapping(uint[][] typeIdsOfOperatorsToCreate) {
			operatorMapping.calcMapping(typeIdsOfOperatorsToCreate);
		}

		void allocateGenes() {
			genes.length = getNumberOfGenes();
		}

		this.cachedNumberOfInputs = cachedNumberOfInputs;
		this.cachedNumberOfOutputs = cachedNumberOfOutputs;
		
		initOperatorMapping(typeIdsOfOperatorsToCreate);
		createOperatorInstances(operatorInstancePrototype, typeIdsOfOperatorsToCreate);
		allocateGenes();
		cachedNumberOfOperatorInstances = countNumberOfOperatorInstances();
	}

	protected final void createOperatorInstances(IOperatorInstancePrototype!ValueType operatorInstancePrototype, uint[][] typeIdsOfOperatorsToCreate) {
		operatorInstancesWithInfo2.length = typeIdsOfOperatorsToCreate.length;


		uint currentGenomeIndex = 0;

		uint adressesIndex = 0;
		uint column = 0;

		uint runningCounterForOperatorsBeforeThisColumn = 0;

		foreach( iterationColumn; typeIdsOfOperatorsToCreate ) {
			operatorInstancesWithInfo2[column].length = typeIdsOfOperatorsToCreate[column].length;

			uint row = 0;
			foreach( iterationOperator; iterationColumn ) {
				{
					operatorInstancesWithInfo2[column][row] = new OperatorInstanceWithInfo();
					uint typeId = typeIdsOfOperatorsToCreate[column][row];
					operatorInstancesWithInfo2[column][row].operatorInstance = operatorInstancePrototype.createInstance(typeId);
					operatorInstancesWithInfo2[column][row].genomeIndex = currentGenomeIndex;
					operatorInstancesWithInfo2[column][row].numberOfOperatorsBeforeThisColumn = runningCounterForOperatorsBeforeThisColumn;

					currentGenomeIndex += operatorInstancesWithInfo2[column][row].operatorInstance.getGeneSliceWidth();
				}

				row++;
				adressesIndex++;
			}

			runningCounterForOperatorsBeforeThisColumn += operatorInstancesWithInfo2[column].length;

			column++;
		}
	}

	// TODO< rename >
	public OperatorInstanceWithInfo[][] operatorInstancesWithInfo2;

	protected OperatorMapping!ValueType operatorMapping = new OperatorMapping!ValueType;

	protected uint cachedNumberOfOperatorInstances;

	public final @property uint numberOfOperatorInstances() {
		return cachedNumberOfOperatorInstances;
	}

	public final uint getOutputGene(uint outputIndex) {
		// calculate the output gene,
		// the output gene starts with all inputs followed by all operator instances
		uint numberOfGenesExceptOuputGenes = getNumberOfGenes() - cachedNumberOfOutputs;
		uint index = numberOfGenesExceptOuputGenes + outputIndex;
		Gene gene = genes[index];

		return cachedNumberOfInputs + (gene % (numberOfOperatorInstances));
	}


	///////////////////////////
	// misc

	public final void transcribeToOperatorsForNodes() {
		uint[] convertGenesToUint(Gene[] array) {
			uint[] result;
			result.length = array.length;
			foreach( i; 0..array.length ) {
				result[i] = array[i];
			}
			return result;
		}

		// for debugging
		bool[] geneMarker;
		geneMarker.length = getNumberOfGenes();

		foreach( iterationColumn; operatorInstancesWithInfo2 ) {
			foreach( iteratorOperatorInstanceWithInfo; iterationColumn ) {
				uint sliceIndex = iteratorOperatorInstanceWithInfo.genomeIndex;
				uint[] slicedGene = convertGenesToUint(genes[sliceIndex..sliceIndex+iteratorOperatorInstanceWithInfo.operatorInstance.getGeneSliceWidth()]);
				iteratorOperatorInstanceWithInfo.operatorInstance.decodeSlicedGene(slicedGene);


				{
					uint i = 0;

					foreach( ref bool iterationFlag; geneMarker[sliceIndex..sliceIndex+iteratorOperatorInstanceWithInfo.operatorInstance.getGeneSliceWidth()] ) {
						if(iterationFlag) {
							import std.stdio;
							writeln("i= ", i);
							assert(false);
						}
						i++;
					}
				}
				


				foreach( ref bool iterationFlag; geneMarker[sliceIndex..sliceIndex+iteratorOperatorInstanceWithInfo.operatorInstance.getGeneSliceWidth()] ) {
					iterationFlag = true;
				}
			}
		}
	}
	

	///////////////////////////
	// calculate the offsets of the different sections of the genome

	


	protected final uint getNumberOfGenes() {
		return operatorInstancesWithInfo2[$-1][$-1].genomeIndex + operatorInstancesWithInfo2[$-1][$-1].operatorInstance.getGeneSliceWidth() + cachedNumberOfOutputs;
	}

	protected uint cachedNumberOfOutputs, cachedNumberOfInputs;
}






interface IRating {
	void resetRating(ChromosomeWithState chromosomeWithState);
	void rate(ChromosomeWithState chromosomeWithState);
}

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

			//import std.stdio;
			//writeln(result.tuple.length);
			//writeln(result.tuple);
			//writeln(trainingResultTokens.length);
			//writeln(trainingResultTokens);

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

	// TODO< generate training samples after pattern >

	IOperatorInstancePrototype!ValueType operatorInstancePrototype = new TokenMatcherOperatorInstancePrototype(
		tokenRegister.numberOfTokens,
		matcherNumberOfComperatorsPerOperator, matcherNumberOfVariants,
		permutations,
		matcherWildcardPropability,

		selectorNumberOfInputConnections,
		selectorNumberOfNetworkInputs,
		selectorSelectFromInputs
	);

	ChromosomeWithState[] chromosomesWithStates;
	ChromosomeWithState[] temporaryMutants; // all time allocated to speed up the algorithm

	ulong numberOfGenerations = 10000;


	Parameters parameters = new Parameters();
	parameters.numberOfInputs = 1;
	parameters.numberOfOutputs = 1;

	uint numberOfMutations = 2;
	uint numberOfCandidates = 5; // 4 + 1  evolutionary strategy 

	/// uint[][] typeIdsOfOperatorsToCreate = [[0, 0], [1, 1]];
	uint[][] typeIdsOfOperatorsToCreate = [[0], [1]];

	Random gen = Random(); //Random(44);
	gen.seed(unpredictableSeed);

	Context context = Context.make(parameters, operatorInstancePrototype, typeIdsOfOperatorsToCreate, gen);




	tokenRegister.debugDump();



	TestRating ratingImplementation = new TestRating();
	ratingImplementation.trainingResultTokens = tokenRegister.register(tokenizer.tokenize("humans only extant members hominina clade"));



	// we just maintain one candidate
	chromosomesWithStates ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
	
	foreach( i; 0..numberOfCandidates ) {
		// TODO< can be null entities, where literaly everyting is null >
		temporaryMutants ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
	}


	foreach( generation; 0..numberOfGenerations ) {
		bool reportCurrentGeneration = false, reportBestRatingChange = false;
		float reportBestRating;

		reportCurrentGeneration |= ((generation % cast(typeof(generation))generationReportInterval) == 0);

		// copy to temporary which get mutated
		{
			foreach( iterationMutant; temporaryMutants ) {
				chromosomesWithStates[0].copyChromosomeToDestination(iterationMutant);
			}
		}

		// mutate
		{
			foreach( iterationMutant; temporaryMutants ) {
				context.pointMutationOnGene(iterationMutant.genotype, numberOfMutations);
			}
		}

		// evaluation and rating
		{
			foreach( iterationChromosome; temporaryMutants ) {
				context.decodeChromosome(iterationChromosome);
				
				ratingImplementation.resetRating(iterationChromosome);

				foreach( iterationTrainingSample; trainingSamples ) {
					ratingImplementation.trainingSampleType = iterationTrainingSample.type;

					context.executeGraph(iterationChromosome, [TextIndexOrTupleValue.makeTuple(iterationTrainingSample.tokens)]);
					ratingImplementation.rate(iterationChromosome);
				}

				//writeln("rating of mutant = ", iterationChromosome.rating);
			}
		}

		// selection of best one
		{
			int bestPseudoIndex = -1;
			float bestRating = chromosomesWithStates[0].rating, oldBestRating = bestRating;

			int pseudoIndex = 0;
			foreach( iterationChromosome; temporaryMutants ) {
				// equal is important here to allow for genetic drift!
				if( iterationChromosome.rating >= bestRating ) {
					bestPseudoIndex = pseudoIndex;
					bestRating = iterationChromosome.rating;
				}

				pseudoIndex++;
			}

			if( bestPseudoIndex != -1 ) {
				temporaryMutants[bestPseudoIndex].copyChromosomeToDestination(chromosomesWithStates[0]);
				chromosomesWithStates[0].rating = temporaryMutants[bestPseudoIndex].rating;
			}

			// report
			if( bestRating > oldBestRating ) {
				reportBestRatingChange = true;
				reportBestRating = bestRating;
			}
		}

		



		// report
		reportCurrentGeneration |= reportBestRatingChange;

		bool reported = reportCurrentGeneration;

		if( reported ) {
			if( reportCurrentGeneration ) {
				write("gen=", generation, " ");
			}

			if( reportBestRatingChange ) {
				write("bestRating=", reportBestRating, " ");
			}

			writeln();



			// debug
			bool debugEnabled = true;
			if( debugEnabled ) {
				chromosomesWithStates[0].genotype.transcribeToOperatorsForNodes();
				import std.stdio;
				writeln(chromosomesWithStates[0].genotype.getDebugMathematica());
			}
		}
	}
}

