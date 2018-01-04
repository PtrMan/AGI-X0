using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.framework.misc {
    public static class BinaryConversion {
        public static int base2ToInt(bool[] arr) {
            int result = 0;
            for (int i = 0; i < arr.Length; i++) {
                bool bit = arr[i];
                result |= ((1 << i) * (bit ? 1 : 0));
            }
            return result;
        }

        public static bool[] intToBase2(int val, int numberOfBits) {
            bool[] arr = new bool[numberOfBits];
            for (int i = 0; i < numberOfBits; i++) {
                arr[i] = (val & (1 << i)) != 0;
            }
            return arr;
        }
    }
}
