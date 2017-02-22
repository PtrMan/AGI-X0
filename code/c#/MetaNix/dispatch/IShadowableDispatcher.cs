using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    // used to shadow function invokations from other handlers
    interface IShadowableDispatcher {
        /**
         * \param wasShadowed got the call fullfilled by the implementation
         */
        Node tryDispatch(HiddenFunctionId hiddenFunctionId, IList<Node> arguments, out bool wasShadowed);

        // is called if no shadowable dispatcher got called
        void informCompleteFallThroughWithResult(HiddenFunctionId hiddenFunctionId, IList<Node> arguments, Node result);
    }
}
