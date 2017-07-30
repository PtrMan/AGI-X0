using System;

namespace MetaNix.nars.inference {
    public static class TruthFunctions {
        /**
         * {<A ==> B>} |- <B ==> A>
         * /param v1 Truth value of the premise
         * /return Truth value of the conclusion
         */
        public static TruthValue conversion(TruthValue v1) {
            float f1 = v1.frequency;
            float c1 = v1.confidence;
            float w = UtilityFunctions.and(f1, c1);
            float c = UtilityFunctions.w2c(w);
            return TruthValue.make(1, c);
        }

        /**
         * {A} |- (--A)
         * /param v1 Truth value of the premise
         * /return Truth value of the conclusion
         */
        public static TruthValue negation(TruthValue v1) {
            float f = 1 - v1.frequency;
            float c = v1.confidence;
            return TruthValue.make(f, c);
        }

        /**
         * {<A ==> B>} |- <(--, B) ==> (--, A)>
         * /param v1 Truth value of the premise
         * /return Truth value of the conclusion
         */
        public static TruthValue contraposition(TruthValue v1) {
            float f1 = v1.frequency;
            float c1 = v1.confidence;
            float w = UtilityFunctions.and(1 - f1, c1);
            float c = UtilityFunctions.w2c(w);
            return TruthValue.make(0, c);
        }


        /* ----- double argument functions, called in MatchingRules ----- */
        /**
         * {<S ==> P>, <S ==> P>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue revision(TruthValue v1, TruthValue v2) {
            return revision(v1, v2, TruthValue.make(0, 0));
        }

        public static TruthValue revision(TruthValue v1, TruthValue v2, TruthValue result) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float w1 = UtilityFunctions.c2w(v1.confidence);
            float w2 = UtilityFunctions.c2w(v2.confidence);
            float w = w1 + w2;
            result.frequency = (w1 * f1 + w2 * f2) / w;
            result.confidence = UtilityFunctions.w2c(w);
            return result;
        }

        /**
         * {<S ==> M>, <M ==> P>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue deduction(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2, f);
            return TruthValue.make(f, c);
        }

        /**
         * {M, <M ==> P>} |- P
         * /param v1 Truth value of the first premise
         * /param reliance Confidence of the second (analytical) premise
         * /return Truth value of the conclusion
         */
        public static TruthValue deduction(TruthValue v1, float reliance) {
            float f1 = v1.frequency;
            float c1 = v1.confidence;
            float c = UtilityFunctions.and(f1, c1, reliance);
            return TruthValue.make(f1, c, true);
        }

        /**
         * {<S ==> M>, <M <=> P>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue analogy(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2, f2);
            return TruthValue.make(f, c);
        }

        /**
         * {<S <=> M>, <M <=> P>} |- <S <=> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue resemblance(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2, UtilityFunctions.or(f1, f2));
            return TruthValue.make(f, c);
        }

        /**
         * {<S ==> M>, <P ==> M>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue abduction(TruthValue v1, TruthValue v2) {
            if (v1.analytic || v2.analytic) {
                return TruthValue.make(0.5f, 0f);
            }
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float w = UtilityFunctions.and(f2, c1, c2);
            float c = UtilityFunctions.w2c(w);
            return TruthValue.make(f1, c);
        }

        /**
         * {M, <P ==> M>} |- P
         * /param v1 Truth value of the first premise
         * /param reliance Confidence of the second (analytical) premise
         * /return Truth value of the conclusion
         */
        public static TruthValue abduction(TruthValue v1, float reliance) {
            if( v1.analytic ) {
                return TruthValue.make(0.5f, 0f);
            }
             float f1 = v1.frequency;
             float c1 = v1.confidence;
             float w = UtilityFunctions.and(c1, reliance);
             float c = UtilityFunctions.w2c(w);
            return TruthValue.make(f1, c, true);
        }

