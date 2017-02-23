using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    public interface IHiddenDispatcher {
        ImmutableNodeReferer dispatch(HiddenFunctionId hiddenFunctionId, IList<ImmutableNodeReferer> arguments);
    }
}
