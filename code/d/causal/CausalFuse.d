module CausalFuse;

import CausalDag;

/**
 * functionality for fusing multiple DAG elements into a single one, this produces a new DAG
 */

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
