module CartesianGeneticProgramming;

import TokenOperators;
import CgpException : CgpException;

import std.random : uniform;

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


// used to dispatch the function and other global stuff
class Context {
	// typeIdsOfOperatorsToCreate are the typeId's (and at the same type the layout) of the to be created operators
	public static Context make(Parameters parameters, IOperatorInstancePrototype!ValueType operatorInstancePrototype, uint[][] typeIdsOfOperatorsToCreate) {
		Context result = new Context();
		result.protectedGlobals.numberOfInputs = parameters.numberOfInputs;
		result.protectedGlobals.numberOfOutputs = parameters.numberOfOutputs;

		result.operatorInstancePrototype = operatorInstancePrototype;
		result.typeIdsOfOperatorsToCreate = typeIdsOfOperatorsToCreate;

		return result;
	}

	protected final this() {
		protectedGlobals = new Globals();
	}

	// returns the default of the value
	public final ValueType valueOfDefault() {
		return ValueType.makeDefaultValue();
	}
	

	// for testing public
	// decoding CGP chromosomes
	// see
	// http://www.cartesiangp.co.uk/cgp-in-nutshell.pdf  page 8
	public final void decodeChromosome(ChromosomeWithState chromosome) {
		void resetOutputValues() {
			chromosome.evalutionStates.resetOutputValues(valueOfDefault());
		}
		

		void resetToEvaluate() {
			chromosome.evalutionStates.resetToEvaluate();
		}

		void identifiyInitialnodesWhichNeedToBeEvaluated() {
			foreach( i; 0..globals.numberOfOutputs ) {
				ConnectionAdress.EnumType connectionType;
				uint translatedConnectionIndex;
				ConnectionAdress.translateIndexToTypeAndIndexOfType(chromosome.genotype.getOutputGene(i), connectionType, translatedConnectionIndex, globals.numberOfInputs);

				if( connectionType == ConnectionAdress.EnumType.NODE ) {
					chromosome.evalutionStates.statesOfNodes[translatedConnectionIndex].toEvaluate = true;
				}
			}
		}

		void whichNodesAreUsed() {
			Genotype genotype = chromosome.genotype;

			int p = genotype.numberOfOperatorInstances()-1;

			for(;;) {
				if( chromosome.evalutionStates.statesOfNodes[p].toEvaluate ) {
					foreach( inputConnectionIndex; 0..genotype.getOperatorInstanceWithInfoByLinearIndex(p).operatorInstance.getNumberOfInputConnections() ) {
						uint connectionIndex = genotype.getOperatorInstanceWithInfoByLinearIndex(p).operatorInstance.getInputIndexForConnection(inputConnectionIndex);
						
						ConnectionAdress.EnumType connectionType;
						uint translatedConnectionIndex;
						ConnectionAdress.translateIndexToTypeAndIndexOfType(connectionIndex, connectionType, translatedConnectionIndex, globals.numberOfInputs);

						if( connectionType == ConnectionAdress.EnumType.NODE ) {
							chromosome.evalutionStates.statesOfNodes[translatedConnectionIndex].toEvaluate = true;
						}
					}
				}

				p--;
				if( p < 0 ) {
					break;
				}
			}
		}

		
		void transcribeToOperatorsForNodes() {
			chromosome.genotype.transcribeToOperatorsForNodes();
		}


		resetOutputValues();
		resetToEvaluate();
		identifiyInitialnodesWhichNeedToBeEvaluated();
		whichNodesAreUsed();
		transcribeToOperatorsForNodes();
	}

