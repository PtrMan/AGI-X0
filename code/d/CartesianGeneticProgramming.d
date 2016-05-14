module CartesianGeneticProgramming;


import std.random : uniform;

alias uint GeneIndex;

alias double ValueType;


class Parameters {
	public uint numberOfInputs;
	public uint numberOfNodes; // without input and output, just real nodes
	public uint numberOfInputsPerNode;

	public uint numberOfOutputs;
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

// used to dispatch the function and other global stuff
class Context {
	public static Context make(Parameters parameters) {
		Context result = new Context();
		result.protectedGlobals.numberOfInputs = parameters.numberOfInputs;
		result.protectedGlobals.numberOfNodes = parameters.numberOfNodes;
		result.protectedGlobals.numberOfInputsPerNode = parameters.numberOfInputsPerNode;
		result.protectedGlobals.numberOfOutputs = parameters.numberOfOutputs;
		return result;
	}

	protected final this() {
		protectedGlobals = new Globals();
	}

	public final ValueType computeNode(ValueType a, ValueType b, uint functionIndex) {
		// TODO< code generated dispatch logic for each task >
		if( functionIndex == 0 ) { // stupid addition
			writeln("OP  ", a, " + ", b, " = ", a+b);

			return a + b;
		}
		else if( functionIndex == 1 ) { // stupid multiplication
			writeln("OP  ", a, " * ", b, " = ", a*b);

			return a * b;
		}
		assert(false, "Internal Error");
		return 0.0; // shouldn't ever be reached
	}

	// returns the default of the value
	public final ValueType valueOfDefault() {
		return 0.0f;
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
				ConnectionAdress.translateIndexToTypeAndIndexOfType(chromosome.view.getOutputGene(i), connectionType, translatedConnectionIndex, globals.numberOfInputs);

				if( connectionType == ConnectionAdress.EnumType.NODE ) {
					chromosome.evalutionStates.statesOfNodes[translatedConnectionIndex].toEvaluate = true;
				}
			}
		}

