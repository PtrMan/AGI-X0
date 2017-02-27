using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.resourceManagement.compute {
    // triggers the execution of tasks
    public class ComputeExecutor {
        public ComputeExecutor(ComputeContext context) {
            this.context = context;
            privateRecorder = new ComputeContextResourceRecorder(context);
        }

        public void run(double runForApproxWallclockTimeInSeconds) {
            executeForTargetDuration(runForApproxWallclockTimeInSeconds);
            budgetTimeStep(runForApproxWallclockTimeInSeconds);
            recordResourceUsage();
        }

        private void recordResourceUsage() {
            recorder.logResourceUsage();
        }

        private void executeForTargetDuration(double runForApproxWallclockTimeInSeconds) {
            // TODO
        }

        void budgetTimeStep(double timedeltaInSeconds) {
            foreach( ComputeBudgetedTask iBudgeted in context.computeBudgetedTasks.list ) {
                iBudgeted.budget.recalcPriorityByDeltaInSeconds(timedeltaInSeconds);
            }
        }

        public ComputeContextResourceRecorder recorder {
            get {
                return privateRecorder;
            }
        }

        ComputeContextResourceRecorder privateRecorder;
        public ComputeContext context;
    }
}
