// pure levin search without any modifications
namespace MetaNix.search.levin {
    
    public abstract class Levin<RepresentationType> {
        public Levin(int maxLength) {
            this.maxLength = maxLength;
            this.enumeratedEncoding = new uint[]{}; // start with length zero
        }

        public void iteration(out bool isCompleted) {
            isCompleted = false;

            RepresentationType representation = decode(enumeratedEncoding);
            apply(representation);

            bool isLengthComplete;
            increment(ref enumeratedEncoding, out isLengthComplete);
            if( isLengthComplete ) {
                if( enumeratedEncoding.Length == maxLength ) {
                    isCompleted = true;
                    return;
                }

                enumeratedEncoding = new uint[enumeratedEncoding.Length+1];
            }
        }

        protected abstract void apply(RepresentationType representation);

        // decodes the enumerated encoding to the usecase specific representation
        protected abstract RepresentationType decode(uint[] enumeratedEncoding);

        protected abstract void increment(ref uint[] enumeratedEncoding, out bool isLengthCompleted);
        
        int maxLength;
        uint[] enumeratedEncoding;
    }
}
