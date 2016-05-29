module serialisation.BitstreamWriter;

import std.math : abs;

/** \brief 
 *
 */
class BitstreamWriter(BitstreamDestinationImplementation) {
	protected final this() {
	}

	public final this(BitstreamDestinationImplementation bitstreamDestination) {
		this.bitstreamDestination = bitstreamDestination;
	}

	/** \brief add bits
	 *
	 * \param bits ...
	 */
	final public void addBits(bool[] bits) {
		foreach(bit; bits) {
			addBoolean(bit);
		}
	}

	/** \brief Add a bool
	 *
	 * \param boolean ...
	 */
	final public void addBoolean(bool boolean) {
		bitstreamDestination.addBoolean(boolean);
	}

	/** \brief Tries to add a Number with variable length to the Data
	 *
	 * Important is that the number must be smaller than 65536
	 * \param value is the Number
	 * \param successChained will be false if something went wrong
	 */
	final public void addUint_2__4_8_12_16(uint value, ref bool successChained) {
		uint n;

		if( !successChained ) {
			return;
		}

		if( value & 0xFFFF0000 ) {
			successChained = false;
			return;
		}
		else if( value & 0xF000 ) {
			n = 3;
		}
		else if( value & 0xF00 ) {
			n = 2;
		}
		else if( value & 0xF0 ) {
			n = 1;
		}
		else {
			n = 0;
		}

		// add count
		addBoolean((n>>1) & 1);
		addBoolean(n & 1);

		// add bits
		uint bitCount = (n+1) * 4;

		foreach( bitI; 0..bitCount ) {
			addBoolean(((1 << bitI) & value) != 0);
		}
	}

	/** \brief Tries to add an integer with n Bits
	 *
	 * Important is that the Number must be small enougth to fit into n bits
	 *
	 * \param value is the Number
	 * \param bitCount Number of bits
	 * \param SuccessChained will be false if something went wrong
	 */
	final public void addUint__n(uint value, uint bitCount, ref bool successChained) {
		assert(bitCount > 0, "Bits need to be bigger than 0");

		// only assert because it is in the gamecode very inpropable that something like this happens
		assert(bitCount <= 32, "Bits need to be less or equal to 32");

		if( !successChained ) {
			return;
		}

		if( bitCount == 0 ) {
			successChained = false;
			return;
		}

		uint mask = 0;

		for( uint bitI = 0; bitI < bitCount; bitI++ ) {
			mask = mask | (1<<bitI);
		}

		// check if the Number fits into n bits
		if( (~mask) & value ) {
			successChained = false;
			return;
		}

		foreach( bitI; 0..bitCount ) {
			addBoolean(((1 << bitI) & value) != 0);
		}
	}

	/** \brief Tries to add an fixed point value with n Bits
	 *
	 * Important is that the value must fit into range (checked)
	 *
	 * \param value is the Number
	 * \param Bits Number of bits (inclusive sign)
	 * \param range Maximal range of the number
	 * \param successChained will be false if something went wrong
	 */
	final public void addFixed__n(float value, uint bits, float range, ref bool successChained) {
		assert(bits > 2, "bits need to be bigger than 2");

		// only assert because it is in the gamecode very inpropable that something like this happens
		assert(bits <= 32, "bits need to be less or equal to 32");

		assert(range > 0.0f, "range must be greater than 0.0");

		if( !successChained ) {
			return;
		}

		float valueTemp = abs(value);

		// check if the number fits into range
		if( valueTemp > range ) {
			successChained = false;
			return;
		}

		uint valueUint = cast(uint)( valueTemp/range * cast(float)(1 << (bits-1)) );

		bool sign = valueTemp > 0.0f;
		addBoolean(sign);
		addUint__n(valueUint, bits-1, successChained);

		// NOTE< successChained doesn't have to be checked! >
	}

	/** \brief Add a 16 bit float Value
	 *
	 * \param value ...
	 */
	final public void addFloat16(float value) {
		bool successChained;

		uint valueUint = *(cast(uint*)&value);

		// transfer sign
		uint converted = (valueUint & 0x80000000) >> 16;

		// transfer exponent
		converted |= (((valueUint >> 26) & 0x1f) << 10);

		// transfer mantise
		converted |= ((valueUint >> 13) & 0x3ff);

		// works perfectly until here

		successChained = true;

		addUint__n(converted, 16, successChained);

		assert(successChained, "Should not fail");
	}

	/** \brief Add a 32 bit float value
	 *
	 * \param value ...
	 */
	final public void addFloat32(float value) {
		bool successChained;
		
		uint valueUint = *(cast(uint*)&value);

		successChained = true;

		addUint__n(valueUint, 32, successChained);

		assert(successChained, "Should not fail");
	}

	final public void addString(string value, ref bool successChained) {
		if( !successChained ) {
			return;
		}

		if( value.length > 1<<16-1 ) {
			successChained = false;
			return;
		}

		addUint__n(value.length, 16, successChained);

		foreach( iterationChar; value ) {
			uint charAsUint = cast(uint)iterationChar;
			addUint__n(charAsUint, 8, successChained);
		}
	}

	/+
   // TODO UNITTEST
   // TOUML
   /** \brief Tries to add a string sended as 8 Bit ASCII codes
    *
    * The format which is written in the Bitstream is
    *  * Length of the String (-1 if LengthGreaterThanZero)
    *  * The String as 8 bit numbers
    *
    * \param String ...
    * \param LengthBits how many bits should be used for the encoding of the length
    * \param LengthGreaterThanZero if this is true it means that the Length in the bitstream is one less than the real length, this can safe a bit if a string can'tbe empty
    * \param successChained will be false if something went wrong
    */
   final public void addWideString8(WideString String, uint LengthBits, bool LengthGreaterThanZero, ref bool successChained)
   {
      uint StringLength;

      if( !successChained )
      {
         return;
      }

      StringLength = String.getLength();

      if( LengthGreaterThanZero && StringLength == 0 )
      {
         successChained = false;
         return;
      }

      if( LengthBits > 32 )
      {
         successChained = false;
         return;
      }

      if( LengthGreaterThanZero )
      {
         this.addUint__n(StringLength-1, LengthBits, successChained);
      }
      else
      {
         this.addUint__n(StringLength, LengthBits, successChained);
      }

      if( !successChained )
      {
         return;
      }

      foreach( Sign; String.getSigns )
      {
         this.addUint__n(Sign, 8, successChained);
      }
   }+/

	protected BitstreamDestinationImplementation bitstreamDestination;
}
