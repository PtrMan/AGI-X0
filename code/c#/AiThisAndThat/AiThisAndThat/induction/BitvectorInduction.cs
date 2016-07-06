using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

/*
 * algorithm extrapolates a bitsequence. It does pure induction.
 * inefficient algorithmic unoptimized version.
 * inspiration from the paper "A formal theory of inductive inference. Part I" from R.J. Solomonoff
 * 
 */
namespace induction {
    class BitvectorInduction {
        public struct Prediction {
            public Prediction(ulong trueCounter, ulong falseCounter) {
                this.trueCounter = trueCounter;
                this.falseCounter = falseCounter;
            }
            
            double propabilityTrue {
                get { return (double)trueCounter / (double)(trueCounter + falseCounter); }
            }

            double propabilityFalse {
                get { return 1.0 - propabilityTrue; }
            }

            public ulong trueCounter;
            public ulong falseCounter;
        }
        
        public static Prediction predictNextBit(SlowBitvector deductionString, uint order) {
            Debug.Assert(order != 0, "Order is not allowed to be zero!");
            if( order == 0 ) {
                throw new SystemException("Order is not allowed to be zero!");
            }

            bool isOrderInfinite = order == -1;


            // TODO< implement for infinite order (== -1) and other orders
            Debug.Assert(order == 1, "Only implemented for order == 1! TODO");

            ulong
                falseCounter = 0,
                trueCounter = 0;

            // should take continuation without direct evidence into account?
            bool takeFloatingonsiderationIntoAccount = true;
            int startIndexDeltaFromEnding = takeFloatingonsiderationIntoAccount ? 0 : -1;

            for (int insertionIndex = deductionString.vector.Count + startIndexDeltaFromEnding; insertionIndex >= 1; insertionIndex--) {
                Console.WriteLine("InsertionIndex={0}", insertionIndex);
                
                for (int startIndex = 0; startIndex < deductionString.vector.Count + 1; startIndex++) {
                    for (int endIndex = startIndex + 1; endIndex < deductionString.vector.Count + 1; endIndex++) {
                        SlowBitvector deductionStringSub = deductionString.subvector((uint)startIndex, (uint)endIndex);


                        Console.WriteLine("");

                        Console.WriteLine("{0}", debugBitvector(deductionString));
                        Console.WriteLine("{0}", generateVoidString((uint)insertionIndex) +  debugBitvector(deductionStringSub));

                        bool matches = doesMatch(deductionString, deductionStringSub, insertionIndex);
                        Console.WriteLine("does match={0}", matches);

                        // TODO< if does match the extract the candidate bit and increment the counter >
                        if(matches && (insertionIndex + endIndex >= deductionString.vector.Count) ) {
                            Console.WriteLine("==>HERE");

                            // extract candidate bit
                            int deductionStringSubIndex = deductionString.vector.Count - insertionIndex;
                            bool candidateSymbol = deductionStringSub.vector[deductionStringSubIndex];
                            if( candidateSymbol ) {
                                trueCounter++;
                            }
                            else {
                                falseCounter++;
                            }
                        }
                    }
                }
            }
            
            return new Prediction(trueCounter, falseCounter);
        }

        private static bool doesMatch(SlowBitvector deductionString, SlowBitvector candidateBitvector, int insertionIndex) {
            if (insertionIndex + candidateBitvector.vector.Count <= deductionString.vector.Count) {
                return false;
            }

            int overlapEndIndex = Math.Min(deductionString.vector.Count, insertionIndex + candidateBitvector.vector.Count);
            int overlapLength = overlapEndIndex - insertionIndex;
            Debug.Assert(overlapLength >= 0, "Overlap length isn't allowed to be negative!");

            // fast exit
            if( overlapLength == 0 ) {
                return true;
            }

            SlowBitvector cuttedDeductionString = deductionString.subvector((uint)insertionIndex, (uint)overlapEndIndex);
            SlowBitvector cuttedCandidateBitvector = candidateBitvector.subvector(0, (uint)overlapLength);

            return checkOverlapEqual(cuttedDeductionString, cuttedCandidateBitvector);
        }

