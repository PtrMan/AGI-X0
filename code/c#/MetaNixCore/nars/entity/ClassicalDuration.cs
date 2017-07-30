using System;

namespace MetaNix.nars.entity {
    public class ClassicalDuration{
        public uint value;


        /** this represents the amount of time in proportion to a duration in which
         *  Interval resolution calculates.  originally, NARS had a hardcoded duration of 5
         *  and an equivalent Interval scaling factor of ~1/2 (since ln(E) ~= 1/2 * 5).
         *  Since duration is now adjustable, this factor approximates the same result
         *  with regard to a "normalized" interval scale determined by duration.
         */
        double linear = 0.5;

        double log; //caches the value here
        int lastValue = -1;

        public ClassicalDuration() {
        }


        public ClassicalDuration(uint v) {
                value = v;
        }

        public void setLinear(double linear) {
            this.linear = linear;
            update();
        }

        protected void update() {
            uint val = value;
            lastValue = (int)val;
            this.log = Math.Log(val * linear);
        }

        public double getSubDurationLog() {
            uint val = value;
            if (lastValue != val) {
                update();
            }
            return log;
        }

    }
}