		void whichNodesAreUsed() {
			int p = chromosome.view.nodes.length-1;

			for(;;) {
				if( chromosome.evalutionStates.statesOfNodes[p].toEvaluate ) {
					foreach( iterationConnectionI; 0..globals.numberOfInputsPerNode ) {
						auto iterationConnection = chromosome.view.nodes[p].connections[iterationConnectionI];

						ConnectionAdress.EnumType connectionType;
						uint translatedConnectionIndex;
						ConnectionAdress.translateIndexToTypeAndIndexOfType(iterationConnection, connectionType, translatedConnectionIndex, globals.numberOfInputs);

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

		void loadInputDataValues() {
			foreach( i; 0..protectedGlobals.numberOfInputs ) {
				chromosome.evalutionStates.statesOfNodes[i].output = input[i];
			}
		}

		void executeGraph() {
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
					assert(false, "Unreachable");
					// TODO< throw something >
					return 0.0f;
				}
			}

			foreach( i; 0..chromosome.view.nodes.length ) {
				if( chromosome.evalutionStates.statesOfNodes[i].toEvaluate ) {
					assert( globals.numberOfInputsPerNode == 2 ); // we just have it implemented for two inputs
					
					uint x = chromosome.view.nodes[i].connections[0];
					uint y = chromosome.view.nodes[i].connections[1];
					uint z = chromosome.view.nodes[i].function_;
					chromosome.evalutionStates.statesOfNodes[i].output = computeNode(getValueBySourceIndex(x), getValueBySourceIndex(y), z);
				}
			}
		}

		resetOutputValues();
		resetToEvaluate();
		identifiyInitialnodesWhichNeedToBeEvaluated();
		whichNodesAreUsed();
		loadInputDataValues();
		executeGraph();
	}

	public final ChromosomeView createRandomGenotypeView() {
		return ChromosomeView.makeViewOfGenotype(createRandomGenotype());
	}

	protected final Genotype createRandomGenotype() {
		Genotype randomGenotype = new Genotype(globals.numberOfNodes, numberOfFunctions, globals.numberOfInputsPerNode, globals.numberOfOutputs);
		
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
		if( genotype.genes[geneIndex].isFunctionGene ) {
			// change gene to a randomly chosen new valid function
			genotype.genes[geneIndex] = uniform!"[)"(0, numberOfFunctions);
		}
		else if( genotype.genes[geneIndex].isConnectionGene ) {
			// change gene to randomly chosen new valid connection
			genotype.genes[geneIndex] = uniform!"[)"(0, globals.numberOfInputs + globals.numberOfNodes);
		}
		else if( genotype.genes[geneIndex].isOutputGene ) {
			// change gene to a new valid output connection
			genotype.genes[geneIndex] = uniform!"[)"(0, globals.numberOfNodes); // TODO< wrong too >
		}
		else {
			assert(false, "Unreachable!");
		}
	}



	public final @property uint numberOfFunctions() {
		return 2;
	}


	public ValueType[] input;

	protected static class Globals : Parameters {
		/*
		public final @property uint numberOfConnections() {
			return numberOfNodes * numberOfInputsPerNode;
		}*/
	}

	protected Globals protectedGlobals;

	public final @property Globals globals() {
		return protectedGlobals;
	}
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

// simulates a c like array of uint's, but does something more fancier
class ConnectionsView {
	public final this(uint nodeIndex, ChromosomeView chromosomeView) {
		this.nodeIndex = nodeIndex;
		this.chromosomeView = chromosomeView;
	}

	public final uint opIndexAssign(uint value, size_t connectionIndex) {
		chromosomeView.setConnectionOfNodeIndex(nodeIndex, connectionIndex, value);
		return value;
	}

	public final uint opIndex(size_t connectionIndex) {
		return chromosomeView.getConnectionOfNodeIndex(nodeIndex, connectionIndex);
	}

	protected uint nodeIndex;
	protected ChromosomeView chromosomeView;
}

// view to a Chromosomeview and the NodeStateVector
class NodeView {
	public final this(uint nodeIndex, ChromosomeView chromosomeView) {
		this.nodeIndex = nodeIndex;
		this.chromosomeView = chromosomeView;
		connectionsView = new ConnectionsView(nodeIndex, chromosomeView);
	}

	public final @property uint function_() {
		return chromosomeView.getFunctionOfNodeIndex(nodeIndex);
	}

	public final @property void function_(uint newFunction) {
		chromosomeView.setFunctionOfNodeIndex(nodeIndex, newFunction);
	}

	public final @property ConnectionsView connections() {
		return connectionsView;
	}

	protected uint nodeIndex;
	protected ConnectionsView connectionsView;
	protected ChromosomeView chromosomeView;
}

// the chromosome is just a view on the Genotype
class ChromosomeView {
	public static ChromosomeView makeViewOfGenotype(Genotype genotypeViewOf) {
		ChromosomeView result = new ChromosomeView();
		result.genotypeViewOf = genotypeViewOf;

		result.nodes.length = genotypeViewOf.numberOfNodes;

		foreach( i; 0..result.nodes.length ) {
			result.nodes[i] = new NodeView(i, result);
		}

		return result;
	}

	protected final this() {
	}


	public NodeView[] nodes;

	public final uint getFunctionOfNodeIndex(uint nodeIndex) {
		return genotypeViewOf.getFunctionOfNodeIndex(nodeIndex);
	}

	public final void setFunctionOfNodeIndex(uint nodeIndex, uint newFunction) {
		genotypeViewOf.setFunctionOfNodeIndex(nodeIndex, newFunction);
	}

	public final void setConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex, uint value) {
		genotypeViewOf.setConnectionOfNodeIndex(nodeIndex, connectionIndex, value);
	}

	public final uint getConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex) {
		return genotypeViewOf.getConnectionOfNodeIndex(nodeIndex, connectionIndex);
	}

	public final uint getOutputGene(uint outputIndex) {
		return genotypeViewOf.getOutputGene(outputIndex);
	}

	public final Genotype getGenotypeViewOf() {
		return genotypeViewOf;
	}

	protected Genotype genotypeViewOf;
}


class ChromosomeWithState {
	public ChromosomeView view;
	public EvaluationStates evalutionStates;

	public static ChromosomeWithState createFromChromosomeView(ChromosomeView view) {
		ChromosomeWithState result = new ChromosomeWithState();
		result.view = view;
		result.evalutionStates = EvaluationStates.createBySize(view.nodes.length);
		return result;
	}

