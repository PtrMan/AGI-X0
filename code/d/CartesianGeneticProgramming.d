module CartesianGeneticProgramming;

import TokenOperators;


import std.random : uniform;

alias uint GeneIndex;

alias TextIndexOrTupleValue ValueType;


class CgpException : Exception {
    this (string msg) {
        super(msg) ;
    }
}

class Parameters {
	public uint numberOfInputs;
	public uint numberOfNodes; // without input and output, just real nodes
	
	public uint numberOfOutputs;

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

// used to dispatch the function and other global stuff
class Context {
	public static Context make(Parameters parameters, IOperatorInstancePrototype!ValueType operatorInstancePrototype) {
		Context result = new Context();
		result.protectedGlobals.numberOfInputs = parameters.numberOfInputs;
		result.protectedGlobals.numberOfNodes = parameters.numberOfNodes;
		result.protectedGlobals.numberOfOutputs = parameters.numberOfOutputs;

		result.operatorInstancePrototype = operatorInstancePrototype;

		return result;
	}

	protected final this() {
		protectedGlobals = new Globals();
	}

	/*
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
	*/

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
				writeln(chromosome.view.getOutputGene(i));
				ConnectionAdress.translateIndexToTypeAndIndexOfType(chromosome.view.getOutputGene(i), connectionType, translatedConnectionIndex, globals.numberOfInputs);

