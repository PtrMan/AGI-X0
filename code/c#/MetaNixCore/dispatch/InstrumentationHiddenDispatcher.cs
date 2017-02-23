using System;
using System.Collections.Generic;

using MetaNix.datastructures;

namespace MetaNix.dispatch {
    // observes dispatching calls
    public interface IHiddenDispatchObserver {
        void dispatchEnter(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments);
        void dispatchExit(HiddenFunctionId hiddenFunctionId, ImmutableNodeReferer resultOfCall);
    }

    // informs instrumentations about a dispatch
    public class InstrumentationHiddenDispatcher : IHiddenDispatcher {
        IHiddenDispatcher chainDispatcher;

        // by priority ordered observers, first ones are "closer" to the call
        public IList<IHiddenDispatchObserver> dispatchObservers = new List<IHiddenDispatchObserver>();

        public InstrumentationHiddenDispatcher(IHiddenDispatcher chainDispatcher) {
            this.chainDispatcher = chainDispatcher;
        }

        public ImmutableNodeReferer dispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments) {
            for(int i = dispatchObservers.Count-1;i>=0;i--) {
                dispatchObservers[i].dispatchEnter(hiddenFunctionId, arguments);
            }

            ImmutableNodeReferer result = chainDispatcher.dispatch(hiddenFunctionId, arguments);

            for (int i = 0; i < dispatchObservers.Count; i++) {
                dispatchObservers[i].dispatchExit(hiddenFunctionId, result);
            }

            return result;
        }
    }
}
