using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaNix.attention;
using MetaNix.scheduler;

namespace MetaNix.resourceManagement.compute {
    // used for testing the schedueler and management code
    public class ComputeNoOperationPerformedBugetedTask : ComputeBudgetedTask {
        public ComputeNoOperationPerformedBugetedTask(Budget startBudget, string humanReadableUniqueName) : base(startBudget, humanReadableUniqueName) {
        }

        public override void processTask(Scheduler scheduler, double softTimelimitInSeconds, out EnumTaskStates taskState) {
            // do nothing
            taskState = EnumTaskStates.WAITNEXTFRAME;
        }
    }
}
