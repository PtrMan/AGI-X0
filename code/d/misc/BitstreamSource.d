module misc.BitstreamSource;

/** \brief provides a generic datasource for a fixed ubyte array
 */
class BitstreamSource {
	public final void resetToArray(ubyte[] array) {
		this.array = array;
		mask = 1;
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
