using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MetaNix.nars {
    // intervals can be "anchored" in time with an absolute time -> isAnchored = false
    // or they can be 

    // [...)

    public class Interval {
        private double privateBeginRelative, privateEndRelative;
        private double relativeTime; // only valid if 

        // is the interval "anchored" in time or is it relative to something
        public bool isAnchored { get {
            return relativeTime != double.NaN;
        }}

        public bool isInstantaneous { get {
            return beginRelative == endRelative;
        } }

        // returns instantanious time
        public double instantaneousRelative { get {
            Debug.Assert(!isAnchored);
            Debug.Assert(isInstantaneous);
            return beginRelative;
        } }

        // returns instantanious time
        public double instantaneousAbsolute { get {
            Debug.Assert(isAnchored);
            Debug.Assert(isInstantaneous);
            return beginAbsolute;
        } }

        // only valid if it is an relative time
        public double beginAbsolute { get {
            Debug.Assert(isAnchored);
            return relativeTime + privateBeginRelative;
        } }

        // only valid if it is an relative time
        public double endAbsolute { get {
            Debug.Assert(isAnchored);
            return relativeTime + privateEndRelative;
        } }

        // only valid if it is not an relative time
        public double beginRelative { get {
            Debug.Assert(!isAnchored);
            return privateBeginRelative;
        } }

        // only valid if it is not an relative time
        public double endRelative { get {
            Debug.Assert(!isAnchored);
            return privateEndRelative;
        } }
        
        public Interval add(Interval other) {
            Debug.Assert(!other.isAnchored); // adding absolute time is illegal

            Interval result = new Interval();
            result.relativeTime = relativeTime;
            result.privateBeginRelative = privateBeginRelative + other.privateBeginRelative;
            result.privateEndRelative = privateEndRelative + other.privateEndRelative;
            return result;
        }

        public bool isInIntervalAbsolute(double absoluteTime) {
            Debug.Assert(isAnchored);
            return beginAbsolute <= absoluteTime && endAbsolute > absoluteTime;
        }

        public bool isInIntervalRelative(double relativeTime) {
            Debug.Assert(!isAnchored);
            return beginRelative <= relativeTime && endRelative > relativeTime;
        }

        /* uncommented because not used
        public  bool isAbsoluteTimeBefore(double absoluteTime) {
            assert(isAnchored);
            return absoluteTime < beginAbsolute;
        }
        */

        public bool isRelativeTimeBefore(double relativeTime) {
            Debug.Assert(!isAnchored);
            return relativeTime < beginRelative;
        }

        public bool isEqual(Interval other) {
            return beginRelative == other.beginRelative && endRelative == other.endRelative && relativeTime == other.relativeTime;
        }

        public  bool isAbsoluteTimeInside(double absolute) {
            Debug.Assert(isAnchored); // only defined for nonrelative intervals

            if (isInstantaneous) {
                return absolute == beginRelative;
            }
            return absolute >= beginAbsolute && absolute < endAbsolute;
        }

        public static Interval makeAbsolute(double begin, double end) {
            Interval result = new Interval();
            result.relativeTime = 0.0;
            result.privateBeginRelative = begin;
            result.privateEndRelative = end;
            return result;
        }

        public static Interval makeRelative(double begin, double end) {
            Interval result = new Interval();
            result.relativeTime = double.NaN;
            result.privateBeginRelative = begin;
            result.privateEndRelative = end;
            return result;
        }

        public static Interval makeInstantaneousRelative(double time) {
            Interval result = new Interval();
            result.relativeTime = double.NaN;
            result.privateBeginRelative = time;
            result.privateEndRelative = time;
            return result;
        }

        public static Interval makeInstantaneousRelative() {
            return makeInstantaneousRelative(0.0);
        }


        // example
        // 1 -> [0.5;1.5)
        // 1.0 -> [0.95;1.05)
        // 1.00 -> [0.995;1.005]

        // https://groups.google.com/forum/#!topic/open-nars/SLs8TyNwF7w
        // >A single point with accuracy is equivalent to an interval. For example, 1 corresponds to [0.5, 1.5), 1.0 to [0.95, 1.05), 1.00 to [0.995, 1.005), ...
        // 
        // https://groups.google.com/forum/#!topic/open-nars/tC9NNwgmCjc
        public static Interval createIntervalByBaseAndPrecision(double @base, double precision) {
            if (precision == double.NaN) { // instantaneous
                return Interval.makeInstantaneousRelative(@base);
            }

            Debug.Assert(precision >= 0.0); // else it's ill defined

            // calculate the whole range [0;r)
            double rangeBase = System.Math.Pow(10.0, -precision);
            // calculate the difference to minus and plus
            double rangeDiff = rangeBase * 0.5;

            double rangeMin = @base * (1.0 - rangeDiff);
            double rangeMax = @base * (1.0 + rangeDiff);
            return Interval.makeRelative(rangeMin, rangeMax);
        }
    }
}
