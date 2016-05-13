module CartesianGeneticProgramming;


import std.random : uniform;

alias uint GeneIndex;

alias double ValueType;


// used to dispatch the function and other global stuff
class Context {
	public final ValueType computeNode(ValueType a, ValueType b, uint functionIndex) {
		// TODO< code generated dispatch logic for each task >
		if( functionIndex == 0 ) { // stupid addition
			return a + b;
		}
		else if( functionIndex == 1 ) { // stupid multiplication
			return a * b;
		}
		assert(false, "Internal Error");
		return 0.0; // shouldn't ever be reached
	}

	// chromosome is the output
	/* not needed
	public final void translateGenotypeToChromosome(Genotype genotype, Chromosome chromosome) {
		foreach( functionGeneIndex; 0..numberOfFunctions ) {
			chromosome.nodes[functionGeneIndex].function_ = genotype.genes[functionGeneIndex];
		}

		foreach( connectionGeneIndex; 0..numberOfFunctions*numberOfConnectionsPerFunction ) {
			chromosome.nodes[connectionGeneIndex / numberOfConnectionsPerFunction].connections[connectionGeneIndex % numberOfConnectionsPerFunction] = genotype.genes[numberOfFunctions + connectionGeneIndex];
		}

		foreach( outputGeneIndex; 0..numberOfOutputs ) {
			chromosome.outputGeneIndices[outputGeneIndex] = genotype.genes[numberOfFunctions + numberOfFunctions*numberOfConnectionsPerFunction + outputGeneIndex];
		}
	}*/

