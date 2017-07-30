namespace MetaNix.nars {
    public struct TermTupleIndex {
        // using ulong for futureproof, 32 bit is enough for now, too
        public ulong index;

        public static TermTupleIndex make(ulong index) {
            TermTupleIndex result;
            result.index = index;
            return result;
        }

        public static TermTupleIndex makeInvalid() {
            TermTupleIndex result;
            result.index = ulong.MaxValue;
            return result;
        }

        public static bool operator==(TermTupleIndex a, TermTupleIndex b) {
            return a.index == b.index;
        }

        public static bool operator !=(TermTupleIndex a, TermTupleIndex b) {
            return !(a.index == b.index);
        }
    }

    public struct CompoundIndex {
        // using uint because it is enough for now
        public ulong index;

        public static CompoundIndex make(uint index) {
            CompoundIndex result;
            result.index = index;
            return result;
        }

        public static CompoundIndex makeInvalid() {
            CompoundIndex result;
            result.index = uint.MaxValue;
            return result;
        }
    }
}
