using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaNix.scheduler {
    // from https://github.com/PtrMan/WhiteSphereEngine/blob/master/code/d/whiteSphereEngine/scheduler/Scheduler.d
    /** \brief Schedueler which is running on one Core/Thread
     *
     */
    public class Scheduler {
        IList<ITask> tasks = new List<ITask>();
        bool inLoop = false;
        Object sync = new Object();
        
        /** \brief do all work
         *
         * This Method do the work for all Tasks which are running
         *
         */
        public void process() {
            IList<ITask> finishedTasks = new List<ITask>();

            // remeber # of tasks to not process the tasks which are added by tasks
            int numberOfTasksToProcess = tasks.Count;
            for(int taskI = 0; taskI < numberOfTasksToProcess; taskI++) {
                ITask iterationTask = tasks[taskI];

                EnumTaskStates taskState;
                // TODO< tune timeschedule
                iterationTask.processTask(this, 0.1, out taskState);
                if (taskState == EnumTaskStates.FINISHED) {
                    finishedTasks.Add(iterationTask);
                }
            }
            
            // remove all finished tasks
            foreach(ITask iterationFinishedTask in finishedTasks) {
                tasks.Remove(iterationFinishedTask);
            }
        }

        /** \brief adds Thread-safe a task to the Tasklist
         *
         * \param pTask the Task to add
         */
        public void addTaskSync(ITask pTask) {
            lock(sync) {
                tasks.Add(pTask);
            }
        }

        /** \brief removes Thread-safe a task
         *
         * \param pTask the Task to remove
         */
        public void removeTaskSync(ITask pTask) {
            lock (sync) {
                tasks.Remove(pTask);
            }
        }

        public void flushTasksSync() {
            lock (sync) {
                tasks.Clear();
            }
        }
    }
}
