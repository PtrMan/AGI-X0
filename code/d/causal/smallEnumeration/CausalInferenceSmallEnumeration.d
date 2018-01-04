
// connections compactly encoded
struct Connections {
	bool[] flags;
}


// the algorithm first enumerates all cmbinations of nodes and (directed only to the right) edges
// then it tries to minimize the energy after the "causal inference" principle

// iteration 0
// first we start of with an node
//                 0

// (there are no edges to other nodes possible)

// iteration 1
// in the next iteration we add a new node and do all possible conections to the combinations with length-1 (the previous iteration)
// these are

//            1    0
//            1--->0

// then we minimize the energy(doesn't do anything in this case)

// iteration 2
//       2    1    0
//       2    1--->0

//       /---------V
//       2    1    0
//       2    1--->0
//       \---------^

//       2--->1    0
//       2--->1--->0

//       /---------V
//       2--->1    0
//       2--->1--->0
//       \---------^

// after minimization

//       2   1    0
//       2   1--->0

//       1   2--->0
//       2   1--->0
//       \--------^

//       2--->1    0
//       2--->1--->0

//       /---------V
//       2--->1    0
//       2--->1--->0
//       \---------^




//int childrenIndices[];

//int currentIndex;

struct Element {
	int index;
	Element*[] childrens;
}

// checks if the index "compareIndex" violates any constraints
// the constraint is that 
bool validPosition(int[] childrenIndices, int compareIndex) {
	childrenIndices.all!(v => compareIndex >= v);
}

bool violatesContraints(int[] childrenIndices, int compareIndex) {
	return validPosition(childrenIndices, compareIndex);
}

unittest {
	assert(!violatesContraints([1, 2], 0));
	assert(violatesContraints([1, 2], 1));
}

bool violatesContraints(Element *element, int compareIndex) {
	return violatesContraints(element.childrens.map!(v => v.index).array, compareIndex);
}

// calculate the binding-energy/entropy
// given the position of the element
int energy(Element *element, int index) {
	int[] childrenIndices = element.childrens.map!(v => v.index).array;
	assert(childrenIndices.all!(v => v >= 0));
	return childrenIndices.map!(v => v - index).sum;
}


// TODO< enumeration of possible combinations >

// TODO< energy minimization and rememering the best solution >

// TODO< storage for later >