        /*
        private static void countPossibleExtensionsOf(SlowBitvector deductionString, SlowBitvector testVector, ref ulong falseCounter, ref ulong trueCounter) {
            Console.WriteLine("countPossibleExtensionsOf()");
            
            for (int deltaIn = 0; deltaIn < Math.Min(deductionString.vector.Length, testVector.vector.Length); deltaIn++) {
                SlowBitvector beginOverlap = deductionString.subvector((uint)(deductionString.vector.Length - 1 - deltaIn), (uint)(deductionString.vector.Length - 1));

                Console.WriteLine("  beginOverlapVector={0}", debugBitvector(beginOverlap));

                bool isValidOverlap = checkValidOverlap(beginOverlap, testVector);
                if (!isValidOverlap) {
                    continue;
                }

                Console.WriteLine("  (is valid overlap)");


                // extract bit and count up
                // BUGGY; TODO
                bool extractedCandidateBit = testVector.vector[deltaIn];

                Console.WriteLine("  extractedCandidateBit={0}", extractedCandidateBit);

                if   (extractedCandidateBit) { trueCounter++; }
                else                         { falseCounter++; }
            }
        }*/

        private static bool checkOverlapEqual(SlowBitvector a, SlowBitvector b) {
            Debug.Assert(a.vector.Count == b.vector.Count, "Must have same length!");
            return a == b;
        }

        // for debugging
        private static string debugBitvector(SlowBitvector vector) {
            StringBuilder sb = new StringBuilder();

            foreach(bool iterationValue in vector.vector) {
                if( iterationValue ) {
                    sb.Append("1");
                }
                else {
                    sb.Append("0");
                }
            }

            return sb.ToString();
        }

        private static string generateVoidString(uint count) {
            string result = "";

            for( uint i = 0; i < count; i++) {
                result += " ";
            }

            return result;
            
        }




        /**
         * used to enumerte (valid) strings of symbols
         */
        // for testing public
        abstract public class AbstractSymbolEnumerationContext {
            public AbstractSymbolEnumerationContext(uint maxNumberOfSymbols) {
                this.maxNumberOfSymbols = maxNumberOfSymbols;
                this.numberOfSymbols = 1;
            }

            
            public void increment(out uint numberOfDecodedSymbols, out bool finished) {
                finished = false;
                numberOfDecodedSymbols = numberOfSymbols;

                for (;;) {
                    // skip special strings of wildcard symbols
                    if (shouldMaskGetSkipped()) {
                        enumerationMask++;
                        continue;
                    }

                    // check for done mask, if true we increment the number of symbols and check for termination,
                    // if it doesn't terminate we continue the enumeration
                    if (enumerationMask == checkEnumerationDoneMask) {
                        if (numberOfSymbols > maxNumberOfSymbols) {
                            // enumeration is done
                            finished = true;
                            return;
                        }
                        
                        numberOfSymbols++;
                        numberOfDecodedSymbols = numberOfSymbols;

                        numberOfSymbolsChanged();

                        continue;
                    }

                    decode();

                    enumerationMask++;

                    // check for invalid symbols and increment if this is the case
                    if (isEnumerationMaskValid()) {
                        continue;
                    }


                    return;
                }
            }

            protected abstract void decode();
            protected abstract bool isEnumerationMaskValid();
            protected abstract bool shouldMaskGetSkipped();
            protected abstract void numberOfSymbolsChanged();

            protected uint maxNumberOfSymbols;
            protected uint numberOfSymbols;

            protected ulong
                // is composed out of a bit which is on the outer left to delemit the symbol string to be matched
                // the special value of each symbol in the symbol string is 0 (takes as many bits as "numberOfBitsForSymbol")
                // it is called wildcard symbol
                enumerationMask,

                checkEnumerationDoneMask;
        }



        /**
         * used to enumerte (valid) strings of symbols
         * zero (0) is the wildcard, which doesn't affect anything
         * other symbols are matched exactly in the algorithm which is using this algorithm
         * 
         */
        // for testing public
        public class SymbolEnumerationContext : AbstractSymbolEnumerationContext {
            private SymbolEnumerationContext(uint maxNumberOfSymbols) : base(maxNumberOfSymbols) {}

            public static SymbolEnumerationContext make(uint countOfSymbolsInAlphabet, uint maxNumberOfSymbols) {
                // TODO< calc numberOfBitsForSymbol from countOfSymbols + 1
                uint numberOfBitsForSymbol = 2;

                // if this case hits us, we have things to do to extend the algorithm to a fast bitvector
                Debug.Assert(maxNumberOfSymbols * numberOfBitsForSymbol + 1 <= 64, "More bits are used that available in a 64-bit ulong value, aborting");

                SymbolEnumerationContext result = new SymbolEnumerationContext(maxNumberOfSymbols);
                result.countOfSymbolsInAlphabet = countOfSymbolsInAlphabet;
                result.numberOfBitsForSymbol = numberOfBitsForSymbol;
                result.protectedDecodedSymbols = new uint[maxNumberOfSymbols];
                
                result.reset();
                return result;
            }

