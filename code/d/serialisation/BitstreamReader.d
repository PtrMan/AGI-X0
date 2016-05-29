module serialisation.BitstreamReader;

/** \brief 
 *
 */
class BitstreamReader(BitstreamSourceImplementation) {
	protected final this() {
	}

	public final this(BitstreamSourceImplementation bitstreamSource) {
		this.bitstreamSource = bitstreamSource;
	}

	/** \brief returns the next Bool
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public bool getBoolean(ref bool successChained) {
		bool result;

		if( !successChained ) {
			return false;
		}

		if( isEndReached ) {
			successChained = false;
			return false;
		}

		// just to make sure no runtime error happens
		if( !bitstreamSource.isValid ) {
			successChained = false;
			return false;
		}

		return bitstreamSource.readNextBit();
	}

	/** \brief returns a number of up to 16 bits wide
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public uint getUint_2__4_8_12_16(ref bool successChained) {
		if( !successChained ) {
			return 0;
		}

		if( isEndReached ) {
			successChained = false;
			return 0;
		}

		if( !bitstreamSource.isValid(2) ) {
			successChained = false;
			return 0;
		}

		uint numberOf4Bits = 0;
		if( bitstreamSource.readNextBit() ) {
			numberOf4Bits |= 2;
		}

		if( bitstreamSource.readNextBit() ) {
			numberOf4Bits |= 1;
		}

		uint numberOfBits = (numberOf4Bits+1) * 4;

		assert(numberOfBits > 0, "numberOfBits must be greater than 0!");

		if( !bitstreamSource.isValid(numberOfBits) ) {
			successChained = false;
			return 0;
		}


		uint result = 0;
		foreach( bitI; 0..numberOfBits ) {
			if( bitstreamSource.readNextBit() ) {
				result |= (1<<bitI);
			}
		}

		return result;
	}

   // TOUNITTEST
   // TODO< rename and use string >
   /** \brief returns a fixed ength string with an 8 bit encoding
    *
    * \param numberOfSigns ...
    * \param successChained ...
    * \return ...
    */
    /+
	final public char[] getString8FixedLength(uint numberOfSigns, ref bool successChained) {
      char[] Return;
      uint i;

      if( !successChained )
      {
         return [];
      }

      for( i = 0; i < numberOfSigns; i++ )
      {
         Return ~= cast(char)getUint__n(8, successChained);
      }

      if( !successChained )
      {
         return [];
      }

      return Return;
   }
   +/

	/** \brief returns a number of 'Bits' bits wide
	 *  \param numberOfBits How many bits should be readed
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public uint getUint__n(uint numberOfBits, ref bool successChained) {
		// only assert because it is in the gamecode very inpropable that something like this happens
		assert(numberOfBits <= 32, "bits need to be less or equal to 32");

		uint result = 0;

		if( !successChained ) {
			return 0;
		}

		if( numberOfBits == 0 ) {
			successChained = false;
			return 0;
		}

		if( !bitstreamSource.isValid(numberOfBits) ) {
			successChained = false;
			return 0;
		}

		foreach( bitI; 0..numberOfBits ) {
			if( bitstreamSource.readNextBit() ) {
				result |= (1 << bitI);
			}
		}

		return result;
	}

   
	/** \brief tries to return a fixed point value with n Bits
	 * 
	 *  \param bits How many bits does the value have (inclusive sign bit)
	 *  \param Range Maximal range of this number
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public float getFixed__n(uint bits, float range, ref bool successChained) {
		assert(bits > 2, "bits need to be bigger than 2");

		// only assert because it is in the gamecode very inpropable that something like this happens
		assert(bits <= 32, "bits need to be less or equal to 32");

		assert(range > 0.0f, "range must be greater than 0.0");

		if( !successChained ) {
			return 0.0f;
		}

		bool sign = getBoolean(successChained);

		if( !successChained ) {
			return 0.0f;
		}

		int valueInt = getUint__n(bits-1, successChained);

		if( !successChained ) {
			return 0.0f;
		}

		if( sign ) {
			valueInt *= -1;
		}

		return (cast(float)valueInt / cast(float)(1 << (bits-1))) * range;
	}

	// TODO< unit tests >
	/** \brief tries to return 'Bits' bits
	 * 
	 *  \param bits How many bits should be read?
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public bool[] getBits(uint bits, ref bool successChained) {
		bool[] result;

		if( !successChained ) {
			return result;
		}

		if( !bitstreamSource.isValid(bits)  ) {
			successChained = false;

			return result;
		}

		uint remaining = bits;

		for(;;) {
			if( remaining == 0 ) {
				break;
			}

			result ~= bitstreamSource.readNextBit();

			remaining--;
		}


		return result;
	}


	/** \brief returns a 32 bit float
	 *  \param successChained will be false if the function failed
	 *  \return ...
	 */
	final public float getFloat32(ref bool successChained) {
		if( !successChained ) {
			return 0.0f;
		}

		uint valueUint = getUint__n(32, successChained);

		if( !successChained ) {
			return 0.0f;
		}

		return *(cast(float*)&valueUint);
	}

   
	/** \brief returns a half precision float value
	 *
	 * \param successChained will be false if the function failed
	 * \return ...
	 */
	final public float getFloat16(ref bool successChained) {
		if( !successChained ) {
			return 0.0f;
		}

		uint valueUint = getUint__n(16, successChained);

		if( !successChained ) {
			return 0.0f;
		}

		// transfer sign bit
		uint returnTemp = (valueUint & 0x8000) << (31-15);

		// extract and transfer exponent
		returnTemp |= (((valueUint >> 10) & 0x1f) << 26);

		// extract and transfer mantise
		returnTemp |= ((valueUint & 0x3ff) << 13);

		return *(cast(float*)&returnTemp);
	}

	/** \brief is the end reached?
	 *
	 * \return ...
	 */
	final public @property bool isEndReached() {
		// currently not implemented
		// TODO?
		return false;
	}

	protected BitstreamSourceImplementation bitstreamSource;
}
