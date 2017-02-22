using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaNix.dispatch {
    class TimingAndCountHiddenDispatchObserver : IHiddenDispatchObserver {
        class Instrumentation {
            public long callcounter;
            public long calltimeMinInNs, calltimeMaxInNs, calltimeSumInNs;
        }

        Stopwatch sw = new Stopwatch();
        bool privateEnabledTimingInstrumentation;
        Dictionary<HiddenFunctionId, Instrumentation> countersDictionary = new Dictionary<HiddenFunctionId, Instrumentation>();


        public void dispatchEnter(HiddenFunctionId hiddenFunctionId, IList<Node> arguments) {
            incrementCallCounter(hiddenFunctionId);

            if (!privateEnabledTimingInstrumentation) {
                return;
            }
            sw.Restart();
        }

        public void dispatchExit(HiddenFunctionId hiddenFunctionId, Node resultOfCall) {
            if (!privateEnabledTimingInstrumentation) {
                return;
            }

            sw.Stop();
            double ticks = sw.ElapsedTicks;
            long elapsedNs = (long)((ticks / Stopwatch.Frequency) * 1000000000);

            Instrumentation instrumentation = countersDictionary[hiddenFunctionId];
            instrumentation.calltimeSumInNs += elapsedNs;
            instrumentation.calltimeMaxInNs = Math.Max(elapsedNs, instrumentation.calltimeMaxInNs);
            // needs special handling for the case when it's the first call
            if (instrumentation.callcounter == 1) {
                instrumentation.calltimeMinInNs = elapsedNs;
            }
            else {
                instrumentation.calltimeMinInNs = Math.Min(elapsedNs, instrumentation.calltimeMinInNs);
            }
        }


        // class to pass istrumentation informations to the outside
        public class InstrumentationDescriptor {
            public long callcounter;
            public long? calltimeMinInNs, calltimeMaxInNs, calltimeSumInNs;
        }

        public InstrumentationDescriptor getInstrumentation(HiddenFunctionId hiddenFunctionId) {
            InstrumentationDescriptor descriptor = new InstrumentationDescriptor();
            descriptor.callcounter = countersDictionary[hiddenFunctionId].callcounter;
            if (privateEnabledTimingInstrumentation) {
                descriptor.calltimeMaxInNs = countersDictionary[hiddenFunctionId].calltimeMaxInNs;
                descriptor.calltimeMinInNs = countersDictionary[hiddenFunctionId].calltimeMinInNs;
                descriptor.calltimeSumInNs = countersDictionary[hiddenFunctionId].calltimeSumInNs;
            }
            return descriptor;
        }


        public void resetCounters() {
            countersDictionary.Clear();
        }

        void incrementCallCounter(HiddenFunctionId functionId) {
            if (!countersDictionary.ContainsKey(functionId)) {
                countersDictionary[functionId] = new Instrumentation();
                countersDictionary[functionId].callcounter = 1;
                return;
            }
            countersDictionary[functionId].callcounter++;
        }

        // switching the timing instrumentation makes only sense with an total flush
        public void resetCountersAndSetEnableTimingInstrumentation(bool enableTimingInstrumentation) {
            privateEnabledTimingInstrumentation = enableTimingInstrumentation;
            resetCounters();
        }

        public IList<Tuple<HiddenFunctionId, long>> counters {
            get {
                IList<Tuple<HiddenFunctionId, long>> result = new List<Tuple<HiddenFunctionId, long>>(countersDictionary.Keys.Count);

                int i = 0;
                foreach (HiddenFunctionId iKey in countersDictionary.Keys) {
                    result[i] = new Tuple<HiddenFunctionId, long>(iKey, countersDictionary[iKey].callcounter);
                    i++;
                }

                return result;
            }
        }
    }
}
