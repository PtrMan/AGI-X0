using System.Collections.Generic;
using System.Linq;

namespace MetaNix.nars.memory {

    // the indices need to be refreshed after compound-GC!
    class CompoundHashtable {
        private CompoundHashtable() { } // disable standard ctor

        public CompoundHashtable(CompoundAndTermContext compoundAndTermContext, bool withCompoundId) {
		    this.compoundAndTermContext = compoundAndTermContext;
            this.withCompoundId = withCompoundId;
        }

        public bool existHash(ulong hashValue) {
            return hashtable.ContainsKey(hashValue);
        }

        public ulong[] getPotentialIndicesOfCompoundsByHash(ulong hashValue) {
            return hashtable[hashValue].Select(a => a.index).ToArray();
        }

        public void insert(CompoundIndex indexValue) {
            /*
            version(DEBUG) {
                static if (WithCompoundId) {
                    assert(reasonerInstance.accessCompoundByIndex(index).cachedHashWithCompoundIdValid);
                }
                else {
                    assert(reasonerInstance.accessCompoundByIndex(index).cachedHashWithoutCompoundIdValid);
                }
            }
            */

            HashWithIndex hashWithIndex = new HashWithIndex();
            hashWithIndex.index = indexValue.index;
            if (withCompoundId) {
                hashWithIndex.hash = compoundAndTermContext.accessCompoundByIndex(indexValue).cachedHashWithCompoundId;
            }
            else {
                hashWithIndex.hash = compoundAndTermContext.accessCompoundByIndex(indexValue).cachedHashWithoutCompoundId;
            }

            // insert into hashtable
            if( hashtable.ContainsKey(hashWithIndex.hash) ) {
                hashtable[hashWithIndex.hash].Add(hashWithIndex);
            }
            else {
                hashtable[hashWithIndex.hash] = new List<HashWithIndex>() { hashWithIndex };
            }
        }
        /*
        protected static ulong calcHash(HashWithIndex hashWithIndex) {
            return hashWithIndex.hash;
        }
        */

        public class HashWithIndex {
            public ulong index;
            public ulong hash;

            public bool isEqual(HashWithIndex other) {
                return index == other.index && hash.Equals(other.hash);
            }
        }


        private CompoundAndTermContext compoundAndTermContext;
        private IDictionary<ulong, IList<HashWithIndex>> hashtable = new Dictionary<ulong, IList<HashWithIndex>>(); // by index
        private bool withCompoundId;
    }

}
