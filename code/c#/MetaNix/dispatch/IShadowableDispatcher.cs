using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    // used to shadow function invokations from other handlers
    public interface IShadowableDispatcher {
        /**
         * \param wasShadowed got the call fullfilled by the implementation
         */
        ImmutableNodeReferer tryDispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments, out bool wasShadowed);

        // is called if no shadowable dispatcher got called
        void informCompleteFallThroughWithResult(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments, ImmutableNodeReferer result);
    }
}
