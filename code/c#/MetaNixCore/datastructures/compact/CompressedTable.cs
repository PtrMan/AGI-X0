using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaNix.datastructures.compact {
    class CompressedTable {
        // if true check could appear in simdValue, if false it doesn't appear
        public static bool simdPretest(ulong check, ulong simdValue) {
            // checks if the check apears in (packed) value, value has n values packed
            ulong checkedPattern = check | (check << (16 * 1)) | (check << (16 * 2)) | (check << (16 * 3));
            //  xor with negation to get true for all bits of the subword if it matches completely
            ulong xored = checkedPattern ^ (~simdValue);

            ulong orCheckTemporary = xored | (xored >> 16 * 1) | (xored >> 16 * 2) | (xored >> 16 * 3);
            bool atLeastOneMatch = orCheckTemporary == ((1 << 16) - 1);
            return atLeastOneMatch;
        }

        // check if "check"value is in simdValue and returns the found index of it
        public static bool simdTest(ulong check, ulong simdValue, out int index) {
            for (index = 0; index < 4; index++) {
                ulong maskedSimdValue = (simdValue >> (16 * index)) & ((1 << 16) - 1);
                if (maskedSimdValue == check) {
                    return true;
                }
            }
            return false;
        }

        // used to build simd packed values
        public static ulong simdPack(ulong simd0, ulong simd1, ulong simd2, ulong simd3) {
            return simd0 | (simd1 << (16 * 1)) | (simd2 << (16 * 2)) | (simd3 << (16 * 3));
        }

        public static void simdUnpack(out ulong simd0, out ulong simd1, out ulong simd2, out ulong simd3, ulong fromValue) {
            simd0 = (fromValue >> 16 * 0) & ((1 << 16) - 1);
            simd1 = (fromValue >> 16 * 1) & ((1 << 16) - 1);
            simd2 = (fromValue >> 16 * 2) & ((1 << 16) - 1);
            simd3 = (fromValue >> 16 * 3) & ((1 << 16) - 1);
        }

        public static ulong simdGetValueByIndex(ulong value, uint simdIndex) {
            return (value >> (int)(simdIndex * 16)) & ((1 << 16) - 1);
        }


        void find(ulong check, out bool found, out int index, out int simdIndex) {
            found = false;
            simdIndex = 0;

            // search in complete words
            for (index = 0; index < simdValues.Count - 1; index++) {
                if (simdPretest(check, simdValues[index])) {
                    found = simdTest(check, simdValues[index], out simdIndex);
                    if (found) {
                        return;
                    }
                }
            }

            if (simdValues.Count == 0) {
                found = false;
                return;
            }

            // search in incomplete word
            ulong lastValue = simdValues[simdValues.Count - 1];
            for (simdIndex = 0; simdIndex <= (privateUsedValues % valuesPerSimd); simdIndex++) {
                ulong maskedAndShiftedValue = simdGetValueByIndex(lastValue, (uint)simdIndex);
                if (maskedAndShiftedValue == check) {
                    index = simdValues.Count - 1;
                    found = true;
                    return;
                }
            }
        }

        public void append(uint value) {
            uint currentSimdIndex = (uint)privateUsedValues % valuesPerSimd;
            bool isFull = currentSimdIndex == 0;
            if (isFull) {
                simdValues.Add(value);
            }
            else {
                // append to last element by unpacking and packing
                ulong oldPackedValue = simdValues[simdValues.Count - 1];

                ulong simd0, simd1, simd2, simd3;
                simdUnpack(out simd0, out simd1, out simd2, out simd3, oldPackedValue);

                if (currentSimdIndex == 0) {
                    simd0 = value;
                }
                else if (currentSimdIndex == 1) {
                    simd1 = value;
                }
                else if (currentSimdIndex == 2) {
                    simd2 = value;
                }
                else if (currentSimdIndex == 3) {
                    simd3 = value;
                }
                else {
                    throw new Exception("Internal error");
                }

                simdValues[simdValues.Count - 1] = simdPack(simd0, simd1, simd2, simd3);
            }

            privateUsedValues++;
        }

        public bool hasValue(uint value) {
            bool found;
            int index;
            int simdIndex;
            find(value, out found, out index, out simdIndex);
            return found;
        }

        public uint getValueByGlobalIndex(uint globalIndex) {
            uint simdValuesIndex = globalIndex % 4;
            uint indexSimdValuesList = globalIndex / 4;
            return (uint)simdGetValueByIndex(simdValues[(int)indexSimdValuesList], simdValuesIndex);
        }

        public uint usedValues {
            get {
                Debug.Assert(privateUsedValues >= 0);
                return (uint)privateUsedValues;
            }
        }


        int privateUsedValues;
        IList<ulong> simdValues = new List<ulong>();

        const uint valuesPerSimd = 4;
    }
}
