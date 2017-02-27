using System.Collections.Generic;
using System.Linq;

namespace MetaNix.resourceManagement.compute {
    // central object to hold all tasks and provide global schedueling functionality.
    public class ComputeContext {
        public ComputeBudgetedTaskCollection computeBudgetedTasks = new ComputeBudgetedTaskCollection();
    }
}
