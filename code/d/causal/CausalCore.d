module CausalCore;

import CausalDag;

void applyToAllChildrenUntilTerminationCriteria(Dag *dag, size_t entry, void delegate(DagElement *element, size_t elementIndex) apply, bool delegate(DagElement *element, size_t elementIndex) terminationCriteria) {
	size_t[] openList = [entry];

	bool[size_t] visited;

	for(;; !openList.isEmpty) {
		size_t currentIndex = openList.dequeue();

		if( currentIndex in visited ) {
			continue;
		}
		visited[currentIndex] = true;

		if( terminationCriteria(&dag.elements[currentIndex], currentIndex) ) {
			continue;
		}

		openList ~= dag.elements[currentIndex].childrenIndices;

		apply(&dag.elements[currentIndex], currentIndex);
	}
}

void markChildrenUntilEndMarker(Dag *dag, size_t entry) {
	void innerFnApply(DagElement *element, size_t elementIndex) {
		element.marker = true;
	}

	bool innerFnTerminationCriteria(DagElement *element, size_t elementIndex) {
		return element.markerEnd;
	}

	dag.applyToAllChildrenUntilTerminationCriteria(entry, &innerFnApply, &innerFnTerminationCriteria);
}

void unionAllFollowers(Dag *dag, ref bool[size_t] union_, size_t entry) {
	void innerFnApply(DagElement *element, size_t elementIndex) {
		union_[elementIndex] = true;
	}

	bool innerFnTerminationCriteria(DagElement *element, size_t elementIndex) {
		return false;
	}

	dag.applyToAllChildrenUntilTerminationCriteria(entry, &innerFnApply, &innerFnTerminationCriteria);
}

import std.random;

Type[] pickRandomN(Type)(Type[] arr, size_t n) {
	assert(arr.length >= n);

	Type[] result;

	foreach( i; 0..n ) {
		size_t index = uniform(0, arr.length);
		result ~= arr[index];
		arr.remove(index);
	}

	return result;
}

void markRandomFollowersAsEnd(Dag *dag, size_t[] entryIndices, size_t numberOfEndPoints) {
	bool[size_t] candidateIndicesMap;

	foreach( iterationEntryIndex; entryIndices ) {
		unionAllFollowers(dag, candidateIndicesMap, iterationEntryIndex);
	}

	size_t[] candidatesIndices = candidateIndicesMap.keys;
	foreach( iterationIndexToMark ; pickRandomN(candidatesIndices, numberOfEndPoints) ) {
		dag.elements[iterationIndexToMark].markerEnd = true;
	}
}

import std.algorithm.sorting : sort;
import std.algorithm.iteration : map, uniq;
import std.array : array;

// fuses all marked elements into one element in the result dag
Dag *fuseMarkedElements(Dag *inputDag) {
	Dag *result = new Dag;

	// points at the coresponding elements in the result dag from the inputDag
	// value can be -1 if there is no coresponding element in the result
	int[] indicesToDagElementsInResult;



	// build indicesToDagElementsInResult
	foreach( iterationElement; inputDag.elements ) {
		if( iterationElement.marker ) {
			indicesToDagElementsInResult ~= -1;			
		}
		else {
			indicesToDagElementsInResult ~= result.elements.length;
			result.elements ~= DagElement();
		}
	}

	size_t indexOfColapsedElement = result.elements.length;
	result.elements ~= DagElement(); // add the colapsed element
	
	size_t[] innerFnChildIndicesOfMarkedNodes() {
		size_t[] childIndicesOfMarkedNodes;

		foreach( iterationElement; inputDag.elements ) {
			if( iterationElement.marker ) {
				childIndicesOfMarkedNodes ~= iterationElement.childrenIndices;
			}
		}

		return childIndicesOfMarkedNodes.sort.uniq.array;
	}

	size_t[] remappedChildrenIndicesOfMarkedNodes = innerFnChildIndicesOfMarkedNodes()
		.map!(childrenIndex => indicesToDagElementsInResult[childrenIndex]) // rewire indices
		.map!(v => cast(size_t)v)
		.array.sort.uniq.array;
	result.elements[indexOfColapsedElement].childrenIndices = remappedChildrenIndicesOfMarkedNodes;
	

	// remap childrenIndices of elements which get transfered
	foreach( elementIndex, iterationElement; inputDag.elements ) {
		int remappedIndex = indicesToDagElementsInResult[elementIndex];

		if( remappedIndex == -1 ) {
			continue;
		}

		size_t[] remappedChildrenIndices = iterationElement.childrenIndices
			.map!(childrenIndex => indicesToDagElementsInResult[childrenIndex]) // rewire indices
			.map!(v => v == -1 ? indexOfColapsedElement : cast(size_t)v) // point all children to the colapsed node to the new collapsed node
			.array.sort.uniq // remove duplicates
            .array;


		result.elements[remappedIndex].childrenIndices = remappedChildrenIndices;

		// we need an backpointer to the elements which corresponds to the remapped element
		//result.elements[remappedIndex].parentIndex = elementIndex; // uncommented because we dont need it jet
	}

	return result;
}

