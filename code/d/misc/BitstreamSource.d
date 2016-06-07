module misc.BitstreamSource;

/** \brief provides a generic datasource for a fixed ubyte array
 */
class BitstreamSource {
	public final void resetToArray(ubyte[] array) {
		this.array = array;
		mask = 1;
	}

	public final void resetToArray(bool[] boolArray) {
		this.array.length = (boolArray.length / 8) + (((boolArray.length % 8) == 0) ? 0 : 1);
		
		// TODO< set to zero >

		size_t i = 0;
		foreach( iterationBool; boolArray ) {
			if( iterationBool ) {
				this.array[i/8] |= (1 << (i % 8));
			}

			i++;
		}
	}

	public final bool readNextBit() {
		bool result = (array[arrayByteIndex] & mask) != 0;
		mask <<= 1;
		if( mask == (1 << 8) ) {
			mask = 1;
			arrayByteIndex++;
		}

		return result;
	}

	public final bool isValid(uint numberOfBits = 1) {
		// TODO< implement better >
		return arrayByteIndex <= array.length;
	}

	protected ubyte[] array;
	protected size_t arrayByteIndex;
	protected uint mask;
}
