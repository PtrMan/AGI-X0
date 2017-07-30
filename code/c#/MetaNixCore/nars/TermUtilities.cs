namespace MetaNix.nars {
    // utilities we need, not found in OpenNARS
    internal static class TermUtilities {
        public static bool isTermCompoundTerm(TermOrCompoundTermOrVariableReferer term) {
            if( term.isAtomic ) {
                return false;
            }

            if( term.isSpecial || term.isVariable ) {
                return false;
            }

            return true;
        }
    }
}