// make sure the marked elements get fused to one new node at the end and the child indices get rewritten correctly
unittest {
	Dag *dag = new Dag;
	dag.elements ~= DagElement();
	dag.elements ~= DagElement();
	dag.elements ~= DagElement();
	
	dag.elements[0].childrenIndices = [2];
	dag.elements[0].marker = true;
	dag.elements[1].childrenIndices = [2];
	dag.elements[1].marker = true;

	Dag *resultDag = fuseMarkedElements(dag);

	assert(resultDag.elements.length == 2);

	assert(resultDag.elements[0].childrenIndices == []);
	assert(resultDag.elements[1].childrenIndices == [0]); // the collapsed node must point at the last element (which is now the first)
}

import std.range : take, repeat;
import std.algorithm.iteration : filter;

// classic/old energy/entropy minimization algorithm based on randomly sampling the causal chain.
// The working set is used to select the next element.

// The energy is calculated on the fly for an try.
// We take as the result the sequence with the minimal energy after a # of tries.
private size_t[] singleSample(const Dag *dag, out double energy) {
	static class ResultElement {
		size_t index;

		double weightSum; // sum of all weights of the elements before this element

		private const DagElement *dagElement;

		final @property uint weightOfReferencedElement() const pure {
			return dagElement.weight;
		}

		final this(size_t index, const DagElement *dagElement) {
			this.index = index;
			this.dagElement = dagElement;
		}
	}

	// calculate the set of the nodes which don't have any elements pointing to them, which is the definition of begin nodes
	size_t[] innerFnSetOfBeginNodes() {
		bool[] elementIsReferenced;
		elementIsReferenced.length = dag.elements.length;

		void markElementsAtIndices(const size_t[] indices) {
			foreach( iterationIndex; indices ) {
				elementIsReferenced[iterationIndex] = true;
			}
		}

		foreach(iterationElement; dag.elements) {
			markElementsAtIndices(iterationElement.childrenIndices);
		}

		{
			size_t[] result;
			foreach( i, iterationElementIsReferenced; elementIsReferenced ) {
				if( !iterationElementIsReferenced ) {
					result ~= i;
				}
			}
			return result;
		}
	}

	// these indices point for each dag-element at the position in the result array
	int[] backIndices = (-1).repeat().take(dag.elements.length).array;
	
	// is the count of remaining references for each element until it's zero, when it can be inserted into the result sequence (resultSequence)
	int[] remainingReferenceCounters;
	remainingReferenceCounters.length = dag.elements.length;

	// counts how many references point to each element (in the form of children indices)
	void innerFnCountReferences() {
		foreach( iterationElement; dag.elements ) {
			foreach( iterationChildIndex; iterationElement.childrenIndices ) {
				remainingReferenceCounters[iterationChildIndex]++;
			}
		}
	}

	void innerFnDecrementReferencesForElement(const DagElement dagElement) {
		foreach( iterationChildIndex; dagElement.childrenIndices ) {
			assert(remainingReferenceCounters[iterationChildIndex] > 0);
			remainingReferenceCounters[iterationChildIndex]--;
		}
	}
	void innerFnDecrementReferencesByElementIndex(size_t elementIndex) {
		innerFnDecrementReferencesForElement(dag.elements[elementIndex]);
	}


	innerFnCountReferences();

	ResultElement[] resultSequence;

	// invariant, elements in workingSet don't appear in alreadySampled
	size_t[] workingSet = innerFnSetOfBeginNodes();
	bool[size_t] alreadySampled;

	size_t innerFnPickCandidateAndRemoveFromWorkingset() {
		assert(workingSet.length > 0);
		size_t workingSetIndex = uniform(0, workingSet.length);
		assert(workingSetIndex < workingSet.length);

		size_t currentCandidate = workingSet[workingSetIndex];
		workingSet = workingSet.remove(workingSetIndex);
		return currentCandidate;
	}

	void innerFnAddToWorkingSet(const size_t[] indices) {
		workingSet ~= indices.filter!(v => !(v in alreadySampled)).array;
		workingSet = workingSet.sort.uniq.array;
	}

	for(; !workingSet.isEmpty ;) {
		size_t currentCandidateIndex = innerFnPickCandidateAndRemoveFromWorkingset();

		innerFnDecrementReferencesByElementIndex(currentCandidateIndex);

		assert(!(currentCandidateIndex in alreadySampled));
		alreadySampled[currentCandidateIndex] = true;

		backIndices[currentCandidateIndex] = resultSequence.length; // link back index
		resultSequence ~= new ResultElement(currentCandidateIndex, &dag.elements[currentCandidateIndex]);

		innerFnAddToWorkingSet(dag.elements[currentCandidateIndex].childrenIndices.filter!(v => remainingReferenceCounters[v] == 0).array);
	}

	double innerFnCalculateEnergy() {
		double energyResult = 0;

		foreach( iterationElementIndex, iterationElement; dag.elements ) {
			foreach( iterationChildIndex; iterationElement.childrenIndices ) {
				assert(backIndices[iterationElementIndex] != -1, "Calculating the distance of elements to elements not in the result sequence is invalid!");
				assert(backIndices[iterationChildIndex] != -1, "Calculating the distance of elements to elements not in the result sequence is invalid!");
				double distance = resultSequence[backIndices[iterationChildIndex]].weightSum - resultSequence[backIndices[iterationElementIndex]].weightSum;
				
				assert(distance > 0); // distance must always be positive, if it's not then it's not a DAG, which violates the causal relationship!
				energyResult += distance;
			}
		}

		return energyResult;
	}

	// (re)calculate the weightsum of all elements in the resultSequeuence
	void innerFnCalculateWeightSum() {
		double currentEnergySum = 0;

		foreach( ref sequenceElement; resultSequence ) {
			sequenceElement.weightSum = currentEnergySum;

			assert(sequenceElement.weightOfReferencedElement > 0);
			currentEnergySum += sequenceElement.weightOfReferencedElement;
		}
	}

	void innerFnDebug() {
		if( true ) {
			return;
		}

		import std.stdio;

		foreach( iterationBackIndex; backIndices ) {
			write(iterationBackIndex, " ");
		}

		writeln();
	}

	innerFnCalculateWeightSum();
	innerFnDebug();
	energy = innerFnCalculateEnergy();
	return resultSequence.map!(v => v.index).array;
}