				if( connectionType == ConnectionAdress.EnumType.NODE ) {
					chromosome.evalutionStates.statesOfNodes[translatedConnectionIndex].toEvaluate = true;
				}
			}
		}

		void whichNodesAreUsed() {
			Genotype genotype = chromosome.view.getGenotypeViewOf();

			int p = chromosome.view.nodes.length-1;

			for(;;) {
				if( chromosome.evalutionStates.statesOfNodes[p].toEvaluate ) {
					foreach( inputConnectionIndex; 0..genotype.operatorInstances[p].getNumberOfInputConnections() ) {
						uint connectionIndex = genotype.operatorInstances[p].getInputGeneIndexForConnection(inputConnectionIndex);
						
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

		void loadInputDataValues() {
			import std.stdio;
			writeln("TRACE ", "chromosome.evalutionStates.statesOfNodes.length ", chromosome.evalutionStates.statesOfNodes.length);

			foreach( i; 0..protectedGlobals.numberOfInputs ) {
				chromosome.evalutionStates.statesOfNodes[i].output = input[i];
			}
		}

		void transcribeToOperatorsForNodes() {
			Genotype genotype = chromosome.view.getGenotypeViewOf();
			genotype.transcribeToOperatorsForNodes();
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
					throw new CgpException("Unreachable");
				}
			}



			Genotype genotype = chromosome.view.getGenotypeViewOf();
			foreach( i; 0..chromosome.view.nodes.length ) {
				if( !chromosome.evalutionStates.statesOfNodes[i].toEvaluate ) {
					continue;
				}
				
				ValueType[] inputs;

				foreach( inputConnectionIndex; 0..genotype.operatorInstances[i].getNumberOfInputConnections() ) {
					uint inputGeneIndex = genotype.operatorInstances[i].getInputGeneIndexForConnection(inputConnectionIndex);
					inputs ~= getValueBySourceIndex(inputGeneIndex);
				}

				chromosome.evalutionStates.statesOfNodes[i].output = genotype.operatorInstances[i].calculateResult(inputs);				
			}
		}

		resetOutputValues();
		resetToEvaluate();
		identifiyInitialnodesWhichNeedToBeEvaluated();
		whichNodesAreUsed();
		loadInputDataValues();
		transcribeToOperatorsForNodes();
		executeGraph();
	}

	public final ChromosomeView createRandomGenotypeView() {
		return ChromosomeView.makeViewOfGenotype(createRandomGenotype());
	}

	protected final Genotype createRandomGenotype() {
		Genotype randomGenotype = new Genotype(operatorInstancePrototype, globals.numberOfNodes, globals.numberOfInputs, globals.numberOfOutputs);
		
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
		if( genotype.genes[geneIndex].isGenericGene ) {
			// change gene to a randomly chosen new valid function
			genotype.genes[geneIndex] = uniform!"[)"(0, uint.max);
		}
		else if( genotype.genes[geneIndex].isOutputGene ) {
			// change gene to a new valid output connection
			genotype.genes[geneIndex] = globals.numberOfInputs + uniform!"[)"(0, globals.numberOfNodes);
		}
		else {
			throw new CgpException("Unreachable");
			assert(false, "Unreachable!");
		}
	}


	public ValueType[] input;

	public IOperatorInstancePrototype!ValueType operatorInstancePrototype;

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
class GenericGeneView {
	public final this(uint nodeIndex, ChromosomeView chromosomeView) {
		this.nodeIndex = nodeIndex;
		this.chromosomeView = chromosomeView;
	}

	public final uint opIndexAssign(uint value, size_t connectionIndex) {
		chromosomeView.setGenericGeneOfNodeByIndex(nodeIndex, connectionIndex, value);
		return value;
	}

	public final uint opIndex(size_t connectionIndex) {
		return chromosomeView.getGenericGeneOfNodeIndex(nodeIndex, connectionIndex);
	}

	protected uint nodeIndex;
	protected ChromosomeView chromosomeView;
}

// view to a Chromosomeview and the NodeStateVector
class NodeView {
	public final this(uint nodeIndex, ChromosomeView chromosomeView) {
		this.nodeIndex = nodeIndex;
		this.chromosomeView = chromosomeView;
		genericGeneView = new GenericGeneView(nodeIndex, chromosomeView);
	}

	public final @property GenericGeneView genericGenes() {
		return genericGeneView;
	}

	protected uint nodeIndex;
	protected GenericGeneView genericGeneView;
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

	public final void setGenericGeneOfNodeByIndex(uint nodeIndex, uint connectionIndex, uint value) {
		genotypeViewOf.setGenericGeneOfNodeByIndex(nodeIndex, connectionIndex, value);
	}

	public final uint getGenericGeneOfNodeIndex(uint nodeIndex, uint connectionIndex) {
		return genotypeViewOf.getGenericGeneOfNodeIndex(nodeIndex, connectionIndex);
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

	public static ChromosomeWithState createFromChromosomeView(ChromosomeView view, uint numberOfInputs) {
		ChromosomeWithState result = new ChromosomeWithState();
		result.view = view;
		result.evalutionStates = EvaluationStates.createBySize(view.nodes.length + numberOfInputs);
		
		result.cachedNumberOfInputs = numberOfInputs;
		return result;
	}

	// helper
	public final ValueType getValueOfOutput(uint outputIndex) {
		ConnectionAdress.EnumType connectionType;
		uint translatedConnectionIndex;

		import std.stdio;
		writeln("view.getOutputGene(outputIndex) ", view.getOutputGene(outputIndex));

		ConnectionAdress.translateIndexToTypeAndIndexOfType(view.getOutputGene(outputIndex), connectionType, translatedConnectionIndex, cachedNumberOfInputs);

		if( connectionType == ConnectionAdress.EnumType.NODE ) {
			return evalutionStates.statesOfNodes[translatedConnectionIndex].output;
		}

		throw new CgpException("Internal error: Output node is input node, is not allowed");
	}

	public final void copyChromosomeToDestination(ChromosomeWithState destination) {
		foreach( nodeIndex; 0..view.nodes.length ) {
			// we can't assign the nodes directly because te nodes are views and it would mess up everything completly

			foreach( connectionIndex; 0..view.getGenotypeViewOf().operatorInstances[nodeIndex].getNumberOfInputConnections() ) {
				destination.view.nodes[nodeIndex].genericGenes[connectionIndex] = view.nodes[nodeIndex].genericGenes[connectionIndex];
			}
		}
	}

	public final void checkInvariant() {
		assert(view.nodes.length == evalutionStates.statesOfNodes.length);
	}

	public float rating = 0.0f;

	protected uint cachedNumberOfInputs;
}

struct Gene {
	public final this(Genotype parentGenotype, uint indexInGenotype, uint value) {
		this.parentGenotype = parentGenotype;
		this.indexInGenotype = indexInGenotype;
		this.value = value;
	}

	public final @property bool isGenericGene() {
		return parentGenotype.isGenericGene(indexInGenotype);
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

	public final this(IOperatorInstancePrototype!ValueType operatorInstancePrototype, uint cachedNumberOfNodes, uint cachedNumberOfInputs, uint cachedNumberOfOutputs) {
		this.cachedNumberOfInputs = cachedNumberOfInputs;

		this.protectedCachedNumberOfNodes = cachedNumberOfNodes;
		this.cachedNumberOfOutputs = cachedNumberOfOutputs;

		this.operatorInstances.length = cachedNumberOfNodes;
		
		createOperatorInstances(operatorInstancePrototype);
		allocateGenes();
	}

	protected final void allocateGenes() {
		genes.length = getNumberOfGenes();
		foreach( i; 0..genes.length ) {
			genes[i].parentGenotype = this;
			genes[i].indexInGenotype = i;
		}
	}

	protected final void createOperatorInstances(IOperatorInstancePrototype!ValueType operatorInstancePrototype) {
		import std.stdio;
		writeln("TRACE ", "createOperatorInstances() ", "protectedCachedNumberOfNodes ", protectedCachedNumberOfNodes);

		foreach( i; 0..protectedCachedNumberOfNodes ) {
			operatorInstances[i] = operatorInstancePrototype.createInstance();
		}
	}

	public final @property uint numberOfConnectionsPerNode() {
		// ASSUMTION  all operators require the same number of genes
		return operatorInstances[0].getGeneSliceWidth();
	}

	public final @property uint numberOfNodes() {
		return protectedCachedNumberOfNodes;
	}

	public IOperatorInstance!ValueType[] operatorInstances;

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

		uint sliceIndex = 0;

		foreach( i; 0..protectedCachedNumberOfNodes ) {
			uint[] slicedGene = convertGenesToUint(genes[sliceIndex  +cachedNumberOfInputs..sliceIndex  +cachedNumberOfInputs+operatorInstances[i].getGeneSliceWidth()]);

			assert(slicedGene.length == operatorInstances[i].getGeneSliceWidth());

			operatorInstances[i].decodeSlicedGene(slicedGene);
			sliceIndex += slicedGene.length;
		}
	}


	///////////////////////////
	// used by chromosome views

	
	/*
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
	}*/

	public final void setGenericGeneOfNodeByIndex(uint nodeIndex, uint genericGeneIndex, uint value) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		assert(genericGeneIndex < numberOfConnectionsPerNode);
		genes[getOffsetOfGenesForGenericGenes() + nodeIndex*numberOfConnectionsPerNode + genericGeneIndex] = value;
	}

	public final uint getGenericGeneOfNodeIndex(uint nodeIndex, uint genericGeneIndex) {
		assert(nodeIndex < protectedCachedNumberOfNodes);
		assert(genericGeneIndex < numberOfConnectionsPerNode);
		return genes[getOffsetOfGenesForGenericGenes() + nodeIndex*numberOfConnectionsPerNode + genericGeneIndex];
	}

	public final uint getOutputGene(uint outputIndex) {
		assert(outputIndex < cachedNumberOfOutputs);
		return genes[getOffsetOfGenesForOutputs() + outputIndex];
	}
	

	////////////////////////
	// used by mutation

	package final @property bool isGenericGene(uint index) {
		assert(index < protectedCachedNumberOfNodes*numberOfConnectionsPerNode + cachedNumberOfOutputs);
		return index >= getOffsetOfGenesForGenericGenes() && index < getOffsetOfGenesForOutputs();
	}

	package final @property bool isOutputGene(uint index) {
		assert(index < protectedCachedNumberOfNodes*numberOfConnectionsPerNode + cachedNumberOfOutputs);
		return index >= getOffsetOfGenesForOutputs();
	}

	///////////////////////////
	// calculate the offsets of the different sections of the genome

	
	protected final uint getOffsetOfGenesForGenericGenes() {
		return 0;
	}

	protected final uint getOffsetOfGenesForOutputs() {
		return protectedCachedNumberOfNodes*numberOfConnectionsPerNode;
	}

	protected final uint getNumberOfGenes() {
		return protectedCachedNumberOfNodes*numberOfConnectionsPerNode + cachedNumberOfOutputs;
	}

	protected uint protectedCachedNumberOfNodes, cachedNumberOfOutputs, cachedNumberOfInputs;
}






interface IRating {
	void rate(ChromosomeWithState chromosomeWithState);
}

class TestRating : IRating {
	public final void rate(ChromosomeWithState chromosomeWithState) {
		chromosomeWithState.rating = 0.0f;

		ValueType result = chromosomeWithState.getValueOfOutput(0 /* of output 0 just for testing */);
		
		if( !result.isSet ) {
			return;
		}

		import std.stdio;
		writeln("MARKER rate()");

		if( result.tuple[0] == 0 ) { // checks for "i"
			chromosomeWithState.rating += 1.0f;
		}

		if( result.tuple[1] == 2 ) { // checks for "tired"
			chromosomeWithState.rating += 1.0f;
		}
	}
}



import std.stdio : writeln;

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

	// i am tired
	// i am very tired
	// it is very rainy
	// it is rainy
	uint numberOfTokens = 9;
	uint readWidth = 3;

	IOperatorInstancePrototype!ValueType operatorInstancePrototype = new TokenMatcherOperatorInstancePrototype(readWidth, numberOfTokens);

	ChromosomeWithState[] chromosomesWithStates;
	ChromosomeWithState[] temporaryMutants; // all time allocated to speed up the algorithm

	uint numberOfGenerations = 500000;


	Parameters parameters = new Parameters();
	parameters.numberOfInputs = 1;
	parameters.numberOfNodes = 1;
	parameters.numberOfOutputs = 1;

	uint numberOfMutations = 1;
	uint numberOfCandidates = 5; // 4 + 1  evolutionary strategy 


	Context context = Context.make(parameters, operatorInstancePrototype);

	context.input = [TextIndexOrTupleValue.makeTuple([0, 1, 2])]; // i am tired
	// TODO< multiple, with "i am very tired" >

	IRating ratingImplementation = new TestRating();

	// we just maintain one candidate
	chromosomesWithStates ~= ChromosomeWithState.createFromChromosomeView(context.createRandomGenotypeView(), parameters.numberOfInputs);
	
	foreach( i; 0..numberOfCandidates ) {
		// TODO< can be null entities, where literaly everyting is null >
		temporaryMutants ~= ChromosomeWithState.createFromChromosomeView(context.createRandomGenotypeView(), parameters.numberOfInputs);
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
		{
			int bestPseudoIndex = -1;
			float bestRating = chromosomesWithStates[0].rating;

			int pseudoIndex = 0;
			foreach( iterationChromosome; temporaryMutants ) {
				// equal is important here to allow for genetic drift!
				if( iterationChromosome.rating >= bestRating ) {
					bestPseudoIndex = pseudoIndex;
					bestRating = iterationChromosome.rating;
				}

				pseudoIndex++;
			}

			writeln("best rating = ", bestRating);

			if( bestPseudoIndex != -1 ) {
				temporaryMutants[bestPseudoIndex].copyChromosomeToDestination(chromosomesWithStates[0]);
				chromosomesWithStates[0].rating = temporaryMutants[bestPseudoIndex].rating;
			}
		}
	}
}

