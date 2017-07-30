using System.Diagnostics;

namespace MetaNix.nars {
    public class StaticBitset {
        public StaticBitset(uint numberOfBits) {
            this.numberOfBits = numberOfBits;

            array = new ulong[numberOfBits / (NUMBEROFBITSFORMACHINEWORD) + 1]; // TODO< remove +1 if modulo is zero >

            reset();
        }

        public void reset() {
            for (int i = 0; i < array.Length; i++) {
                array[i] = 0;
            }
        }

        public static StaticBitset or_(StaticBitset a, StaticBitset b) {
            Debug.Assert(a.numberOfBits == b.numberOfBits);

            StaticBitset result = new StaticBitset(a.numberOfBits);
            for (int i = 0; i < result.array.Length; i++) {
                result.array[i] = a.array[i] | b.array[i];
            }
            return result;
        }

        public static bool existsOverlap(StaticBitset a, StaticBitset b) {
            Debug.Assert(a.numberOfBits == b.numberOfBits);

            StaticBitset result = new StaticBitset(a.numberOfBits);
            for (int i = 0; i < result.array.Length; i++) {
                if ((a.array[i] & b.array[i]) != 0) {
                    return true;
                }
            }
            return false;
        }


        public bool get(uint bitIndexParameter) {
            uint arrayIndex = bitIndexParameter / NUMBEROFBITSFORMACHINEWORD;
            uint bitIndex = bitIndexParameter % NUMBEROFBITSFORMACHINEWORD;
            return (array[arrayIndex] & (1ul << (int)bitIndex)) != 0;
        }

        public void set(uint bitIndexParameter, bool value) {
            uint arrayIndex = bitIndexParameter / NUMBEROFBITSFORMACHINEWORD;
            uint bitIndexInWord = bitIndexParameter % NUMBEROFBITSFORMACHINEWORD;

            ulong negationMask = ~(1ul << (int)bitIndexInWord);

            if (value) {
                array[arrayIndex] |= (1ul << (int)bitIndexInWord);
            }
            else {
                array[arrayIndex] = array[arrayIndex] & negationMask;
            }
        }

        const uint NUMBEROFBITSFORMACHINEWORD = sizeof(ulong) * 8;

        ulong[] array;
        private uint numberOfBits;
    };

}