import std.variant;

// context used for variables passed around between the calls to the sample function
// required because we should be able to "interrupt" tasks of energy/entropy minimization
struct SampleContext {
	private Variant[string] values;

	// accesses a value with the name as a type
	final @property Type value(Type, string key)() {
		return values[key].get!Type;
	}

	final @property Type value(Type, string key)(Type parameter) {
		values[key] = Variant(parameter);
		return parameter;
	}
}

struct SampleWithStrategyBatchArguments {
	Dag *dag;
	
	uint numberOfBatchSteps;
	SampleContext *sampleContext;
}

// resets the state of the batch
void sampleWithStrategyBatchSetup(ref SampleWithStrategyBatchArguments arguments) {
	arguments.sampleContext.value!(double, "bestEnergy") = 1e14; // TODO< double max >
	arguments.sampleContext.value!(size_t[], "bestSequence") = [];
}

// sample a number of times with an strategy to decide when to stop
// numberOfBatchSteps the number of steps of this batch with the SampleContext
void sampleWithStrategyBatch(ref SampleWithStrategyBatchArguments arguments, void delegate(size_t[] currentSequence, double energy, out bool continue_, SampleContext *context) continueStrategy, out double resultEnergy, out size_t[] resultSequence, out bool terminated) {
	terminated = false;

	foreach( i; 0..arguments.numberOfBatchSteps ) {
		bool continue_;
		double currentEnergy;
		size_t[] currentSequence = singleSample(arguments.dag, /*out*/ currentEnergy);

		continueStrategy(currentSequence, currentEnergy, /*out*/ continue_, arguments.sampleContext);
		if( !continue_ ) {
			resultEnergy = arguments.sampleContext.value!(double, "bestEnergy");
			resultSequence = arguments.sampleContext.value!(size_t[], "bestSequence");
			terminated = true;
			return;
		}

		if( currentEnergy < arguments.sampleContext.value!(double, "bestEnergy") ) {
			arguments.sampleContext.value!(double, "bestEnergy") = currentEnergy;
			arguments.sampleContext.value!(size_t[], "bestSequence") = currentSequence;
		}
	}
}

