module causal.Datastructures;

struct Decoration {
	uint overlapCounter; // used to find the elements of the overlaps
	int regionMarker; // used for marking elements in different regions, used for swapping elements
}

struct Element {
	Element*[] childrens;
	Element*[] up;

	size_t index;

	// decoration
	Decoration decoration;
}
