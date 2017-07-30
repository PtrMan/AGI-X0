using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApplication1 {
    public interface IBitstreamDestination {
        void addBoolean(bool value);
    }

    public class BitstreamWriter {
        private BitstreamWriter() { }
        public BitstreamWriter(IBitstreamDestination bitstreamDestination) {
            this.bitstreamDestination = bitstreamDestination;
        }

        /** \brief add bits
	     *
	     * \param bits ...
	     */
	    public void addBits(IEnumerable<bool> bits) {
		    foreach(bool bit in bits) {
			    addBoolean(bit);
		    }
	    }

        public void addBytes(IEnumerable<byte> bytes, ref bool successChained) {
            foreach(byte iByte in bytes) {
                addByte(iByte, ref successChained);
            }
        }

        public void addByte(byte value, ref bool successChained) {
            addUint__n((uint)value, 8, ref successChained);
        }


	    /** \brief Add a bool
	     *
	     * \param boolean ...
	     */
	    public void addBoolean(bool boolean) {
            bitstreamDestination.addBoolean(boolean);
	    }

	    /** \brief Tries to add a Number with variable length to the Data
	     *
	     * Important is that the number must be smaller than 65536
	     * \param value is the Number
	     * \param successChained will be false if something went wrong
	     */
	    public void addUint_2__4_8_12_16(uint value, ref bool successChained) {
		    uint n;

		    if( !successChained ) {
			    return;
		    }

		    if( (value & 0xFFFF0000) != 0 ) {
			    successChained = false;
			    return;
		    }
		    else if( (value & 0xF000) != 0 ) {
			    n = 3;
		    }
		    else if( (value & 0xF00) != 0 ) {
			    n = 2;
		    }
		    else if( (value & 0xF0) != 0 ) {
			    n = 1;
		    }
		    else {
			    n = 0;
		    }

		    // add count
		    addBoolean(((n>>1) & 1) != 0);
		    addBoolean((n & 1) != 0);

		    // add bits
		    uint bitCount = (n+1) * 4;

		    for( int bitI = 0; bitI < bitCount; bitI++ ) {
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
	    public void addUint__n(uint value, uint bitCount, ref bool successChained) {
		    Debug.Assert(bitCount > 0, "Bits need to be bigger than 0");

		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(bitCount <= 32, "Bits need to be less or equal to 32");

		    if( !successChained ) {
			    return;
		    }

		    if( bitCount == 0 ) {
			    successChained = false;
			    return;
		    }

		    uint mask = 0;

		    for( int bitI = 0; bitI < bitCount; bitI++ ) {
			    mask = mask | (uint)(1<<bitI);
		    }

		    // check if the Number fits into n bits
		    if( ((~mask) & value) != 0 ) {
			    successChained = false;
			    return;
		    }

		    for( int bitI = 0; bitI < bitCount; bitI++ ) {
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
	    public void addInt__n(int value, uint bitCount, ref bool successChained) {
		    Debug.Assert(bitCount > 0, "Bits need to be bigger than 0");

		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(bitCount <= 31, "Bits need to be less or equal to 31");

		    if( !successChained ) {
			    return;
		    }

		    if( bitCount == 0 ) {
			    successChained = false;
			    return;
		    }

            bool sign = value < 0;
            value = System.Math.Abs(value);

		    uint mask = 0;

		    for( int bitI = 0; bitI < bitCount; bitI++ ) {
			    mask = mask | (uint)(1<<bitI);
		    }

		    // check if the Number fits into n bits
		    if( ((~mask) & value) != 0 ) {
			    successChained = false;
			    return;
		    }

            addBoolean(sign);
		    for( int bitI = 0; bitI < bitCount; bitI++ ) {
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
	    public void addFixed__n(float value, uint bits, float range, ref bool successChained) {
		    Debug.Assert(bits > 2, "bits need to be bigger than 2");

		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(bits <= 32, "bits need to be less or equal to 32");

		    Debug.Assert(range > 0.0f, "range must be greater than 0.0");

		    if( !successChained ) {
			    return;
		    }

		    float valueTemp = Math.Abs(value);

		    // check if the number fits into range
		    if( valueTemp > range ) {
			    successChained = false;
			    return;
		    }

		    uint valueUint = (uint)( valueTemp/range * (float)(1 << (int)(bits-1)) );

		    bool sign = valueTemp > 0.0f;
		    addBoolean(sign);
		    addUint__n(valueUint, bits-1, ref successChained);

		    // NOTE< successChained doesn't have to be checked! >
	    }

	    /** \brief Add a 16 bit float Value
	     *
	     * \param value ...
	     */
        /* this is still D code, convert it with caution and test it!
	    public void addFloat16(float value) {
		    bool successChained;

		    uint valueUint = *(cast(uint*)&value);

		    // transfer sign
		    uint converted = (valueUint & 0x80000000) >> 16;

		    // transfer exponent
		    converted |= (((valueUint >> 26) & 0x1f) << 10);

		    // transfer mantise
		    converted |= ((valueUint >> 13) & 0x3ff);

		    successChained = true;

		    addUint__n(converted, 16, ref successChained);

		    Debug.Assert(successChained, "Should not fail");
	    }
         */

	    /** \brief Add a 32 bit float value
	     *
	     * \param value ...
	     */
	    public void addFloat32(float value, ref bool successChained) {
		    addBytes(BitConverter.GetBytes(value), ref successChained);
	    }

        /** \brief Add a 64 bit float value
	     *
	     * \param value ...
	     */
	    public void addFloat64(double value, ref bool successChained) {
            byte[] arr = BitConverter.GetBytes(value);
		    addBytes(arr, ref successChained);
	    }

        protected IBitstreamDestination bitstreamDestination;
    }

    public interface IBitstreamSource {
        bool isEndReached { get; }

        bool isValid(uint numberOfRequestedBits);
        bool isValid();
        
        bool readNextBit();
    }

    /** \brief 
     *
     */
    public class BitstreamReader {
	    protected BitstreamReader() {}
	    public BitstreamReader(IBitstreamSource bitstreamSource) {
		    this.bitstreamSource = bitstreamSource;
	    }

	    /** \brief returns the next Bool
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
	    public bool getBoolean(ref bool successChained) {
		    bool result;

		    if( !successChained ) {
			    return false;
		    }

		    if( isEndReached ) {
			    successChained = false;
			    return false;
		    }

		    // just to make sure no runtime error happens
		    if( !bitstreamSource.isValid() ) {
			    successChained = false;
			    return false;
		    }

		    return bitstreamSource.readNextBit();
	    }

	    /** \brief returns a number of up to 16 bits wide
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
	    public uint getUint_2__4_8_12_16(ref bool successChained) {
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

		    Debug.Assert(numberOfBits > 0, "numberOfBits must be greater than 0!");

		    if( !bitstreamSource.isValid(numberOfBits) ) {
			    successChained = false;
			    return 0;
		    }


		    uint result = 0;
		    for( int bitI = 0; bitI < numberOfBits; bitI++ ) {
			    if( bitstreamSource.readNextBit() ) {
				    result |= (uint)(1<<bitI);
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
        /*
	    public char[] getString8FixedLength(uint numberOfSigns, ref bool successChained) {
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
       */

        public byte[] getBytes(uint numberOfBytes, ref bool successChained) {
            if (!successChained) {
                return new byte[]{};
            }

            byte[] resultArr = new byte[numberOfBytes];

            for (int i = 0; i < numberOfBytes; i++) {
                resultArr[i] = getByte(ref successChained);
            }
            return resultArr;
        }

        public byte getByte(ref bool successChained) {
            return (byte)getUint__n(8, ref successChained);
        }

	    /** \brief returns a number of 'Bits' bits wide
	     *  \param numberOfBits How many bits should be readed
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
	    public uint getUint__n(uint numberOfBits, ref bool successChained) {
		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(numberOfBits <= 32, "bits need to be less or equal to 32");

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

		    for( int bitI = 0; bitI < numberOfBits; bitI++ ) {
			    if( bitstreamSource.readNextBit() ) {
				    result |= (uint)(1 << bitI);
			    }
		    }

		    return result;
	    }

        /** \brief returns a number of 'Bits' bits wide
	     *  \param numberOfBits How many bits should be readed
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
	    public int getInt__n(uint numberOfBits, ref bool successChained) {
		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(numberOfBits <= 31, "bits need to be less or equal to 32");

		    int result = 0;

		    if( !successChained ) {
			    return 0;
		    }

		    if( numberOfBits == 0 ) {
			    successChained = false;
			    return 0;
		    }

		    if( !bitstreamSource.isValid(1+numberOfBits) ) {
			    successChained = false;
			    return 0;
		    }

            bool sign = bitstreamSource.readNextBit();
		    for( int bitI = 0; bitI < numberOfBits; bitI++ ) {
			    if( bitstreamSource.readNextBit() ) {
				    result |= (1 << bitI);
			    }
		    }

            if( sign ) {
                result *= -1;
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
	    public float getFixed__n(uint bits, float range, ref bool successChained) {
		    Debug.Assert(bits > 2, "bits need to be bigger than 2");

		    // only assert because it is in the gamecode very inpropable that something like this happens
		    Debug.Assert(bits <= 32, "bits need to be less or equal to 32");

		    Debug.Assert(range > 0.0f, "range must be greater than 0.0");

		    if( !successChained ) {
			    return 0.0f;
		    }

		    bool sign = getBoolean(ref successChained);
		    if( !successChained ) {
			    return 0.0f;
		    }

		    int valueInt = (int)getUint__n(bits-1, ref successChained);

		    if( !successChained ) {
			    return 0.0f;
		    }

		    if( sign ) {
			    valueInt *= -1;
		    }

		    return ((float)valueInt / (float)(1 << ((int)bits-1))) * range;
	    }

	    // TODO< unit tests >
	    /** \brief tries to return 'Bits' bits
	     * 
	     *  \param bits How many bits should be read?
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
        /* commented because it has to be converted from D
	    public bool[] getBits(uint bits, ref bool successChained) {
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
	    } */


	    /** \brief returns a 32 bit float
	     *  \param successChained will be false if the function failed
	     *  \return ...
	     */
        /* commented because it has to be converted from D
	    public float getFloat32(ref bool successChained) {
		    if( !successChained ) {
			    return 0.0f;
		    }

		    uint valueUint = getUint__n(32, successChained);

		    if( !successChained ) {
			    return 0.0f;
		    }

		    return *(cast(float*)&valueUint);
	    }*/

        public double getFloat64(ref bool successChained) {
		    if( !successChained ) {
			    return 0.0;
		    }

            byte[] valueBytes = getBytes(8, ref successChained);
            if( !successChained ) {
                return 0.0;
            }

		    return BitConverter.ToDouble(valueBytes, 0);
        }
   
	    /** \brief returns a half precision float value
	     *
	     * \param successChained will be false if the function failed
	     * \return ...
	     */
        /* commented because it has to be converted from D
	    public float getFloat16(ref bool successChained) {
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
	    }*/

	    /** \brief is the end reached?
	     *
	     * \return ...
	     */
	    public bool isEndReached { get {
		        return bitstreamSource.isEndReached;
            }
	    }

	    protected IBitstreamSource bitstreamSource;
    }


    public class ByteBakedBitstreamSource : IBitstreamSource {
        public ByteBakedBitstreamSource(byte[] arr) {
            this.buffer = arr;
        }

        public bool isEndReached { get {
                return !isCurrentIndexValid(0);
            }
        }

        public bool isValid(uint numberOfRequestedBits) {
            return isCurrentIndexValid(numberOfRequestedBits);
        }

        public bool isValid() {
            return isCurrentIndexValid();
        }

        bool isCurrentIndexValid(uint numberOfreadaheadBits = 0) {
            // we can't know the exact number of bits
            return (globalbitIndex + numberOfreadaheadBits) / 8 <= buffer.Length;
        }

        public bool readNextBit() {
            if( !isCurrentIndexValid(0) ) {
                throw new Exception("read beyond physical end!");
            }

            bool result = ((1 << bitIndex) & (int)buffer[globalbitIndex / 8]) != 0;
            bitIndex = (bitIndex+1) % 8;
            globalbitIndex++;

            //Console.WriteLine("read bit {0}", result);

            return result;
        }

        long globalbitIndex = 0;
        byte[] buffer;
        int bitIndex = 0;
    }

    public class ByteBakedBitstreamDestination : IBitstreamDestination {
        public void addBoolean(bool value) {
            // if previous byte was full we append a new byte
            if( bitIndex == 0 ) {
                buffer.Add(0);
            }

            byte mask = (byte)((value ? 1 : 0) << bitIndex);
            if (value) {
                buffer[buffer.Count-1] |= mask;
            }

            globalbitIndex++;
            bitIndex = (bitIndex + 1) % 8;
        }

        long globalbitIndex = 0;
        public IList<byte> buffer = new List<byte>();
        int bitIndex = 0;
    }
}
