module optimisation.cartesianGeneticProgramming.Permutation;

import optimisation.cartesianGeneticProgramming.CgpException : CgpException;

class Permutation {
	uint[] srcIndices;

	// TODO< generics instead of uint parameter and result >
	public final uint[] apply(uint[] argument, out bool success) {
		success = false;
		uint[] result;

		foreach( srcIndex; srcIndices ) {
			if( srcIndex >= argument.length ) {
				// failed to match because the argument list is too short
				return [];
			}

			result ~= argument[srcIndex];
		}

		success = true;
		return result;
	}

	public static Permutation calcPermutation(uint[] a, uint[] b) {
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
}
