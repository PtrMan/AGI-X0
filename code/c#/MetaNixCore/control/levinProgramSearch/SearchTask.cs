using System;
using System.Collections.Generic;

using MetaNix.scheduler;
using MetaNix.search.levin2;
using MetaNix.weakAi.behaviorTree;

namespace MetaNix.control.levinProgramSearch {
    // starts an search and monitors the search for an program with levin search
    public class SearchTask : Task {
        // /param completitionObservable used to register ourself to get notified if an search got completed by another task
        public SearchTask(
            Scheduler scheduler,
            SparseArrayProgramDistribution sparseArrayProgramDistribution,
            AdvancedAdaptiveLevinSearchProblem problem,
            Observable completitionObservable
        ) {
            this.scheduler = scheduler;
            this.sparseArrayProgramDistribution = sparseArrayProgramDistribution;
            this.problem = problem;

            this.completitionObservable = completitionObservable;
            this.completitionObserver = new CompletitionObserver(this);
            completitionObservable.register(completitionObserver);
        }

        public override Task clone() {
            return new SearchTask(scheduler, sparseArrayProgramDistribution, problem, completitionObservable);
        }

        public override void reset() {
            wasStarted = false;
        }

        public override EnumReturn run(EntityContext context) {
            if( abortSearch ) { // if the search was forced to be aborted because some task found a solution
                if(levinSearchTask != null) {
                    scheduler.removeTaskSync(levinSearchTask);
                    levinSearchTask = null;
                }

                // if the search was done by ourself we return success
                if (notifiedSuccess) {
                    return EnumReturn.SUCCESS;
                }

                return EnumReturn.FAILURE;
            }

            if( wasStarted ) {
                if( notifiedSuccess ) {
                    if (levinSearchTask != null) {
                        scheduler.removeTaskSync(levinSearchTask);
                        levinSearchTask = null;
                    }

                    // notify all other searches that the search was successful
                    completitionObservable.notify();

                    return EnumReturn.SUCCESS;
                }
                else if( notifiedFailure ) {
                    if (levinSearchTask != null) {
                        scheduler.removeTaskSync(levinSearchTask);
                        levinSearchTask = null;
                    }

                    return EnumReturn.FAILURE;
                }
                return EnumReturn.RUNNING;
            }
            else {
                wasStarted = true;
                submitTask(problem);
                return EnumReturn.RUNNING;
            }
        }

        void submitTask(AdvancedAdaptiveLevinSearchProblem problem) {
            Observable levinSearchObservable = new Observable();
            levinSearchObservable.register(new AdvancedAdaptiveLevinSearchLogObserver());
            levinSearchObservable.register(new LevinSearchObserver(this));

            levinSearchTask = new LevinSearchTask(levinSearchObservable);
            scheduler.addTaskSync(levinSearchTask);

            levinSearchTask.levinSearchContext = new LevinSearchContext();
            levinSearchTask.levinSearchContext.interpreterArguments.maxNumberOfRetiredInstructions = problem.maxNumberOfRetiredInstructions;
            levinSearchTask.levinSearchContext.interpreterArguments.interpreterState = problem.initialInterpreterState;
            levinSearchTask.levinSearchContext.instructionsetCount = problem.instructionsetCount;
            levinSearchTask.levinSearchContext.trainingSamples = problem.trainingSamples;
            levinSearchTask.levinSearchContext.parentProgram = problem.parentProgram;
            levinSearchTask.levinSearchContext.initiateSearch(sparseArrayProgramDistribution, problem.enumerationMaxProgramLength);
        }

        AdvancedAdaptiveLevinSearchProblem problem;
        Scheduler scheduler;
        SparseArrayProgramDistribution sparseArrayProgramDistribution;

        LevinSearchTask levinSearchTask; // the task for the scheduler which does the levin search
                                         // we need to have a handle to it to be able to finish it if/when the other searches have completed

        bool notifiedSuccess, notifiedFailure;
        bool abortSearch;

        bool wasStarted = false;
        private Observable completitionObservable;
        private CompletitionObserver completitionObserver;

        internal void notifyProgramSearchSuccess() {
            notifiedSuccess = true;
        }

        internal void notifyProgramSearchFailure() {
            notifiedFailure = true;
        }

        internal void notifyAbortBecauseSearchCompletedByOurselfOrOtherSearchTask() {
            abortSearch = true;

            if (levinSearchTask != null) {
                scheduler.removeTaskSync(levinSearchTask);
                levinSearchTask = null;
            }
        }
    }


    internal class CompletitionObserver : IObserver {
        private SearchTask searchTask;

        public CompletitionObserver(SearchTask searchTask) {
            this.searchTask = searchTask;
        }

        public void notify(params object[] arguments) {
            searchTask.notifyAbortBecauseSearchCompletedByOurselfOrOtherSearchTask();
        }
    }
    

    // is notified by the progress of the levin search and notifies our SearchTask
    class LevinSearchObserver : IObserver {
        public LevinSearchObserver(SearchTask searchTask) {
            this.searchTask = searchTask;
        }

        public void notify(params object[] arguments) {
            string messageType = (string)arguments[0];
            LevinSearchTask task = (LevinSearchTask)arguments[1];
            string taskName = (string)arguments[2];
            if (messageType == "success") {
                searchTask.notifyProgramSearchSuccess();
            }
            else if (messageType == "increaseProgramsize") {
            }
            else if (messageType == "failed") {
                searchTask.notifyProgramSearchFailure();
            }
        }

        SearchTask searchTask;
    }
}
