using System;
using System.Collections.Generic;

namespace MetaNix {
    class Ensure {
        static public void ensure(bool value) {
            if(!value) {
                throw new Exception();
            }
        }

        // if it fails it indicates an internal error and is because of some logical flaw in our substrate
        static public void ensureHard(bool value, string message = null) {
            if(!value) {
                string stringifiedMessage = message != null ? ": " + message : "";
                throw new Exception("Internal error" + stringifiedMessage); // TODO< throw internal exception
            }
        }
    }
}
