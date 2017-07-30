using System;

using MetaNix.nars.config;

namespace MetaNix.nars.inference {
    // https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/inference/UtilityFunctions.java
    public static class UtilityFunctions {
        public static float and(float a, float b) {
            return a * b;
        }

        public static float and(float a, float b, float c) {
            return a * b * c;
        }

        public static float and(float a, float b, float c, float d) {
            return a * b * c * d;
        }

        /**
         * A function where the output is disjunctively determined by the inputs
         * \param arr The inputs, each in [0, 1]
         * \return The output that is no smaller than each input
         */
        public static float or(params float[] arr) {
            float product = 1;
            foreach (float f in arr) {
                product *= (1 - f);
            }
            return 1 - product;
        }


        public static float or(float a, float b) {
            return 1f - ((1f - a) * (1f - b));
        }

        //may be more efficient than the for-loop version above, for 3 params
        public static float aveGeo(float a, float b, float c) {
            return (float)Math.Pow(a * b * c, 1.0 / 3.0);
        }

        //may be more efficient than the for-loop version above, for 2 params
        public static float aveAri(float a, float b) {
            return (a + b) / 2f;
        }

        /**
         * A function to convert weight to confidence
         * \param w Weight of evidence, a non-negative real number
         * \return The corresponding confidence, in [0, 1)
         */
        public static float w2c(float w) {
            return w / (w + Parameters.HORIZON);
        }

        /**
         * A function to convert confidence to weight
         * \param c confidence, in [0, 1)
         * \return The corresponding weight of evidence, a non-negative real number
         */
        public static float c2w(float confidence) {
            return Parameters.HORIZON * confidence / (1 - confidence);
        }

    }
}
