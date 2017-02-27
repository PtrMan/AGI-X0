using MetaNix.attention;

namespace MetaNix.datastructures {
    // something which has an budget
    abstract public class HasBudget {
        public Budget budget {
            get {
                return protectedGetBudget();
            }
        }

        abstract protected Budget protectedGetBudget();
    }
}
