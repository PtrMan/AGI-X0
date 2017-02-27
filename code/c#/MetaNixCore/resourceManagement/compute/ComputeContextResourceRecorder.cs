using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaNix.resourceManagement.compute {
    // logs resource usage of the tasks hold by ComputeContext
    public class ComputeContextResourceRecorder {
        public ComputeContextResourceRecorder(ComputeContext computeContext) {
            this.computeContext = computeContext;
        }

        public class RecordedTaskInfo {
            public RecordedTaskInfo(double budgetPriority, string humanReadableUniqueName) {
                this.budgetPriority = budgetPriority;
                this.humanReadableUniqueName = humanReadableUniqueName;
            }

            public double budgetPriority;
            public string humanReadableUniqueName;
        }

        public class ResourceUsageRecord {
            public IDictionary<string, RecordedTaskInfo> recordedTaskInfosByName = new Dictionary<string, RecordedTaskInfo>();

            //public IList<RecordedTaskInfo> recordedTaskInfos;
            internal void setRecordedTaskInfos(IEnumerable<RecordedTaskInfo> recordedTaskInfos) {
                recordedTaskInfosByName.Clear();
                foreach(var iRecordedTaskInfo in recordedTaskInfos) {
                    Ensure.ensureHard(!recordedTaskInfosByName.ContainsKey(iRecordedTaskInfo.humanReadableUniqueName));
                    recordedTaskInfosByName[iRecordedTaskInfo.humanReadableUniqueName] = iRecordedTaskInfo;
                }
            }

            public float? sumOfbudgetPriorities;
        }

        // called every fixed interval (lets say one second)
        public void logResourceUsage() {
            ResourceUsageRecord createdResourceUsageRecord = new ResourceUsageRecord();
            createdResourceUsageRecord.setRecordedTaskInfos( computeContext.computeBudgetedTasks.list.Select(v => new RecordedTaskInfo(v.budget.priority, v.humanReadableUniqueName)) );
            createdResourceUsageRecord.sumOfbudgetPriorities = computeContext.computeBudgetedTasks.list.Select(v => (float)v.budget.priority).Aggregate((sum, current) => sum + current);
            records.Add(createdResourceUsageRecord);
        }

        public void resetLog() {
            records.Clear();
        }

        ComputeContext computeContext;

        public IList<ResourceUsageRecord> records = new List<ResourceUsageRecord>();
    }
}
