using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    // observes dispatching calls
    interface IHiddenDispatchObserver {
        void dispatchEnter(HiddenFunctionId hiddenFunctionId, IList<Node> arguments);
        void dispatchExit(HiddenFunctionId hiddenFunctionId, Node resultOfCall);
    }

    // informs instrumentations about a dispatch
    class InstrumentationHiddenDispatcher : IHiddenDispatcher {
        IHiddenDispatcher chainDispatcher;

        // by priority ordered observers, first ones are "closer" to the call
        public IList<IHiddenDispatchObserver> dispatchObservers = new List<IHiddenDispatchObserver>();

        public InstrumentationHiddenDispatcher(IHiddenDispatcher chainDispatcher) {
            this.chainDispatcher = chainDispatcher;
        }

        public Node dispatch(HiddenFunctionId hiddenFunctionId, IList<Node> arguments) {
            for(int i = dispatchObservers.Count-1;i>=0;i--) {
                dispatchObservers[i].dispatchEnter(hiddenFunctionId, arguments);
            }

            Node result = chainDispatcher.dispatch(hiddenFunctionId, arguments);

            for (int i = 0; i < dispatchObservers.Count; i++) {
                dispatchObservers[i].dispatchExit(hiddenFunctionId, result);
            }

            return result;
        }
    }
}
