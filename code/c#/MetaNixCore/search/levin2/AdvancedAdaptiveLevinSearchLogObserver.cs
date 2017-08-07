using System;

using MetaNix.framework.logging;

namespace MetaNix.search.levin2 {
    // logs levin search related events
    public class AdvancedAdaptiveLevinSearchLogObserver : IObserver {
        

        public AdvancedAdaptiveLevinSearchLogObserver(ILogger log) {
            this.log = log;
        }

        public void notify(params object[] arguments) {
            string messageType = (string)arguments[0];
            LevinSearchTask task = (LevinSearchTask)arguments[1];
            string taskName = (string)arguments[2];

            string message = "";

            if ( messageType == "success" ) {
                ulong iteration = task.levinSearchContext.searchIterations;

                message = string.Format("< Task:{0} (no meta)> success, {0} required< #iterations={1}, cputime=?, realtime=?>", taskName, iteration);
            }
            else if(messageType == "increaseProgramsize") {
                message = string.Format("<Task:{0} (no meta)> increaseProgramsize, {1}", taskName, task.levinSearchContext.numberOfInstructions);
            }
            else if(messageType == "failed") {
                message = string.Format("<Task:{0} (no meta)> failed, {1}", taskName, "?");
            }

            Logged logged = new Logged();
            logged.message = message;
            logged.notifyConsole = Logged.EnumNotifyConsole.YES;
            logged.origin = new string[] {"search", "ALS"};
            logged.serverity = Logged.EnumServerity.INFO;
            log.write(logged);
        }

        private ILogger log;
    }
}
