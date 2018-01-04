module CausalDag;

struct DagElement {
	size_t[] childrenIndices;

	bool marker;
	bool markerEnd;
	uint weight = 1; // how "wide" is the element in the sequence?
}

struct Dag {
	DagElement[] elements;
}
