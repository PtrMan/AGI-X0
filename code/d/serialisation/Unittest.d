import serialisation.BitstreamReader;
import serialisation.BitstreamWriter;

version(unittest) {
	class BitstreamDestination {
		public final this() {
			flush();
		}

		public final void addBoolean(bool value) {
			array ~= value;
		}

		
		public final @property ubyte[] dataAsUbyte() {
			assert(false, "not implemented");
			return [];
		}
		

		public final @property size_t length() {
			return array.length;
		}

		public final void flush() {
			array.length = 0;
		}

		public bool[] array;
	}

	class BitstreamSource {
		//public final void resetToArray(ubyte[] array) {
		//	this.array = array;
		//	mask = 1;
		//}

		public final bool readNextBit() {
			bool result = array[index];
			index++;
			return result;
		}

		public final bool isValid(uint numberOfBits = 1) {
			return index + numberOfBits <= array.length;
		}

		public bool[] array;
		public uint index;
	}

}

// test 
unittest {
	foreach( value; 0..(1<<16)-1) {
		import std.stdio : writeln;
		writeln("value=", value);

		BitstreamDestination destination = new BitstreamDestination();
		BitstreamWriter!BitstreamDestination bitstreamWriter = new BitstreamWriter!BitstreamDestination(destination);
		bool successChained = true;
		bitstreamWriter.addUint_2__4_8_12_16(value, successChained);
		assert(successChained);

		BitstreamSource source = new BitstreamSource();
		source.array = destination.array;
		source.index = 0;
		BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(source);
		uint readValue = bitstreamReader.getUint_2__4_8_12_16(successChained);
		assert(successChained);

		// end has to be reached!
		assert(!source.isValid());

		assert(readValue == value);
	}
}

unittest {
	foreach( value; 0..(1<<16)-1) {
		import std.stdio : writeln;
		writeln("value=", value);

		BitstreamDestination destination = new BitstreamDestination();
		BitstreamWriter!BitstreamDestination bitstreamWriter = new BitstreamWriter!BitstreamDestination(destination);
		bool successChained = true;
		bitstreamWriter.addUint__n(value, 16, successChained);
		assert(successChained);

		BitstreamSource source = new BitstreamSource();
		source.array = destination.array;
		source.index = 0;
		BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(source);
		uint readValue = bitstreamReader.getUint__n(16, successChained);
		assert(successChained);

		// end has to be reached!
		assert(!source.isValid());

		assert(readValue == value);
	}
}