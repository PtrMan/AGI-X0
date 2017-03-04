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
            foreach (ITask iterationTask in tasks ) {
                EnumTaskStates taskState;
                // TODO< tune timeschedule
                iterationTask.processTask(this, 0.1, out taskState);
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
