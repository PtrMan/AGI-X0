using System;

namespace MetaNix.search.levin2 {
    // logs levin search related events
    public class AdvancedAdaptiveLevinSearchLogObserver : IObserver {
        public void notify(params object[] arguments) {
            string messageType = (string)arguments[0];
            LevinSearchTask task = (LevinSearchTask)arguments[1];
            string taskName = (string)arguments[2];
            if ( messageType == "success" ) {
                ulong iteration = task.levinSearchContext.searchIterations;

                Console.Write("[search - AALS]<Task:{0} (no meta)> success, ", taskName);
                Console.WriteLine("required< #iterations={0}, cputime=?, realtime=?>", iteration);
            }
            else if(messageType == "increaseProgramsize") {
                Console.WriteLine("[search - AALS]<Task:{0} (no meta)> increaseProgramsize, ", taskName);
            }
            else if(messageType == "failed") {
                Console.WriteLine("[search - AALS]<Task:{0} (no meta)> failed, ", taskName);
            }
        }
    }
}
