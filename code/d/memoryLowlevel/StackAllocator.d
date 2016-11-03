module memoryLowlevel.StackAllocator;

struct StackAllocator(uint SizeInBits, Type) {
	private Type[1 << (SizeInBits-1)] arr;
	private size_t currentNextIndex;

	final void reset() {
		currentNextIndex = 0;
	}

	final void append(Type value, out bool success) {
		success = false;
		assert( currentNextIndex <= (1 << SizeInBits));
		bool overflow = currentNextIndex == (1 << SizeInBits);
		if( overflow ) {
			return;
		}

		arr[currentNextIndex++] = value;

		success = false;
	}

	final void push(Type value, out bool success) {
		append(value, /*out*/success);
	}

	final @property size_t length() pure {
		return currentNextIndex;
	}

	final Type getAtIndex(size_t index) {
		assert(index < currentNextIndex);
		return arr[index];
	}
}
