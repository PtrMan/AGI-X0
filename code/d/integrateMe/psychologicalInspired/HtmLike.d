// inspired by the bit storage of hierachical temporal model

import linopterixed.types.Bigint;

struct HtmLikeCell(uint multiplesOf64Bits) {
	Bigint!multiplesOf64Bits data;

	final bool isCompleteMatch(Bigint!multiplesOf64Bits other) {
		return data.equalTo(other);
	}

	final uint numberOfMatchingBits(Bigint!multiplesOf64Bits other) {
		// xor and count zero bits

		Bigint!multiplesOf64Bits xorResult;
		Bigint!multiplesOf64Bits.booleanXor(data, other, xorResult);

		// TODO< accelerate with counting of set bits and some subtration magic >

		uint matchingBitCounter = 0;
		foreach( bi; 0..multiplesOf64Bits*64 ) {
			if( !xorResult.getBit(bi) ) { // every false bit indicates a match
				matchingBitCounter++;
			}
		}
		return matchingBitCounter;
	}
}



import linopterixed.types.Bigint;