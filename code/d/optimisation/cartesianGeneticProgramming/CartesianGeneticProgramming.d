module CartesianGeneticProgramming;

import std.random : uniform, Random, unpredictableSeed;

import TokenOperators;
import CgpException : CgpException;
import Permutation : Permutation;


public alias TextIndexOrTupleValue ValueType;

class Parameters {
	public uint numberOfInputs, numberOfOutputs;

	public IOperatorInstancePrototype!ValueType operatorInstancePrototype;
}

// used to dispatch the function and other global stuff
class Context {
	// typeIdsOfOperatorsToCreate are the typeId's (and at the same type the layout) of the to be created operators
	public static Context make(Parameters parameters, IOperatorInstancePrototype!ValueType operatorInstancePrototype, uint[][] typeIdsOfOperatorsToCreate, Random gen) {
		Context result = new Context();
		result.protectedGlobals.numberOfInputs = parameters.numberOfInputs;
		result.protectedGlobals.numberOfOutputs = parameters.numberOfOutputs;

		result.operatorInstancePrototype = operatorInstancePrototype;
		result.typeIdsOfOperatorsToCreate = typeIdsOfOperatorsToCreate;
		result.gen = gen;

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
						uint numberOfOperatorsBeforeColumn = genotype.getOperatorInstanceWithInfoByLinearIndex(p).numberOfOperatorsBeforeThisColumn;
						uint connectionIndex = genotype.getOperatorInstanceWithInfoByLinearIndex(p).operatorInstance.getInputIndexForConnection(inputConnectionIndex, numberOfOperatorsBeforeColumn);
						
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

		
		

		resetOutputValues();
		resetToEvaluate();
		identifiyInitialnodesWhichNeedToBeEvaluated();
		transcribeToOperatorsForNodes(chromosome);
		whichNodesAreUsed();
	}

	protected void transcribeToOperatorsForNodes(ChromosomeWithState chromosome) {
		chromosome.genotype.transcribeToOperatorsForNodes();
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

		void resetReturnValues() {
			foreach( iterationEvaluationState; chromosome.evalutionStates.statesOfNodes ) {
				iterationEvaluationState.output = valueOfDefault();
			}
		}


		loadInputDataValues();
		transcribeToOperatorsForNodes(chromosome);
		resetReturnValues();



		Genotype genotype = chromosome.genotype;
		foreach( i; 0..genotype.numberOfOperatorInstances() ) {
			if( !chromosome.evalutionStates.statesOfNodes[i].toEvaluate ) {
				continue;
			}
			
			ValueType[] inputs;

			foreach( inputConnectionIndex; 0..genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.getNumberOfInputConnections() ) {
				uint numberOfOperatorsBeforeColumn = genotype.getOperatorInstanceWithInfoByLinearIndex(i).numberOfOperatorsBeforeThisColumn;
				uint inputGeneIndex = genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.getInputIndexForConnection(inputConnectionIndex, numberOfOperatorsBeforeColumn);
				inputs ~= getValueBySourceIndex(inputGeneIndex);
			}

			chromosome.evalutionStates.statesOfNodes[i].output = genotype.getOperatorInstanceWithInfoByLinearIndex(i).operatorInstance.calculateResult(inputs);				
		}
	}

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
			uint geneIndex = uniform!"[)"(0, genotype.genes.length, gen);
			flipGeneToRandom(genotype, geneIndex);
		}
	}

	protected final void flipGeneToRandom(Genotype genotype, uint geneIndex) {
		genotype.genes[geneIndex] = uniform!"[)"(0, uint.max, gen);
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

	protected Random gen;
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



interface IRating {
	void resetRating(ChromosomeWithState chromosomeWithState);
	void rate(ChromosomeWithState chromosomeWithState);
}

class EvolutionState {
	public alias void delegate(IRating rating, ChromosomeWithState chromosome, Context context) TypeDelegateRating;

	public ChromosomeWithState[] chromosomesWithStates;
	public ChromosomeWithState[] temporaryMutants; // all time allocated to speed up the algorithm

	public final this(uint numberOfCandidates, uint numberOfMutations, TypeDelegateRating ratingDelegate, Context context, IRating rating, Parameters parameters) {
		this.numberOfCandidates = numberOfCandidates;
		this.numberOfMutations = numberOfMutations;
		this.ratingDelegate = ratingDelegate;
		this.context = context;
		this.rating = rating;
		this.parameters = parameters;
		setup();
	}

	public final void generation(out float selectionReportBestRating, out bool selectionReportBestRatingChange) {
		copyToTemporary();
		mutate();
		evaluateAndRate();
		selectBestOne(selectionReportBestRating, selectionReportBestRatingChange);
	}

	// copy to temporary which get mutated
	protected final void copyToTemporary() {
		foreach( iterationMutant; temporaryMutants ) {
			chromosomesWithStates[0].copyChromosomeToDestination(iterationMutant);
		}
	}

	protected final void mutate() {
		foreach( iterationMutant; temporaryMutants ) {
			context.pointMutationOnGene(iterationMutant.genotype, numberOfMutations);
		}
	}

	protected final void evaluateAndRate() {
		foreach( iterationChromosome; temporaryMutants ) {
			context.decodeChromosome(iterationChromosome);
			
			rating.resetRating(iterationChromosome);

			ratingDelegate(rating, iterationChromosome, context);
		}
	}

	protected final void selectBestOne(out float selectionReportBestRating, out bool selectionReportBestRatingChange) {
		selectionReportBestRatingChange = false;

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
			selectionReportBestRatingChange = true;
			selectionReportBestRating = bestRating;
		}
	}


	protected final void setup() {
		// we just maintain one candidate
		chromosomesWithStates ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
		
		foreach( i; 0..numberOfCandidates ) {
			// TODO< can be null entities, where literaly everyting is null >
			temporaryMutants ~= ChromosomeWithState.createFromGenotype(context.createRandomGenotype(), parameters.numberOfInputs);
		}
	}

	protected uint numberOfCandidates;
	protected uint numberOfMutations;

	protected TypeDelegateRating ratingDelegate;
	protected Context context;
	protected IRating rating;
	protected Parameters parameters;
}