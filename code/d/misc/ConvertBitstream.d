module misc.ConvertBitstream;

ubyte[] toUbyte(bool[] bitVector) {
	ubyte[] result;
	result.length = bitVector.length/8 + (((bitVector.length % 8) == 0) ? 0 : 1);
	foreach( i; 0..bitVector.length ) {
		if( bitVector[i] ) {
			result[i/8] |= (1 << (i % 8));
		}
	}
	return result;
}