	// for testing public
	// decoding CGP chromosomes
	// see
	// http://www.cartesiangp.co.uk/cgp-in-nutshell.pdf  page 8
	public final void decodeChromosome(ChromosomeWithState chromosome) {
		void resetToEvaluate() {
			chromosome.evalutionStates.resetToEvaluate();
		}

		void identifiyInitialnodesWhichNeedToBeEvaluated() {
			foreach( i; 0..globals.numberOfOutputs ) {
				chromosome.evalutionStates.statesOfNodes[chromosome.view.getOutputGene(i)].toEvaluate = true;
			}
		}

		void whichNodesAreUsed() {
			int p = chromosome.view.nodes.length-1;

			for(;;) {
				if( chromosome.evalutionStates.statesOfNodes[p].toEvaluate ) {
					assert( globals.numberOfInputsPerNode == 2 ); // we just have it implemented for two inputs
					uint x = chromosome.view.nodes[p].connections[0];
					uint y = chromosome.view.nodes[p].connections[1];
					chromosome.evalutionStates.statesOfNodes[x].toEvaluate = true;
					chromosome.evalutionStates.statesOfNodes[y].toEvaluate = true;
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
			foreach( i; 0..chromosome.view.nodes.length ) {
				if( chromosome.evalutionStates.statesOfNodes[i].toEvaluate ) {
					assert( globals.numberOfInputsPerNode == 2 ); // we just have it implemented for two inputs
					uint x = chromosome.view.nodes[i].connections[0];
					uint y = chromosome.view.nodes[i].connections[1];
					uint z = chromosome.view.nodes[i].function_;
					chromosome.evalutionStates.statesOfNodes[i].output =
						computeNode(chromosome.evalutionStates.statesOfNodes[x].output, chromosome.evalutionStates.statesOfNodes[y].output, z);
				}
			}
		}

		resetToEvaluate();
		identifiyInitialnodesWhichNeedToBeEvaluated();
		whichNodesAreUsed();
		loadInputDataValues();
		executeGraph();
	}


	public final @property uint numberOfFunctions() {
		return 2;
	}


	public ValueType[] input;

	protected static struct Globals {
		public uint numberOfInputs;
		public uint numberOfNodes; // without input and output, just real nodes
		public uint numberOfInputsPerNode;

		public uint numberOfOutputs;


		public final @property uint numberOfConnections() {
			return numberOfNodes * numberOfInputsPerNode;
		}
	}

	protected Globals protectedGlobals;

	public final @property Globals globals() {
		return protectedGlobals;
	}

	/+ see globals
	//public uint numberOfNodes, numberOfInputsOfNode;
	//public uint numberOfGlobalInputs; see globals 

	public final @property uint numberOfConnections() {
		return numberOfNodes*numberOfInputsOfNode;
	}

	public uint numberOfOutputs;
	+/

}




struct EvaluationState {
	bool toEvaluate;
	ValueType output;
}

class EvaluationStates {
	public EvaluationState[] statesOfNodes;

	public final void resetToEvaluate() {
		foreach( iterationState; statesOfNodes ) {
			iterationState.toEvaluate = false;
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

	protected Genotype genotypeViewOf;
}


class ChromosomeWithState {
	public ChromosomeView view;
	public EvaluationStates evalutionStates;

	public final void checkInvariant() {
		assert(view.nodes.length == evalutionStates.statesOfNodes.length);
	}
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

	public final this(uint cachedNumerOfNodes, uint cachedNumberOfFunctions, uint cachedNumerOfConnectionsPerNode, uint cachedNumerOfOutputs) {
		this.cachedNumerOfNodes = cachedNumerOfNodes;
		this.cachedNumberOfFunctions = cachedNumberOfFunctions;
		this.cachedNumerOfConnectionsPerNode = cachedNumerOfConnectionsPerNode;
		this.cachedNumerOfOutputs = cachedNumerOfOutputs;
	}

	///////////////////////////
	// used by chromosome views

	public final uint getFunctionOfNodeIndex(uint nodeIndex) {
		assert(nodeIndex < cachedNumerOfNodes);
		uint functionIndex = genes[getOffsetOfGenesForFunctions() + nodeIndex];
		assert(functionIndex < cachedNumberOfFunctions);
		return functionIndex;
	}

	public final void setFunctionOfNodeIndex(uint nodeIndex, uint newFunction) {
		assert(nodeIndex < cachedNumerOfNodes);
		assert(newFunction < cachedNumberOfFunctions);
		genes[getOffsetOfGenesForFunctions() + nodeIndex] = newFunction;
	}

	public final void setConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex, uint value) {
		assert(nodeIndex < cachedNumerOfNodes);
		assert(connectionIndex < cachedNumerOfConnectionsPerNode);
		genes[getOffsetOfGenesForConnections() + nodeIndex*cachedNumerOfConnectionsPerNode + connectionIndex] = value;
	}

	public final uint getConnectionOfNodeIndex(uint nodeIndex, uint connectionIndex) {
		assert(nodeIndex < cachedNumerOfNodes);
		assert(connectionIndex < cachedNumerOfConnectionsPerNode);
		return genes[getOffsetOfGenesForConnections() + nodeIndex*cachedNumerOfConnectionsPerNode + connectionIndex];
	}

	public final uint getOutputGene(uint outputIndex) {
		assert(outputIndex < cachedNumerOfOutputs);
		return genes[getOffsetOfGenesForOutputs() + outputIndex];
	}
	

	////////////////////////
	// used by mutation

	package final @property bool isFunctionGene(uint index) {
		assert(index < cachedNumerOfNodes + cachedNumerOfNodes*cachedNumerOfConnectionsPerNode + cachedNumerOfOutputs);
		return index >= getOffsetOfGenesForFunctions() && index < getOffsetOfGenesForConnections();
	}

	package final @property bool isConnectionGene(uint index) {
		assert(index < cachedNumerOfNodes + cachedNumerOfNodes*cachedNumerOfConnectionsPerNode + cachedNumerOfOutputs);
		return index >= getOffsetOfGenesForConnections() && index < getOffsetOfGenesForOutputs();
	}

	package final @property bool isOutputGene(uint index) {
		assert(index < cachedNumerOfNodes + cachedNumerOfNodes*cachedNumerOfConnectionsPerNode + cachedNumerOfOutputs);
		return index >= getOffsetOfGenesForOutputs();
	}

	///////////////////////////
	// calculate the offsets of the different sections of the genome

	protected final uint getOffsetOfGenesForFunctions() {
		return 0;
	}

	protected final uint getOffsetOfGenesForConnections() {
		return cachedNumerOfNodes;
	}

	protected final uint getOffsetOfGenesForOutputs() {
		return cachedNumerOfNodes + cachedNumerOfNodes*cachedNumerOfConnectionsPerNode;
	}



	protected uint cachedNumerOfNodes, cachedNumberOfFunctions, cachedNumerOfConnectionsPerNode; // new
	protected uint cachedNumerOfOutputs; // new
}



// see
// http://www.cartesiangp.co.uk/cgp-in-nutshell.pdf  page 9
void pointMutationOnGene(Genotype genotype, Context context, uint numberOfMutations) {
	foreach( mutationIterator; 0..numberOfMutations ) {
		uint geneIndex = uniform!"[)"(0, genotype.genes.length);

		if( genotype.genes[geneIndex].isFunctionGene ) {
			// change gene to a randomly chosen new valid function
			genotype.genes[geneIndex] = uniform!"[)"(0, context.numberOfFunctions);
		}
		else if( genotype.genes[geneIndex].isConnectionGene ) {
			// change gene to randomly chosen new valid connection
			genotype.genes[geneIndex] = uniform!"[)"(0, context.globals.numberOfConnections);
		}
		else if( genotype.genes[geneIndex].isOutputGene ) {
			// change gene to a new valid output connection
			genotype.genes[geneIndex] = uniform!"[)"(0, context.globals.numberOfConnections);
		}
		else {
			assert(false, "Unreachable!");
		}
	}
}

// TODO< evolution strategy >


void main() {

}








/*
class Node {
	public bool toEvaluate = false;

	public uint[] connections;
	public uint function_;

	public ValueType output;
}

class Chromosome {
	public Node[] nodes;
	public GeneIndex[] outputGeneIndices;

	public final void resetToEvaluate() {
		foreach( iterationNode; nodes ) {
			iterationNode.toEvaluate = false;
		}
	}

	public final GeneIndex getOutputGene(uint index) {
		return outputGeneIndices[index];
	}

	public final  @property uint numberOfOutputGenes() {
		return outputGeneIndices.length;
	}
}
*/