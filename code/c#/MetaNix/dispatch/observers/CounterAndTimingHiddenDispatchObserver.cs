using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaNix.dispatch {
    class TimingAndCountHiddenDispatchObserver : IHiddenDispatchObserver {
        class Instrumentation {
            public HiddenFunctionId functionId;

            public int toIgnoreCounter;
            public long callcounter;
            public int callcounterIgnored;
            public long statisticsCalltimeMinInNs, statisticsCalltimeMaxInNs, statisticsCalltimeSumInNs;
        }

        Stopwatch sw = new Stopwatch();
        bool privateEnabledTimingInstrumentation;
        Dictionary<HiddenFunctionId, Instrumentation> countersDictionary = new Dictionary<HiddenFunctionId, Instrumentation>();

        int ignoredFirstCalls = 1;

        public void dispatchEnter(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments) {
            incrementCallCounter(hiddenFunctionId);

            if (!privateEnabledTimingInstrumentation) {
                return;
            }
            sw.Restart();
        }

        public void dispatchExit(HiddenFunctionId hiddenFunctionId, ImmutableNodeReferer resultOfCall) {
            if (!privateEnabledTimingInstrumentation) {
                return;
            }

            sw.Stop();
            double ticks = sw.ElapsedTicks;
            long elapsedNs = (long)((ticks / Stopwatch.Frequency) * 1000000000);

            Instrumentation instrumentation = countersDictionary[hiddenFunctionId];
            if(instrumentation.toIgnoreCounter == 0) {
                instrumentation.statisticsCalltimeSumInNs += elapsedNs;
                instrumentation.statisticsCalltimeMaxInNs = Math.Max(elapsedNs, instrumentation.statisticsCalltimeMaxInNs);
                // needs special handling for the case when it's the first call
                if (instrumentation.callcounter == 1) {
                    instrumentation.statisticsCalltimeMinInNs = elapsedNs;
                }
                else {
                    instrumentation.statisticsCalltimeMinInNs = Math.Min(elapsedNs, instrumentation.statisticsCalltimeMinInNs);
                }
            }
        }


        // class to pass istrumentation informations to the outside
        public class InstrumentationDescriptor {
            public HiddenFunctionId functionId;

            public long callcounter;
            public int callcounterIgnored; // number of statistical ignored calls

            public long callcounterPlusIgnored {
                get {
                    return callcounter + (long)callcounterIgnored;
                }
            }

            public long? calltimeMinInNs, calltimeMaxInNs, calltimeSumInNs;
        }

        public IList<InstrumentationDescriptor> instrumentations {
            get {
                IList<InstrumentationDescriptor> result = new List<InstrumentationDescriptor>(countersDictionary.Keys.Count);

                int i = 0;
                foreach (HiddenFunctionId iKey in countersDictionary.Keys) {
                    result.Add(getInstrumentation(iKey));
                    i++;
                }

                return result;
            }
        }

        public InstrumentationDescriptor getInstrumentation(HiddenFunctionId hiddenFunctionId) {
            InstrumentationDescriptor descriptor = new InstrumentationDescriptor();
            descriptor.callcounter = countersDictionary[hiddenFunctionId].callcounter;
            descriptor.callcounterIgnored = countersDictionary[hiddenFunctionId].callcounterIgnored;
            descriptor.functionId = countersDictionary[hiddenFunctionId].functionId;
            if (privateEnabledTimingInstrumentation) {
                descriptor.calltimeMaxInNs = countersDictionary[hiddenFunctionId].statisticsCalltimeMaxInNs;
                descriptor.calltimeMinInNs = countersDictionary[hiddenFunctionId].statisticsCalltimeMinInNs;
                descriptor.calltimeSumInNs = countersDictionary[hiddenFunctionId].statisticsCalltimeSumInNs;
            }
            return descriptor;
        }


        public void resetCounters() {
            countersDictionary.Clear();
        }

        void incrementCallCounter(HiddenFunctionId functionId) {
            if (!countersDictionary.ContainsKey(functionId)) {
                countersDictionary[functionId] = new Instrumentation();
                countersDictionary[functionId].callcounter = 0;
                countersDictionary[functionId].functionId = functionId;
                countersDictionary[functionId].toIgnoreCounter = ignoredFirstCalls;
            }

            Ensure.ensureHard(countersDictionary[functionId].toIgnoreCounter >= 0);
            if (countersDictionary[functionId].toIgnoreCounter == 0) {
                countersDictionary[functionId].callcounter++;
            }
            else {
                countersDictionary[functionId].callcounterIgnored++;
            }

            countersDictionary[functionId].toIgnoreCounter = Math.Max(0, countersDictionary[functionId].toIgnoreCounter-1);
        }

        // switching the timing instrumentation makes only sense with an total flush
        public void resetCountersAndSetEnableTimingInstrumentation(bool enableTimingInstrumentation) {
            privateEnabledTimingInstrumentation = enableTimingInstrumentation;
            resetCounters();
        }
    }
}
