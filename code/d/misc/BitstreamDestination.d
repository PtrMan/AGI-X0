module misc.BitstreamDestination;

class BitstreamDestination {
	public final this() {
		flush();
	}

	public final void addBoolean(bool value) {
		bool appendNewByte = (numberOfBits % 8) == 0;
		if( appendNewByte ) {
			array ~= 0;
		}

		if( value ) {
			array[$-1] |= mask;
		}
		

		mask <<= 1;
		if( mask == (1 << 8) ) {
			mask = 1;
		}

		numberOfBits++;
	}

	public final @property ubyte[] dataAsUbyte() {
		return array;
	}

	public final @property size_t length() {
		return numberOfBits;
	}

	public final void flush() {
		array.length = 0;
		mask = 1;
		numberOfBits = 0;
	}



	protected ubyte[] array;
	protected uint mask;
	protected size_t numberOfBits;
}
