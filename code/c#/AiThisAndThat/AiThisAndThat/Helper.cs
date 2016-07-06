using System;
using System.Collections.Generic;

namespace evolutionaryAlgorithms.geneticAlgorithm {
    class Helper {
        public static int toUint(bool[] genome, int startIndex, int length) {
            int result;
            int bitI;
            int exponent;

            result = 0;

            exponent = 1;

            for( bitI = startIndex; bitI < startIndex + length; bitI++ ) {
                if( genome[bitI] ) {
                    result += exponent;
                }

                exponent *= 2;
            }

            return result;
        }
    }
}
