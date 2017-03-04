using MetaNix.attention;
using MetaNix.datastructures;
using MetaNix.scheduler;

namespace MetaNix.resourceManagement.compute {
    abstract public class ComputeBudgetedTask : HasBudget, ITask {
        public ComputeBudgetedTask(Budget startBudget, string humanReadableUniqueName) {
            privateBudget = startBudget;
            this.humanReadableUniqueName = humanReadableUniqueName;
        }

        protected override Budget protectedGetBudget() {
            return privateBudget;
        }

        public abstract void processTask(Scheduler scheduler, double softTimelimitInSeconds, out EnumTaskStates taskState);

        Budget privateBudget;
        internal string humanReadableUniqueName;
    }
}
