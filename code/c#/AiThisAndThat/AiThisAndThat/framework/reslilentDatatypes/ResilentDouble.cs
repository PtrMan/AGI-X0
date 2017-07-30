using System;
using System.Diagnostics;

namespace AiThisAndThat.framework.reslilentDatatypes {
    // double with build in error detection (and possibly correction)
    // used for long term storage (possibly on corruptable media)
    /* commented because not in use
    public class SlowResilentDouble {
        byte[] raw;
        bool parity;

        public double value {
            get {
                checkParity();
                return BitConverter.ToDouble(raw, 0);
            }
            set {
                raw = BitConverter.GetBytes(value);
                parity = calcParity();
            }
        }

        void checkParity() {
            bool parity = calcParity();
            if( parity != this.parity )   throw new Exception("parity error!");
        }

        bool calcParity() {
            Debug.Assert(raw.Length == 8);
            byte c = 0;
            for( int i = 0; i < 8; i++ )   c ^= raw[i];

            bool parity = false;
            for( int bitI = 0; bitI < 8; bitI++ )   parity ^= (((c >> bitI) & 1) == 1);
            return parity;
        }
    }*/

    // stores copies of the value to check fast for errors and recover
    public class FastResilentDouble {
        double a, b, c; // should all have the same value

        public double value {
            get {
                if( isCorrupted() ) {
                    double correctedValue = tryVoteOnCorrectValue();
                    a = correctedValue;
                    b = correctedValue;
                    c = correctedValue;
                }
                return a;
            }
            set {
                a = value;
                b = value;
                c = value;
            }
        }

        double tryVoteOnCorrectValue() {
            if( a == b )  return a;
            if( a == c )  return a;
            if( b == c )  return b;
            throw new Exception("Can't vote on correct value");
        }

        bool isCorrupted() {
            return a != b || b != c;
        }
    }
}
