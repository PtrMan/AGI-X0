using MetaNix.math;
using MetaNix.nars.config;
using System;

namespace MetaNix.nars {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_TonyAnticipationStrategy/nars_core/nars/entity/TruthValue.java
    public class TruthValue {
        public static TruthValue make(float frequency, float confidence, bool analytic = false) {
            TruthValue result = new TruthValue();
            result.frequency = frequency;
            result.confidence = confidence;
            result.analytic = analytic;
            return result;
        }

        public TruthValue clone() {
            TruthValue result = new TruthValue();
            result.frequency = frequency;
            result.confidence = confidence;
            result.analytic = analytic;
            return result;
        }

        /**
         * Calculate the expectation value of the truth value
         *
         * @return The expectation value
         */
        public float expectation { get {
            return (confidence * (frequency - 0.5f) + 0.5f);
        } }

        /**
         * Calculate the absolute difference of the expectation value and that of a
         * given truth value
         *
         * @param t The given value
         * @return The absolute difference
         */
        public float getExpDifAbs(TruthValue t) {
            return Math.Abs(expectation - t.expectation);
        }

        public float frequency, confidence;

        /**
         * Whether the truth value is derived from a definition
         */
        public bool analytic = false;

        // from https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/TruthValue.java#L183
        public bool checkEquals(TruthValue other) {
            return
                MathMisc.checkEpsilon(confidence, other.confidence, Parameters.TRUTH_EPSILON) &&
                MathMisc.checkEpsilon(frequency, other.frequency, Parameters.TRUTH_EPSILON);
        }
    }
}