	// helper
	public final ValueType getValueOfOutput(uint outputIndex) {
		uint geneIndex = view.getOutputGene(outputIndex);
		return evalutionStates.statesOfNodes[geneIndex].output;
	}

	public final void copyChromosomeToDestination(ChromosomeWithState destination) {
		foreach( i; 0..view.nodes.length ) {
			// we can't assign the nodes directly because te nodes are views and it would mess up everything completly

			foreach( connectionI; 0..view.getGenotypeViewOf().numberOfConnectionsPerNode ) {
				destination.view.nodes[i].connections[connectionI] = view.nodes[i].connections[connectionI];
			}
			
			destination.view.nodes[i].function_ = view.nodes[i].function_;
		}
	}

	public final void checkInvariant() {
		assert(view.nodes.length == evalutionStates.statesOfNodes.length);
	}

	public float rating;
}

struct Gene {
	public final this(Genotype parentGenotype, uint indexInGenotype, uint value) {
		this.parentGenotype = parentGenotype;
		this.indexInGenotype = indexInGenotype;
		this.value = value;
	}

	public final @property bool isFunctionGene() {
		return parentGenotype.isFunctionGene(indexInGenotype);
	}

	public final @property bool isConnectionGene() {
		return parentGenotype.isConnectionGene(indexInGenotype);
	}

	public final @property bool isOutputGene() {
		return parentGenotype.isOutputGene(indexInGenotype);
	}

    /*public final void opAssign(uint other) {
    	value = other;
    }*/

    alias value this;

	protected Genotype parentGenotype;
	protected uint value;
	protected uint indexInGenotype;
}

class Genotype {
	// function genes followed by connection genes followed by output genes
	public Gene[] genes;

	public final this(uint cachedNumberOfNodes, uint cachedNumberOfFunctions, uint cachedNumberOfConnectionsPerNode, uint cachedNumberOfOutputs) {
		/*assert(cachedNumberOfNodes > 0);
		assert(cachedNumberOfFunctions > 0);
		assert(cachedNumberOfConnectionsPerNode > 0);
		assert(cachedNumberOfOutputs > 0);*/

		this.protectedCachedNumberOfNodes = cachedNumberOfNodes;
		this.cachedNumberOfFunctions = cachedNumberOfFunctions;
		this.cachedNumberOfConnectionsPerNode = cachedNumberOfConnectionsPerNode;
		this.cachedNumberOfOutputs = cachedNumberOfOutputs;

		allocateGenes();
	}

	protected final void allocateGenes() {
		genes.length = getNumberOfGenes();
		foreach( i; 0..genes.length ) {
			genes[i].parentGenotype = this;
			genes[i].indexInGenotype = i;
		}
	}

	public final @property uint numberOfConnectionsPerNode() {
		return cachedNumberOfConnectionsPerNode;
	}

	public final @property uint numberOfNodes() {
		return protectedCachedNumberOfNodes;
	}

	///////////////////////////
	// used by chromosome views

