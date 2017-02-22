using System;
using System.Collections.Generic;
using MetaNix.dispatch;

namespace MetaNix {
    // tries to lookup the value
    class MemoizerShadowableDispatcher : IShadowableDispatcher {
        public void informCompleteFallThroughWithResult(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments, ImmutableNodeReferer result) {
            if (hiddenFunctionId != functionId) {
                return;
            }
            lookup.memoize(arguments, result);
        }

        public ImmutableNodeReferer tryDispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments, out bool wasShadowed) {
            wasShadowed = false;
            if( hiddenFunctionId != functionId ) {
                return null;
            }

            ImmutableNodeReferer lookupResult = lookup.tryLookup(arguments);
            if( lookupResult == null ) {
                return null;
            }

            wasShadowed = true;
            return lookupResult;
        }

        HiddenFunctionId functionId;
        public MemoizationLookup lookup;
    }
}
