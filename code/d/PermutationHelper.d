module PermutationHelper;

import CgpException : CgpException;

class Permutation {
	uint[] srcIndices;
}

Permutation calcPermutation(uint[] a, uint[] b) {
	Permutation result = new Permutation();

	foreach( ib; 0..b.length ) {
		foreach( ia; 0..a.length ) {
			if( a[ia] == b[ib] ) {
				result.srcIndices ~= ia;
				break;
			}
		}
	}

	if( result.srcIndices.length != b.length ) {
		throw new CgpException("");
	}

	return result;
}