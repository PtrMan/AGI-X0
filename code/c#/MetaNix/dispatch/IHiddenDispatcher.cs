using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    interface IHiddenDispatcher {
        Node dispatch(HiddenFunctionId hiddenFunctionId, IList<Node> arguments);
    }
}
