using System.Diagnostics;

namespace MetaNix.nars {
    public class Bloomfilter {
        public Bloomfilter(uint numberOfBits) {
            this.numberOfBits = numberOfBits;

            filter = new StaticBitset(numberOfBits);
        }

        public void set(uint value) {
            // for all hash functions
            setBit(bloomHash1(value));
        }

        public bool test(uint value) {
            bool isSet = true;

            // for all hash functions
            isSet &= checkBit(bloomHash1(value));

            return isSet;
        }

        public void reset() {
            filter.reset();
        }

        public static bool overlap(Bloomfilter a, Bloomfilter b) {
            Debug.Assert(a.numberOfBits == b.numberOfBits);

            return StaticBitset.existsOverlap(a.filter, b.filter);
        }

        public static Bloomfilter union_(Bloomfilter a, Bloomfilter b) {
            Debug.Assert(a.numberOfBits == b.numberOfBits);

            Bloomfilter result = new Bloomfilter(a.numberOfBits);
            result.filter = StaticBitset.or_(a.filter, b.filter);
            return result;
        }


	    protected void setBit(uint index) {
            filter.set(index % numberOfBits, true);
        }

        protected bool checkBit(uint index) {
            return filter.get(index % numberOfBits);
        }

        protected static uint bloomHash1(uint x) {
            return Hash.hash_(x);
        }

        protected StaticBitset filter;
        public readonly uint numberOfBits;
    }
}
