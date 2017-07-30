using System;
using System.Collections.Generic;
using System.Diagnostics;

using BloomfilterType = MetaNix.nars.Bloomfilter/*<*TermId*int>*/;

namespace MetaNix.nars.entity {
    /**
     * A stamp like in the classic NARS which contains the stamp history (as TermIdType values) and a bloomfilter for the values
     */
    public class DualStamp {
        public DualStamp(uint numberOfElements, uint bloofilterNumberOfBits) {
            privateUsed = 0;
            this.numberOfElements = numberOfElements;

            bloomfilter = new BloomfilterType(bloofilterNumberOfBits);

            termIdHistory = new /*TermId*/uint[numberOfElements];
        }

        public static DualStamp zip(DualStamp a, DualStamp b) {
            DualStamp result = new DualStamp(a.numberOfElements, a.bloomfilter.numberOfBits);

            uint newUsed = Math.Min((uint)(a.privateUsed + b.privateUsed), (uint)a.numberOfElements);

            int ai = 0, bi = 0;

            IList<uint> zipped = new List<uint>();

            // zip
            for(;;ai++,bi++) {
                if( ai == a.used || bi == b.used ) {
                    break;
                }

                zipped.Add(a.accessTermIdHistory(ai));
                zipped.Add(b.accessTermIdHistory(bi));
            }

            // remainder
            for(;;ai++) {
                if( ai == a.used ) {
                    break;
                }

                zipped.Add(a.accessTermIdHistory(ai));
            }

            for (; ; bi++) {
                if (bi == b.used) {
                    break;
                }

                zipped.Add(b.accessTermIdHistory(bi));
            }

            result.insertAtFront(zipped);
            result.recalcBloomfilter(newUsed);
            return result;
        }

        public static bool checkEqual(DualStamp a, DualStamp b) {
            if( a.privateUsed != b.privateUsed ) {
                return false;
            }

            ISet<uint> aSet = new HashSet<uint>();
            for( int i = 0; i < a.privateUsed; i++ ) {
                aSet.Add(a.termIdHistory[i]);
            }

            ISet<uint> bSet = new HashSet<uint>();
            for (int i = 0; i < b.privateUsed; i++) {
                bSet.Add(b.termIdHistory[i]);
            }

            return aSet.Overlaps(bSet) && bSet.Overlaps(aSet);
        }

        public void insertAtFront(IList</*TermId*/uint> termIds) {
            uint newUsed = Math.Min((uint)(privateUsed + termIds.Count), (uint)termIdHistory.Length);

            // push the old values to the back
            for( int i = 0; i < termIdHistory.Length - termIds.Count; i++ ) {
                int baseIndex = termIds.Count;

                termIdHistory[baseIndex + i] = termIdHistory[i];
            }

            for (int i = 0; i < termIds.Count; i++) {
                termIdHistory[i] = termIds[i];
            }

            bool sizeDidntChange = newUsed == privateUsed;
            if (sizeDidntChange) {
                recalcBloomfilter(newUsed);
            }
            else {
                addToBloomfilter(termIds);
            }
        }

        public bool collide(DualStamp a, DualStamp b) {
            return
                collideByBloomfilter(a, b) && // fast rejection by checking if the bits overlap, if at least one bit does overlap it could overlap
                collideIterateHistoryAndCheckBloomfilter(a, b) &&
                collideIterateHistorySlow(a, b);
        }

        public uint used {
            get {
                return privateUsed;
            }
        }

        public uint accessTermIdHistory(int index) {
            Debug.Assert(index < used);
            return termIdHistory[index];
        }
        
	    protected uint privateUsed;
        protected /*TermId*/uint[] termIdHistory;
        protected BloomfilterType bloomfilter;
        private uint numberOfElements;

        protected void recalcBloomfilter(uint newSize) {
            bloomfilter.reset();

            for (int i = 0; i < newSize; i++) {
                bloomfilter.set(termIdHistory[i]);
            }
        }

        protected void addToBloomfilter(IList</*TermId*/uint> termIds) {
            for (int i = 0; i < termIds.Count; i++) {
                bloomfilter.set(termIds[i]);
            }
        }

        // returns if the two bloomfilter (can) collide by checking if bits of the bloomfilters overlap
        protected static bool collideByBloomfilter(DualStamp a, DualStamp b) {
            return BloomfilterType.overlap(a.bloomfilter, b.bloomfilter);
        }

        // iterates over the termIdHistory of the stamp with the least number of entries and checks in the bloomfilter of the other if the bit is set
        protected bool collideIterateHistoryAndCheckBloomfilter(DualStamp a, DualStamp b) {
            if (a.privateUsed < b.privateUsed) {
                return collideIterateHistoryAndCheckBloomfilterHelper(a, b);
            }
            else {
                return collideIterateHistoryAndCheckBloomfilterHelper(b, a);
            }
        }

        // iterates over the termIdHistory of a and checks if the coresponding entry in b in the bloomfilter is set
        protected bool collideIterateHistoryAndCheckBloomfilterHelper(DualStamp a, DualStamp b) {
            Debug.Assert(a.privateUsed <= numberOfElements);

            for (int i = 0; i < a.privateUsed; i++) {
                /*TermId*/uint aTermId = a.termIdHistory[i];
                bool isSetInB = b.bloomfilter.test(aTermId);
                if (isSetInB) {
                    return true;
                }
            }

            return false;
        }

        // ASK PATRICK< is this algorithm right to check all n to n or should we just check for common sequences? >
        protected bool collideIterateHistorySlow(DualStamp a, DualStamp b) {
            Debug.Assert(a.privateUsed <= numberOfElements);
            Debug.Assert(b.privateUsed <= numberOfElements);

            for (int ia = 0; ia < a.privateUsed; ia++) {
                for (int ib = 0; ib < b.privateUsed; ib++) {
                    /*TermId*/uint aTermId = a.termIdHistory[ia];
                    /*TermId*/uint bTermId = b.termIdHistory[ib];
                    if (aTermId == bTermId) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
