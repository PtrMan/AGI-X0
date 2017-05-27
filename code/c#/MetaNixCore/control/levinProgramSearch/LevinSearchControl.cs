using System.Linq;

using MetaNix.weakAi.behaviorTree;
using MetaNix.weakAi.behaviorTree.decoration;
using MetaNix.scheduler;
using MetaNix.search.levin2;
using System;

namespace MetaNix.control.levinProgramSearch {
    /**
     * controls the levin search for the program search
     * 
     */
    /*
     * The current implementation controls it with behavior trees
     * 
     */
    class LevinSearchControl {
        public static Task buildBehaviorTask(
            Scheduler scheduler,
            SparseArrayProgramDistribution sparseArrayProgramDistribution,
            AdvancedAdaptiveLevinSearchProgramDatabase database,
            AdvancedAdaptiveLevinSearchProblem problem
        ) {
            // used by all tasks to inform other tasks (and the same task) that a search was successful, so all other tasks abort their search too
            Observable completitionObservable = new Observable();

            /*
             * * start direct search
             * * start at max 3 searches which use the already present functions, searches go up to programlength = 4 (inclusive ret)
             */

            uint maximalParallelSearchesForIndirectPrograms = 3;

            Parallel parallel = new Parallel();
            parallel.maxCounterFailure = maximalParallelSearchesForIndirectPrograms;
            parallel.maxCounterSuccess = 1; // we just need one successful
            
            // query for related already solved problem with programs which solve it
            var databaseEntriesWithProgramsOfSimilarProblems = database.getQuery()
                .whereHumanReadableHintsAny(problem.humanReadableHints)
                .enumerable;

            Random random = new Random();

            // fill tasks

            throw new NotImplementedException("TODO");
            /* TODO
            foreach ( var iterationDatabaseEntry in databaseEntriesWithProgramsOfSimilarProblems
                .OrderBy(v => random.Next()) // randomize order
                .Take((int)maximalParallelSearchesForIndirectPrograms)
            ) {
                // derive a problem with the parentProgram
                AdvancedAdaptiveLevinSearchProblem derivedProblem =
                    problem
                    .shallowCopy()
                    .setEnumerationMaxProgramLength(TODO)
                    .setParentProgram(iterationDatabaseEntry.program);

                SearchTask createdSearchTask = new SearchTask(
                    scheduler,
                    sparseArrayProgramDistribution,
                    derivedProblem,
                    completitionObservable
                );
                parallel.children.Add(createdSearchTask);
            }
            */
            
            return parallel;
        }
    }
}
