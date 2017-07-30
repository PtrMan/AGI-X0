using System;
using System.Diagnostics;

namespace MetaNix.nars {

    // a term referer can be
    // - index of  compound
    // - id of variable, can be independent or dependent
    public class TermOrCompoundTermOrVariableReferer {
        private int encoding;

        private const int NUMBEROFBITSFORID = 28; // enough concepts for now

        public enum EnumSpecialMaskBits {
            ATOMICTERM = NUMBEROFBITSFORID + 1, // for atomic terms
            INDEPENDENTVAR,
            DEPENDENTVAR,
        }

        public static TermOrCompoundTermOrVariableReferer makeAtomic(int value) {
            int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

            Debug.Assert((value & (~BITMAKSFORID)) == 0);
            TermOrCompoundTermOrVariableReferer result = new TermOrCompoundTermOrVariableReferer();
            result.encoding = value | (1 << (int)EnumSpecialMaskBits.ATOMICTERM);
            return result;
        }

        public static TermOrCompoundTermOrVariableReferer makeNonatomic(int value) {
            int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

            Debug.Assert((value & (~BITMAKSFORID)) == 0);
            TermOrCompoundTermOrVariableReferer result = new TermOrCompoundTermOrVariableReferer();
            result.encoding = value;
            return result;
        }

        public static TermOrCompoundTermOrVariableReferer makeIndependentVariable(int value) {
            int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

            Debug.Assert((value & (~BITMAKSFORID)) == 0);
            TermOrCompoundTermOrVariableReferer result = new TermOrCompoundTermOrVariableReferer();
            result.encoding = value | (1 << (int)EnumSpecialMaskBits.INDEPENDENTVAR);
            return result;
        }

        public static TermOrCompoundTermOrVariableReferer makeDependentVariable(int value) {
            int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

            Debug.Assert((value & (~BITMAKSFORID)) == 0);
            TermOrCompoundTermOrVariableReferer result = new TermOrCompoundTermOrVariableReferer();
            result.encoding = value | (1 << (int)EnumSpecialMaskBits.DEPENDENTVAR);
            return result;
        }

        // flags must not overlap with the id
        //D-code< static assert((BITMAKSFORID & (1 << EnumSpecialMaskBits.ATOMICTERM)) == 0);  >
        //D-code< static assert((BITMAKSFORID & (1 << EnumSpecialMaskBits.INDEPENDENTVAR)) == 0);  >
        //D-code< static assert((BITMAKSFORID & (1 << EnumSpecialMaskBits.DEPENDENTVAR)) == 0);  >

        // helper, TODO< move to other file >
        static private int bitmaskForBits(uint bits) {
		    return ((int)1 << (int)(bits + 1)) - 1;
	    }

        public bool isAtomic {
            get {
                return checkFlag(EnumSpecialMaskBits.ATOMICTERM);
            }
        }

        public bool isIndependentVariable {
            get {
                return checkFlag(EnumSpecialMaskBits.INDEPENDENTVAR);
            }
        }

        public bool isDependentVariable {
            get {
                return checkFlag(EnumSpecialMaskBits.DEPENDENTVAR);
            }
        }

        public bool isVariable {
            get {
                return isIndependentVariable || isDependentVariable;
            }
        }

        public bool hasVarQuery() {
            // TODO
            return false;
        }

        public bool isSpecial {
            get {
                return isVariable;
            }
        }

        public int getAtomic {
            get {
                Debug.Assert(isAtomic && !isSpecial);
                return maskOutId;
            }
        }

        public int getTerm {
            get {
                Debug.Assert(!isAtomic && !isSpecial);
                return maskOutId;
            }
        }

        // provides more generalization than just getAtomic and getTerm
        // ASK< maybe we should just reference terms? >
        public int getAtomicOrTerm {
            get {
                Debug.Assert(!isSpecial);
                return maskOutId;
            }
        }

        // provides more generalization than just getAtomic and getTerm
        // ASK< maybe we should just reference terms? >
        public int getMaskedOutId {
            get {
                return maskOutId;
            }
        }

        public int getDependentVariable {
            get {
                Debug.Assert(!isAtomic && isDependentVariable);
                return maskOutId;
            }
        }

        public int getIndependentVariable {
            get {
                Debug.Assert(!isAtomic && isIndependentVariable);
                return maskOutId;
            }
        }

        protected bool checkFlag(EnumSpecialMaskBits maskIndex) {
            return (encoding & (int)(1 << (int)maskIndex)) != 0;
        }

        protected int maskOutId {
            get {
                int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

                return (encoding & BITMAKSFORID);
            }
        }

        protected int maskedOutFlags {
            get {
                int BITMAKSFORID = bitmaskForBits(NUMBEROFBITSFORID);

                return (encoding & ~BITMAKSFORID);
            }
        }

        public int rawEncoding { get {
            return encoding;
        }}


        public static bool isSameWithoutId(TermOrCompoundTermOrVariableReferer a, TermOrCompoundTermOrVariableReferer b) {
            return a.maskedOutFlags == b.maskedOutFlags;
        }

        public static bool isSameWithId(TermOrCompoundTermOrVariableReferer a, TermOrCompoundTermOrVariableReferer b) {
            return isSameWithoutId(a, b) && a.maskOutId == b.maskOutId;
        }
    }
}
