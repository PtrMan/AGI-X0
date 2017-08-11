using System;
using System.Collections.Generic;

using MetaNix.scheduler;
using MetaNix.search.levin2;
using MetaNix.weakAi.behaviorTree;
using MetaNix.framework.logging;

namespace MetaNix.control.levinProgramSearch {
    // starts an search and monitors the search for an program with levin search
    public class SearchTask : Task {
        // /param completitionObservable used to register ourself to get notified if an search got completed by another task
        public SearchTask(
            Scheduler scheduler,
            SparseArrayProgramDistribution sparseArrayProgramDistribution,
            AdvancedAdaptiveLevinSearchProblem problem,
            Observable completitionObservable,
            ILogger log
        ) {
            this.scheduler = scheduler;
            this.sparseArrayProgramDistribution = sparseArrayProgramDistribution;
            this.problem = problem;
            this.log = log;

            this.completitionObservable = completitionObservable;
            this.completitionObserver = new CompletitionObserver(this);
            completitionObservable.register(completitionObserver);
        }

        public override Task clone() {
            return new SearchTask(scheduler, sparseArrayProgramDistribution, problem, completitionObservable, log);
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
            levinSearchObservable.register(new AdvancedAdaptiveLevinSearchLogObserver(log));
            levinSearchObservable.register(new LevinSearchObserver(this));

            levinSearchTask = new LevinSearchTask(levinSearchObservable, problem.humanReadableTaskname);
            scheduler.addTaskSync(levinSearchTask);

            levinSearchTask.levinSearchContext = new LevinSearchContext();
            levinSearchTask.levinSearchContext.interpreterArguments.maxNumberOfRetiredInstructions = problem.maxNumberOfRetiredInstructions;
            levinSearchTask.levinSearchContext.interpreterArguments.interpreterState = problem.initialInterpreterState;
            levinSearchTask.levinSearchContext.trainingSamples = problem.trainingSamples;
            levinSearchTask.levinSearchContext.parentProgram = problem.parentProgram;
            fillUsedInstructionSet(levinSearchTask.levinSearchContext);
            levinSearchTask.levinSearchContext.initiateSearch(sparseArrayProgramDistribution, problem.enumerationMaxProgramLength);
        }

        void fillUsedInstructionSet(LevinSearchContext levinSearchContext) {
            levinSearchContext.instructionIndexToInstruction.Clear();

            uint instructionIndex = 0;

            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = 0; // RET

            // add all nonspecial instructions
            for( uint instruction = 6; instruction < 39; instruction++ ) {
                levinSearchContext.instructionIndexToInstruction[instructionIndex++] = instruction;
            }

            // add some special jump instructions

            // advance or exit
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(1, -4);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(1, -3);

            // not end or exit
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(2, -4);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(2, -5);

            // jump
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -0); // nop
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -2);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -3);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -4);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -5);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -6);
            // TODO< jump ahead >

            // jump if not flag
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, -2);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, -3);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, -4);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, -5);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, -6);

            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, +1);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, +2);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, +3);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, +4);
            levinSearchContext.instructionIndexToInstruction[instructionIndex++] = InstructionInterpreter.convInstructionAndRelativeToInstruction(4, +5);

            int breakpointHere1 = 1;
        }

        AdvancedAdaptiveLevinSearchProblem problem;
        private ILogger log;
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
