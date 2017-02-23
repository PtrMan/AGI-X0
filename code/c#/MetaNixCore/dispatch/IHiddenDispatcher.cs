using System;
using System.Collections.Generic;

using MetaNix.datastructures;

namespace MetaNix.dispatch {
    public interface IHiddenDispatcher {
        ImmutableNodeReferer dispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments);
    }
}
