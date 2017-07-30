using System.Diagnostics;

namespace MetaNix.nars {
    public class FlagsOfCopula {
        //              product
        bool nal1or2; // --> <->
        bool nal5;    // ==> <=>
        bool nal7;    // =/> <|>      (&/, ...)

        bool arrowLeft, arrowRight;
        bool product; // can be a NAL5 product or a NAL7 product(which is an sequence)

        bool isConjection;

        // uncommented because TOINTEGRATE
        //bool isList; 
        // # 
        // as introduced by pei
        // https://groups.google.com/forum/#!topic/open-nars/S8c6P5ndy5o

        // uncommented because TOINTEGRATE
        //bool isOperator;  // narsese sign is "^"
        // used for the 'name' of an operator

        // TODO< put into misc helper >
        static uint bool2uint(bool v) {
            return v ? (uint)1 : 0;
        }

        public  uint asNumberEncoding { get {
            return bool2uint(nal1or2) | (bool2uint(nal5) << 1) | (bool2uint(arrowLeft) << 2) | (bool2uint(arrowRight) << 3) | (bool2uint(isConjection) << 4);
        } }

        private FlagsOfCopula(){}
        public FlagsOfCopula(bool nal1or2, bool nal5, bool arrowLeft, bool arrowRight, bool isConjection, bool nal7 = false) {
            this.nal1or2 = nal1or2;
            this.nal5 = nal5;
            this.arrowLeft = arrowLeft;
            this.arrowRight = arrowRight;
            this.isConjection = isConjection;
            this.nal7 = nal7;
        }


        public static FlagsOfCopula makeInheritance() {
            FlagsOfCopula result = new FlagsOfCopula();
            result.nal1or2 = true;
            result.arrowRight = true;
            return result;
        }

        public bool isInheritance { get {
		    return nal1or2 && arrowRight && !arrowLeft;
	    }}

        public static FlagsOfCopula makeSimilarity() {
            FlagsOfCopula result = new FlagsOfCopula();
            result.nal1or2 = true;
            result.arrowLeft = true;
            result.arrowRight = true;
            return result;
        }

        public bool isSimilarity{ get {
		    return nal1or2 && arrowRight && arrowLeft;
	    }}


        public static FlagsOfCopula makeImplication() {
            FlagsOfCopula result = new FlagsOfCopula();
            result.nal5 = true;
            result.arrowRight = true;
            return result;
        }

        public bool isImplication{ get {
		    return nal5 && arrowRight && !arrowLeft;
	    }}

        public static FlagsOfCopula makeEquivalence() {
            FlagsOfCopula result = new FlagsOfCopula();
            result.nal5 = true;
            result.arrowLeft = true;
            result.arrowRight = true;
            return result;
        }

        public bool isEquivalence { get {
		    return nal5 && arrowRight && arrowLeft;
	    }}


        public static FlagsOfCopula makeConjuction() {
            FlagsOfCopula result = new FlagsOfCopula();
            result.isConjection = true;
            return result;
        }



        public bool isProduct{ get {
            Debug.Assert(nal5 != nal7);

		    return nal5 && product;
	    }}

	    public bool isSequence { get {
            Debug.Assert(nal5 != nal7);

		    return nal7 && product;
	    }}

        public static bool operator ==(FlagsOfCopula left, FlagsOfCopula right) {
            return
                left.nal1or2 == right.nal1or2 &&
                left.nal5 == right.nal5 &&
                left.nal7 == right.nal7 &&

                left.arrowLeft == right.arrowLeft &&
                left.arrowRight == right.arrowRight &&
                left.product == right.product &&

                left.isConjection == right.isConjection;
        }

        public static bool operator !=(FlagsOfCopula left, FlagsOfCopula right) {
            return !(left == right);
        }
    }
}