	public final uint getFunctionOfNodeIndex(uint nodeIndex) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		uint functionIndex = genes[getOffsetOfGenesForFunctions() + nodeIndex];
		assert(functionIndex < cachedNumberOfFunctions);
		return functionIndex;
	}

	public final void setFunctionOfNodeIndex(uint nodeIndex, uint newFunction) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		assert(newFunction < cachedNumberOfFunctions);
		genes[getOffsetOfGenesForFunctions() + nodeIndex] = newFunction;
	}

	public final void setConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex, uint value) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		assert(connectionIndex < cachedNumberOfConnectionsPerNode);
		genes[getOffsetOfGenesForConnections() + nodeIndex*cachedNumberOfConnectionsPerNode + connectionIndex] = value;
	}

	public final uint getConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		assert(connectionIndex < cachedNumberOfConnectionsPerNode);
		return genes[getOffsetOfGenesForConnections() + nodeIndex*cachedNumberOfConnectionsPerNode + connectionIndex];
	}

	public final uint getOutputGene(uint outputIndex) {
		assert(outputIndex < cachedNumberOfOutputs);
		return genes[getOffsetOfGenesForOutputs() + outputIndex];
	}
	

	////////////////////////
	// used by mutation

	package final @property bool isFunctionGene(uint index) {
		assert(index < protectedCachedNumberOfNodes + protectedCachedNumberOfNodes*cachedNumberOfConnectionsPerNode + cachedNumberOfOutputs);
		return index >= getOffsetOfGenesForFunctions() && index < getOffsetOfGenesForConnections();
	}

	package final @property bool isConnectionGene(uint index) {
		assert(index < protectedCachedNumberOfNodes + protectedCachedNumberOfNodes*cachedNumberOfConnectionsPerNode + cachedNumberOfOutputs);
		return index >= getOffsetOfGenesForConnections() && index < getOffsetOfGenesForOutputs();
	}

	package final @property bool isOutputGene(uint index) {
		assert(index < protectedCachedNumberOfNodes + protectedCachedNumberOfNodes*cachedNumberOfConnectionsPerNode + cachedNumberOfOutputs);
		return index >= getOffsetOfGenesForOutputs();
	}

	///////////////////////////
	// calculate the offsets of the different sections of the genome

	protected final uint getOffsetOfGenesForFunctions() {
		return 0;
	}

	protected final uint getOffsetOfGenesForConnections() {
		return protectedCachedNumberOfNodes;
	}

	protected final uint getOffsetOfGenesForOutputs() {
		return protectedCachedNumberOfNodes + protectedCachedNumberOfNodes*cachedNumberOfConnectionsPerNode;
	}

	protected final uint getNumberOfGenes() {
		return protectedCachedNumberOfNodes + protectedCachedNumberOfNodes*cachedNumberOfConnectionsPerNode + cachedNumberOfOutputs;
	}



	protected uint protectedCachedNumberOfNodes, cachedNumberOfFunctions, cachedNumberOfConnectionsPerNode;
	protected uint cachedNumberOfOutputs;
}






interface IRating {
	void rate(ChromosomeWithState chromosomeWithState);
}

import std.math : isNaN;

class TestRating : IRating {
	public final void rate(ChromosomeWithState chromosomeWithState) {
		ValueType result = chromosomeWithState.getValueOfOutput(0 /* of output 0 just for testing */);

		if( isNaN(result) ) {
			chromosomeWithState.rating = 0.0f;
			return;
		}

		chromosomeWithState.rating = cast(float)result;
	}
}



import std.stdio : writeln;

void main() {
	ChromosomeWithState[] chromosomesWithStates;
	ChromosomeWithState[] temporaryMutants; // all time allocated to speed up the algorithm

	uint numberOfGenerations = 500;


	Parameters parameters = new Parameters();
	parameters.numberOfInputs = 3;
	parameters.numberOfNodes = 10;
	parameters.numberOfInputsPerNode = 2; // depends on the suplied function to the Cartesian programming algorithm
	parameters.numberOfOutputs = 1;

	uint numberOfMutations = 5;
	uint numberOfCandidates = 5; // 4 + 1  evolutionary strategy 


	Context context = Context.make(parameters);

	context.input = [3.0f, 1.5f, 0.7f];

	IRating ratingImplementation = new TestRating();

	// we just maintain one candidate
	chromosomesWithStates ~= ChromosomeWithState.createFromChromosomeView(context.createRandomGenotypeView());
	
	foreach( i; 0..numberOfCandidates ) {
		// TODO< can be null entities, where literaly everyting is null >
		temporaryMutants ~= ChromosomeWithState.createFromChromosomeView(context.createRandomGenotypeView());
	}

	foreach( generation; 0..numberOfGenerations ) {
		writeln("generation ", generation);

		// copy to temporary which get mutated
		{
			foreach( iterationMutant; temporaryMutants ) {
				iterationMutant.copyChromosomeToDestination(iterationMutant);
			}
		}

		// mutate
		{
			foreach( iterationMutant; temporaryMutants ) {
				context.pointMutationOnGene(iterationMutant.view.getGenotypeViewOf(), numberOfMutations);
			}
		}

		// evaluation and rating
		{
			foreach( iterationChromosome; temporaryMutants ) {
				context.decodeChromosome(iterationChromosome);
				ratingImplementation.rate(iterationChromosome);
				writeln("rating of mutant = ", iterationChromosome.rating);
			}
		}

		// selection of best one
		// TODO
	}
}

