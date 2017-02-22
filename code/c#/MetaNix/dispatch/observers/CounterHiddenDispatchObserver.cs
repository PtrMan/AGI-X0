using System;
using System.Collections.Generic;

/*
namespace MetaNix.dispatch.observers {
    class CounterHiddenDispatchObserver : IHiddenDispatchObserver {
        public void dispatchEnter(HiddenFunctionId hiddenFunctionId, IList<Node> arguments) {
            incrementCallCounter(hiddenFunctionId);
        }

        public void dispatchExit(HiddenFunctionId hiddenFunctionId) {
        }

        public void resetCounters() {
            countersDictionary.Clear();
        }

        void incrementCallCounter(HiddenFunctionId functionId) {
            if (!countersDictionary.ContainsKey(functionId)) {
                countersDictionary[functionId] = 1;
                return;
            }
            countersDictionary[functionId]++;
        }

        Dictionary<HiddenFunctionId, long> countersDictionary = new Dictionary<HiddenFunctionId, long>();
    }
}
*/