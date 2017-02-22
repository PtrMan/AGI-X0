using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    interface IHiddenDispatcher {
        ImmutableNodeReferer dispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments);
    }
}