        /**
         * {<M ==> S>, <M ==> P>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static  TruthValue induction( TruthValue v1,  TruthValue v2) {
            return abduction(v2, v1);
        }

        /**
         * {<M ==> S>, <P ==> M>} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static  TruthValue exemplification( TruthValue v1,  TruthValue v2) {
            if( v1.analytic || v2.analytic ) {
                return TruthValue.make(0.5f, 0f);
            }
             float f1 = v1.frequency;
             float f2 = v2.frequency;
             float c1 = v1.confidence;
             float c2 = v2.confidence;
             float w = UtilityFunctions.and(f1, f2, c1, c2);
             float c = UtilityFunctions.w2c(w);
            return TruthValue.make(1, c);
        }

        /**
         * {<M ==> S>, <M ==> P>} |- <S <=> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue comparison( TruthValue v1,  TruthValue v2) {
             float f1 = v1.frequency;
             float f2 = v2.frequency;
             float c1 = v1.confidence;
             float c2 = v2.confidence;
             float f0 = UtilityFunctions.or(f1, f2);
             float f = (f0 == 0) ? 0 : (UtilityFunctions.and(f1, f2) / f0);
             float w = UtilityFunctions.and(f0, c1, c2);
             float c = UtilityFunctions.w2c(w);
            return TruthValue.make(f, c);
        }

        /* ----- desire-value functions, called in SyllogisticRules ----- */
        /**
         * A function specially designed for desire value [To be refined]
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue desireStrong(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2, f2);
            return TruthValue.make(f, c);
        }

        /**
         * A function specially designed for desire value [To be refined]
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue desireWeak(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2, f2, UtilityFunctions.w2c(1.0f));
            return TruthValue.make(f, c);
        }

        /**
         * A function specially designed for desire value [To be refined]
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue desireDed(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2);
            return TruthValue.make(f, c);
        }

        /**
         * A function specially designed for desire value [To be refined]
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static  TruthValue desireInd(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float w = UtilityFunctions.and(f2, c1, c2);
            float c = UtilityFunctions.w2c(w);
            return TruthValue.make(f1, c);
        }

        /* ----- double argument functions, called in CompositionalRules ----- */
        /**
         * {<M --> S>, <M <-> P>} |- <M --> (S|P)>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue union(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.or(f1, f2);
            float c = UtilityFunctions.and(c1, c2);
            return TruthValue.make(f, c);
        }

        /**
         * {<M --> S>, <M <-> P>} |- <M --> (S&P)>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue intersection(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float f2 = v2.frequency;
            float c1 = v1.confidence;
            float c2 = v2.confidence;
            float f = UtilityFunctions.and(f1, f2);
            float c = UtilityFunctions.and(c1, c2);
            return TruthValue.make(f, c);
        }

        /**
         * {(||, A, B), (--, B)} |- A
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue reduceDisjunction(TruthValue v1, TruthValue v2) {
             TruthValue v0 = intersection(v1, negation(v2));
            return deduction(v0, 1f);
        }

        /**
         * {(--, (&&, A, B)), B} |- (--, A)
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue reduceConjunction(TruthValue v1, TruthValue v2) {
            TruthValue v0 = intersection(negation(v1), v2);
            return negation(deduction(v0, 1f));
        }

        /**
         * {(--, (&&, A, (--, B))), (--, B)} |- (--, A)
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue reduceConjunctionNeg(TruthValue v1, TruthValue v2) {
            return reduceConjunction(v1, negation(v2));
        }

        /**
         * {(&&, <#x() ==> M>, <#x() ==> P>), S ==> M} |- <S ==> P>
         * /param v1 Truth value of the first premise
         * /param v2 Truth value of the second premise
         * /return Truth value of the conclusion
         */
        public static TruthValue anonymousAnalogy(TruthValue v1, TruthValue v2) {
            float f1 = v1.frequency;
            float c1 = v1.confidence;
            TruthValue v0 = TruthValue.make(f1, UtilityFunctions.w2c(c1));
            return analogy(v2, v0);
        }



        /**
         * From one moment to eternal
         * \param v1 Truth value of the premise
         * \return Truth value of the conclusion
         */
        public static EternalizedTruthValue eternalize(TruthValue v1) {
            float f1 = v1.frequency;
            float c1 = v1.confidence;
            float c = UtilityFunctions.w2c(c1);
            return EternalizedTruthValue.make(f1, c);
        }

        public static double temporalProjection(long sourceTime, long targetTime, long currentTime) {
            double a = 10000.0; // projection less strict as we changed in v2.0.0  10000.0 slower decay than 100000.0
            return 1.0 - (double)Math.Abs(sourceTime - targetTime) / (double)(Math.Abs(sourceTime - currentTime) + (double)Math.Abs(targetTime - currentTime) + a);
        }
    }
}
