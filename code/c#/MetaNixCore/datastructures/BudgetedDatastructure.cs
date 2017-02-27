using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaNix.attention;

namespace MetaNix.datastructures {
    // Datastructure which holds elements which have a budget
    abstract public class BudgetedDatastructure<Type> {
        public abstract Budget getBuget(Type element);

        public double sumOfPriorities {
            get {
                return protectedSumOfPriority();
            }
        }

        protected abstract double protectedSumOfPriority();
    }
}
