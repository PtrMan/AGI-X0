// see paper "Efficient algorithms with neural network behavior" page 16

// array of buckets

// can be used for O(n) sorting if the elements are evenly distributed


struct ArrayOfBucketElement(Type) {
	Type[] content; // TODO< fast single linked array >
}

struct ArrayOfBucket(Type) {
	ArrayOfBucketElement!Type[] elements;

	final void clear() {
		foreach( ref iterationElement; elements ) {
			iterationElement.content.length = 0;
		}
	}
}

// inserts elements into the array
// can be used for O(n) sorting if the elements are evenly distributed
void insertLinearInto(ComparableType, ArrayOfBucketContentType)(ArrayOfBucket!ArrayContentType *arr, ComparableType min, ComparableType max, ComparableType[] elements) {
	assert(arr.elements.length > 0);

	ComparableType diff = max - min;
	assert(diff > 0);
	ComparableType invDiff = 1.0 / diff;

	foreach( iterationElement; elements ) {
		int index = cast(int)(((iterationElement - min) * invDiff) * cast(ComparableType)arr.elements.length);
		assert(index >= 0);
		assert(index < arr.elements.length);
		arr.elements[index].content ~= iterationElement;
	}
}


// see paper "Efficient algorithms with neural network behavior" page 22
// "partial summation"

// we sum up all elements up to the index in an n dimensional space

interface IGridAccessor(Type) {
	Type access(uint x, uint y);
}

// see paper "Efficient algorithms with neural network behavior" page 22
// calculates the partial sum
Type calcPartialSum(Type)(uint i, uint j, IGridAccessor!Type accessor) {
	Type sum = 0;

	foreach( l; 0..i ) {
		foreach( k; 0..j ) {
			sum += accessor.access(l, k);
		}
	}

	return sum;
}

// see paper "Efficient algorithms with neural network behavior" page 22
// calculates the partial sum
Type calcPartialSumOverArray(Type)(Type[][] arr, uint i1, uint j1, uint i2, uint j2) {
	Type innerFnAccessAt(int i, int j) {
		return arr[i][j];
	}

	return innerFnAccessAt(i2,j2) - innerFnAccessAt(i2,j1) + innerFnAccessAt(i1, j1) - innerFnAccessAt(i1, j2);
}

import NearestNeighbors;

// see paper "Efficient algorithms with neural network behavior" page 29
// estimation of the density with the "volume method"

private double volumeMethodEstimation(uint n, uint N, double Vn) {
	return (cast(double)n - 1.0f) / (cast(double)N * Vn);
}

import NDimensionalHypersphere;

// specialization of volumeMethodEstimation for radius and dimensionality
double volumeMethodEstimationForRadiusAndDimensionality(uint n, uint N, double radius, uint dimensions) {
	double volume = volumeHypersphere(dimensions, radius);
	return volumeMethodEstimation(n, N, volume);
}