	public final void executeGraph(ChromosomeWithState chromosome, ValueType[] input) {
		void loadInputDataValues() {
			foreach( i; 0..protectedGlobals.numberOfInputs ) {
				chromosome.evalutionStates.statesOfNodes[i].output = input[i];
			}
		}

		// translates the index from the graph internaly to either a input index or a node index and pulls the value
		ValueType getValueBySourceIndex(uint sourceIndex) {
			ConnectionAdress.EnumType connectionType;
			uint translatedConnectionIndex;
			ConnectionAdress.translateIndexToTypeAndIndexOfType(sourceIndex, connectionType, translatedConnectionIndex, globals.numberOfInputs);

			if( connectionType == ConnectionAdress.EnumType.NODE ) {
				return chromosome.evalutionStates.statesOfNodes[translatedConnectionIndex].output;
			}
			else if( connectionType == ConnectionAdress.EnumType.INPUT ) {
				return input[translatedConnectionIndex];
			}
			else {
				throw new CgpException("Unreachable");
			}
		}


		loadInputDataValues();



		Genotype genotype = chromosome.genotype;
		foreach( i; 0..genotype.numberOfOperatorInstances() ) {
			if( !chromosome.evalutionStates.statesOfNodes[i].toEvaluate ) {
				continue;
			}
			
			ValueType[] inputs;

			foreach( inputConnectionIndex; 0..genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.getNumberOfInputConnections() ) {
				uint inputGeneIndex = genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.getInputIndexForConnection(inputConnectionIndex);
				inputs ~= getValueBySourceIndex(inputGeneIndex);
			}

			chromosome.evalutionStates.statesOfNodes[i].output = genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.calculateResult(inputs);				
		}
	}

	///public final ChromosomeView createRandomGenotypeView() {
	///	return ChromosomeView.makeViewOfGenotype(createRandomGenotype());
	///}

	protected final Genotype createRandomGenotype() {
		Genotype randomGenotype = new Genotype(operatorInstancePrototype, typeIdsOfOperatorsToCreate, globals.numberOfInputs, globals.numberOfOutputs);
		
		foreach( geneIndex; 0..randomGenotype.genes.length ) {
			flipGeneToRandom(randomGenotype, geneIndex);
		}

		return randomGenotype;
	}

	// see
	// http://www.cartesiangp.co.uk/cgp-in-nutshell.pdf  page 9
	public final void pointMutationOnGene(Genotype genotype, uint numberOfMutations) {
		foreach( mutationIterator; 0..numberOfMutations ) {
			uint geneIndex = uniform!"[)"(0, genotype.genes.length);
			flipGeneToRandom(genotype, geneIndex);
		}
	}

	protected final void flipGeneToRandom(Genotype genotype, uint geneIndex) {
		genotype.genes[geneIndex] = uniform!"[)"(0, uint.max);
	}


	public IOperatorInstancePrototype!ValueType operatorInstancePrototype;

	protected static class Globals : Parameters {
	}

	protected Globals protectedGlobals;

	public final @property Globals globals() {
		return protectedGlobals;
	}

