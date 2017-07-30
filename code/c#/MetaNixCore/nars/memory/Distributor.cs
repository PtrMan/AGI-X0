using System.Collections.Generic;

using MetaNix.misc;

namespace MetaNix.nars.memory {
    /**
     * A pseudo-random number generator, used in Bag.
     */
    public class Distributor {

        /** Shuffled sequence of index numbers */
        public short[] order;
        /** Capacity of the array */
        public uint capacity;

        private static IDictionary<uint, Distributor> distributors = new Dictionary<uint, Distributor>(8);

        public static Distributor get(uint range) {
            Distributor d = distributors[range];
            if (d == null) {
                d = new Distributor(range);
                distributors[range] = d;
            }
            return d;
        }

        /**
         * For any number N < range, there is N+1 copies of it in the array, distributed as evenly as possible
         * /param range Range of valid numbers
         */
        protected Distributor(uint range) {
            uint index, rank, time;
            capacity = (range * (range + 1)) / 2;
            order = new short[capacity];

            ArrayUtilities.fill(ref order, (short)-1);
            index = capacity;

            for (rank = range; rank > 0; rank--) {
                uint capDivRank = capacity / rank;
                for (time = 0; time < rank; time++) {
                    index = (capDivRank + index) % capacity;
                    while (order[index] >= 0) {
                        index = (index + 1) % capacity;
                    }
                    order[index] = (short)(rank - 1);
                }
            }
        }

        /**
         * Get the next number according to the given index
         * /param index The current index
         * /return the random value
         */
        public short pick(uint index) {
            return order[index];
        }

        /**
         * Advance the index
         * /param index The current index
         * /return the next index
         */
        public uint next(uint index) {
            return (index + 1) % capacity;
        }
    }
}
