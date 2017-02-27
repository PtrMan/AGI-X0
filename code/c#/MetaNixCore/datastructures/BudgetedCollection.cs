using System.Collections.Generic;
using System.Linq;

using MetaNix.attention;

namespace MetaNix.datastructures {
    public class BudgetedCollection<Type> : BudgetedDatastructure<Type> where Type : HasBudget {
        public override Budget getBuget(Type element) {
            return element.budget;
        }

        protected override double protectedSumOfPriority() {
            return list.Select(v => v.budget.priority).Aggregate((sum, current) => sum + current);
        }

        public IList<Type> list = new List<Type>();
    }
}
