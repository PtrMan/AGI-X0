using System;

using MetaNix.nars.inference;

namespace MetaNix.nars {
    public class RuleTable {
        public enum EnumTruthFunction {
            REVISION,
            COMPARISON,
            ANALOGY,
            ANALOGYTICK,
            DEDUCTION2,
            ABDUCTION,
            INDUCTION,
            INTERSECTION,

            STRUCTINT,
            STRUCTABD,
            DEDUCTION,
            CONVERSION,
            EXEMPLIFICATION,
            REDUCECONJUNCTION,
            RESEMBLANCE,
        }

        public static TruthValue calcTruthDoublePremise(TruthValue a, TruthValue b, EnumTruthFunction truthFunction) {
            switch(truthFunction) {
                case EnumTruthFunction.REVISION:
                return TruthFunctions.revision(a, b);

                case EnumTruthFunction.COMPARISON:
                return TruthFunctions.comparison(a, b);

                case EnumTruthFunction.ANALOGY:
                return TruthFunctions.analogy(a, b);

                case EnumTruthFunction.ANALOGYTICK:
                return TruthFunctions.anonymousAnalogy(a, b);

                case EnumTruthFunction.DEDUCTION2:
                return TruthFunctions.desireDed(a, b);

                case EnumTruthFunction.INDUCTION:
                return TruthFunctions.induction(a, b);

                case EnumTruthFunction.INTERSECTION:
                return TruthFunctions.intersection(a, b);

                // TODO< search formulas >
                //case EnumTruthFunction.STRUCTINT:
                //case EnumTruthFunction.STRUCTABD:
                //case EnumTruthFunction.REDUCECONJUNCTION,

                case EnumTruthFunction.EXEMPLIFICATION:
                return TruthFunctions.exemplification(a, b);
                
                case EnumTruthFunction.RESEMBLANCE:
                return TruthFunctions.resemblance(a, b);

                default:
                throw new Exception("Double premise truth Function called for non-double premise truth");
            }
        }
    }
}
