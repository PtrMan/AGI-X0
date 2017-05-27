using System.Collections.Generic;

namespace MetaNix.common {
    // from gameengine
    /**
     * Blackboard for communication
     *
     */
    public class Blackboard {
        /**
         * updates the Value associated with the Key
         *
         * if the key doesn't exists it will added
         *
         * \param Key ...
         * \param Value ...
         */
        public void update(string key, object value) {
            dictionary[key] = value;
        }

        /**
         * tries to return the data associated with the Key
         * asserts if the Key is invalid 
         *
         * \param Key ...
         * \return ...
         */
        public object access(string key) {
            return dictionary[key];
        }

        IDictionary<string, object> dictionary = new Dictionary<string, object>();
    }
}
