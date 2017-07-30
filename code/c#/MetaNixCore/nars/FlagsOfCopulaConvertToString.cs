using System;

namespace MetaNix.nars {
    public static class FlagsOfCopulaConvertToString {
        public static string convToHumanString(this FlagsOfCopula flagsOfCopula) {
            if (flagsOfCopula.isInheritance) {
                return "-->";
            }
            else if (flagsOfCopula.isSimilarity) {
                return "<->";
            }
            else if (flagsOfCopula.isImplication) {
                return "==>";
            }
            else if (flagsOfCopula.isEquivalence) {
                return "<=>";
            }
            //else if (flagsOfCopula.isConjection) {
            //    return "&&";
            //}
            else {
                throw new Exception("Internal error - unimplemented");
            }
        }

    }
}
