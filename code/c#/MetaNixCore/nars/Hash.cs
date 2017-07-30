namespace MetaNix.nars {
    class Hash {
        // http://stackoverflow.com/questions/664014/what-integer-hash-function-are-good-that-accepts-an-integer-hash-key
        public static uint hash_(uint x) {
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x);
            return x;
        }
    }
}