import std.algorithm.comparison : max, min;

void sampleWithResetCountingStrategyBatchSetup(ref SampleWithStrategyBatchArguments arguments, uint startRepetitionCount, uint resetRepetitionCounter, uint limitRepetitionCount) {
	sampleWithStrategyBatchSetup(arguments);

	// store the parameters
	arguments.sampleContext.value!(uint, "resetRepetitionCounter") = resetRepetitionCounter;
	arguments.sampleContext.value!(uint, "limitRepetitionCount") = limitRepetitionCount;

	// reset the used variables
	arguments.sampleContext.value!(uint, "counter") = 0;
	arguments.sampleContext.value!(uint, "remainingCounter") = min(startRepetitionCount, limitRepetitionCount); // min for consistency
}

void sampleWithResetCountingStrategyBatch(ref SampleWithStrategyBatchArguments arguments, out double resultEnergy, out size_t[] resultSequence, out bool terminated) {
	void innerFnStrategy(size_t[] currentSequence, double energy, out bool continue_, SampleContext *context) {
		if( energy < arguments.sampleContext.value!(double, "bestEnergy") ) {
			//arguments.sampleContext.value!(double, "bestEnergy") = energy;
			import std.stdio; writeln(arguments.sampleContext.value!(uint, "remainingCounter"));
			arguments.sampleContext.value!(uint, "remainingCounter") = max(arguments.sampleContext.value!(uint, "resetRepetitionCounter"), arguments.sampleContext.value!(uint, "remainingCounter"));
		}

		arguments.sampleContext.value!(uint, "counter") = arguments.sampleContext.value!(uint, "counter") + 1;
		arguments.sampleContext.value!(uint, "remainingCounter") = arguments.sampleContext.value!(uint, "remainingCounter") - 1;

		continue_ = (arguments.sampleContext.value!(uint, "remainingCounter") > 0) && (arguments.sampleContext.value!(uint, "counter") < arguments.sampleContext.value!(uint, "limitRepetitionCount"));
	}

	sampleWithStrategyBatch(arguments, &innerFnStrategy, /*out*/resultEnergy, /*out*/resultSequence, /*out*/terminated);
}





// debug
void debugDag(const Dag *dag) {
	foreach( i, iterationNode; dag.elements ) {
		import std.stdio;
		writeln("[",i,"]"," children=", iterationNode.childrenIndices);
	}
}



import std.algorithm.mutation : remove;

Type dequeue(Type)(ref Type[] arr) {
	Type result = arr[0];
	arr.remove(0);
	return result;
}

void enqueue(Type)(ref Type[] arr, Type element) {
	arr ~= element;
}


bool isEmpty(Type)(Type[] arr) {
	return arr.length == 0;
}