	// are the typeIds of the operators to be created, and it contains the layout of the different operators too
	protected uint[][] typeIdsOfOperatorsToCreate;
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
	}

	public alias uint Gene;

	// function genes followed by connection genes followed by output genes
	public Gene[] genes;

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

		foreach( iterationColumn; typeIdsOfOperatorsToCreate ) {
			operatorInstancesWithInfo2[column].length = typeIdsOfOperatorsToCreate[column].length;

			uint row = 0;
			foreach( iterationOperator; iterationColumn ) {
				{
					operatorInstancesWithInfo2[column][row] = new OperatorInstanceWithInfo();
					uint typeId = typeIdsOfOperatorsToCreate[column][row];
					operatorInstancesWithInfo2[column][row].operatorInstance = operatorInstancePrototype.createInstance(typeId);
					operatorInstancesWithInfo2[column][row].genomeIndex = currentGenomeIndex;

					currentGenomeIndex += operatorInstancesWithInfo2[column][row].operatorInstance.getGeneSliceWidth();
				}

				row++;
				adressesIndex++;
			}

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

		foreach( iterationColumn; operatorInstancesWithInfo2 ) {
			foreach( iteratorOperatorInstanceWithInfo; iterationColumn ) {
				uint sliceIndex = iteratorOperatorInstanceWithInfo.genomeIndex;
				uint[] slicedGene = convertGenesToUint(genes[sliceIndex..sliceIndex+iteratorOperatorInstanceWithInfo.operatorInstance.getGeneSliceWidth()]);
				iteratorOperatorInstanceWithInfo.operatorInstance.decodeSlicedGene(slicedGene);
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
	public final void resetRating(ChromosomeWithState chromosomeWithState) {
		chromosomeWithState.rating = 0.0f;
	}

	public final void rate(ChromosomeWithState chromosomeWithState) {		
		ValueType result = chromosomeWithState.getValueOfOutput(0 /* of output 0 just for testing */);
		
		if( !result.isSet ) {
			return;
		}

		if( result.tuple[0] == 0 ) { // checks for "i"
			chromosomeWithState.rating += 1.0f;
		}

		if( result.tuple[1] == 2 ) { // checks for "tired"
			chromosomeWithState.rating += 1.0f;
		}
	}
}



import std.stdio : writeln, write;

void main() {
	// 0 : i
	// 1 : am
	// 2 : tired
	// 3 : a
	// 4 : road
	// 5 : it
	// 6 : is
	// 7 : very
	// 8 : rainy
	// 9 : extremly

	// i am tired
	// i am very tired
	// it is very rainy
	// it is rainy
	uint numberOfTokens = 10;
	uint readWidth = 4;

	uint numberOfComperatorsPerOperator = 3;
	uint numberOfVariants = 2;

	uint numberOfMatchingOperators = 1; // how many matching operators are there?

	uint
		selectorNumberOfInputConnections = 3,
		selectorNumberOfOperatorsToChoose = /* one because its the input to the whole network */1+numberOfMatchingOperators;


	IOperatorInstancePrototype!ValueType operatorInstancePrototype = new TokenMatcherOperatorInstancePrototype(
		readWidth, numberOfTokens,
		numberOfComperatorsPerOperator, numberOfVariants,

		selectorNumberOfInputConnections, selectorNumberOfOperatorsToChoose
	);

	ChromosomeWithState[] chromosomesWithStates;
	ChromosomeWithState[] temporaryMutants; // all time allocated to speed up the algorithm

	uint numberOfGenerations = 5000000;


	Parameters parameters = new Parameters();
	parameters.numberOfInputs = 1;
	parameters.numberOfOutputs = 1;

	uint numberOfMutations = 1;
	uint numberOfCandidates = 5; // 4 + 1  evolutionary strategy 

	uint[][] typeIdsOfOperatorsToCreate = [[0]];


	Context context = Context.make(parameters, operatorInstancePrototype, typeIdsOfOperatorsToCreate);

	ValueType[][] inputs = [
		[TextIndexOrTupleValue.makeTuple([0, 1, 2])], // i am tired
		//[TextIndexOrTupleValue.makeTuple([0, 1, 8, 2])], // i am rainy tired   - justt for testing
		[TextIndexOrTupleValue.makeTuple([0, 1, 7, 2])], // i am very tired
	];

	IRating ratingImplementation = new TestRating();

	// we just maintain one candidate
	chromosomesWithStates ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
	
	foreach( i; 0..numberOfCandidates ) {
		// TODO< can be null entities, where literaly everyting is null >
		temporaryMutants ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
	}

	uint generationReportInterval = 5000;

	foreach( generation; 0..numberOfGenerations ) {
		bool reportCurrentGeneration = false, reportBestRatingChange = false;
		float reportBestRating;

		reportCurrentGeneration |= ((generation % generationReportInterval) == 0);

		// copy to temporary which get mutated
		{
			foreach( iterationMutant; temporaryMutants ) {
				iterationMutant.copyChromosomeToDestination(iterationMutant);
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

				foreach( iterationInput; inputs ) {
					context.executeGraph(iterationChromosome, iterationInput);
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
		}
	}
}