            public void reset() {
                numberOfSymbols = 1;

                setMasksForWidth(1);
            }

            private void setMasksForWidth(uint numberOfSymbols) {
                emptyEnumerationMask = 1ul << ((int)numberOfBitsForSymbol * (int)numberOfSymbols);
                enumerationMask = 1ul << ((int)numberOfBitsForSymbol * (int)numberOfSymbols);
                checkEnumerationDoneMask = enumerationMask << 1;
            }






            protected override void decode() {
                decodeMask();
            }

            protected override bool isEnumerationMaskValid() {
                return !isInvalidSymbolPresent();
            }

            protected override bool shouldMaskGetSkipped() {
                return enumerationMask == emptyEnumerationMask;
            }

            protected override void numberOfSymbolsChanged() {
                setMasksForWidth(numberOfSymbols);
            }



            private bool isInvalidSymbolPresent() {
                for (int i = 0; i < numberOfSymbols; i++) {
                    if (protectedDecodedSymbols[i] > countOfSymbolsInAlphabet) {
                        return true;
                    }
                }

                return false;
            }

            protected void decodeMask() {
                ulong mask = (1ul << ((int)numberOfBitsForSymbol)) - 1ul;
                //Console.WriteLine("mask={0}", convertToBitString(mask));
                
                for (int i = 0; i < numberOfSymbols; i++) {
                    ulong maskedOutSymbol = (enumerationMask >> (i * (int)numberOfBitsForSymbol)) & mask;
                    protectedDecodedSymbols[i] = (uint)maskedOutSymbol;
                }
            }

            public uint[] decodedSymbols {
                get {
                    return protectedDecodedSymbols;
                }
            }

            private uint[] protectedDecodedSymbols;

            
            private uint numberOfBitsForSymbol;
            

            private uint countOfSymbolsInAlphabet; // how many different symbols exist in the alphabet

            private ulong emptyEnumerationMask;
        }

        // helper for debugging
        private static string convertToBitString(ulong number) {
            string result = "";
            
            for (int i = 63; i >= 0; i--) {
                bool bit = (number & (1ul << i)) != 0ul;
                result += (bit ? "1" : "0");
            }

            return result;
        }

        /*
        public static void x(uint maxNumberOfSymbols) {
            uint numberOfBitsForSymbol = 2;

            


            uint[] decodedSymbols = new uint[maxNumberOfSymbols];

            uint numberOfSymbols = 1;

            ulong
                emptyEnumerationMask = 1ul << ((int)numberOfBitsForSymbol * 1),

                // is composed out of a bit which is on the outer left to delemit the symbol string to be matched
                // the special value of each symbol in the symbol string is 0 (takes as many bits as "numberOfBitsForSymbol")
                // it is called wildcard symbol
                enumerationMask = 1ul << ((int)numberOfBitsForSymbol * 1),

                checkEnumerationDoneMask = enumerationMask << 1;

            for (;;) {
                // skip special strings of wildcard symbols
                if (enumerationMask == emptyEnumerationMask) {
                    enumerationMask++;
                    continue;
                }

                // check for done mask, if true we increment the number of symbols and check for termination,
                // if it doesn't terminate we continue the enumeration
                if (enumerationMask == checkEnumerationDoneMask) {
                    numberOfSymbols++;

                    if (numberOfSymbols >= maxNumberOfSymbols) {
                        // TODO< enumeration is done >
                        Debug.Assert(false, "TODO");
                    }
                    
                    checkEnumerationDoneMask = checkEnumerationDoneMask << (int)numberOfBitsForSymbol;
                    emptyEnumerationMask = emptyEnumerationMask  << (int)numberOfBitsForSymbol;

                    enumerationMask = emptyEnumerationMask;

                    
                }

                decodeMask(enumerationMask, numberOfBitsForSymbol, ref decodedSymbols);

                Debug.Assert(false, "TODO< use the mask >");


                enumerationMask++;
            }
        }

        
         * outdated
        private static void decodeMask(ulong enumerationMask, uint numberOfBitsForSymbol, ref uint[] decodedSymbols) {
            for (int i = 0; i < numberOfBitsForSymbol; i++) {
                ulong mask = (1ul << ((int)numberOfBitsForSymbol+1)) - 1;
                ulong maskedOutSymbol = (enumerationMask >> (i * (int)numberOfBitsForSymbol)) & mask;
                decodedSymbols[i] = (uint)maskedOutSymbol;
            }
        }*/
    }
}
