using System;

using MetaNix.nars.entity;
using MetaNix.nars.memory;
using MetaNix.nars.control;
using MetaNix.nars.config;

namespace MetaNix.nars.inference {
    class TemporalRules {
        public enum EnumRateByConfidence {
            YES,
            NO,
        }

        public enum EnumOrder {
            NONE = 2,
            FORWARD = 1,
            CONCURRENT = 0,
            BACKWARD = -1,
            INVALID = -2,
        }

        // https://github.com/opennars/opennars/blob/master/nars_core/nars/inference/TemporalRules.java#L502
        /**
         * Evaluate the quality of the judgment as a solution to a problem
         *
         * \param problem A goal or question
         * \param solution The solution to be evaluated
         * \return The quality of the judgment as the solution
         */
        public static float solutionQuality(EnumRateByConfidence rateByConfidence, ClassicalTask problemTask, ClassicalSentence solution, Memory memory, CompoundAndTermContext compoundAndTermContext) {
            ClassicalSentence problem = problemTask.sentence;

            /* TODO< implement TEMPORAL
            if (!matchingOrder(problem.temporalOrder, solution.temporalOrder)) {
                return 0.0F;
            }
            */

            TruthValue truth = solution.truth;
            if (problem.stamp.occurrenceTime != solution.stamp.occurrenceTime) {
                truth = solution.projectionTruth(problem.stamp.occurrenceTime, memory.time);
            }

            //when the solutions are comparable, we have to use confidence!! else truth expectation.
            //this way negative evidence can update the solution instead of getting ignored due to lower truth expectation.
            //so the previous handling to let whether the problem has query vars decide was wrong.
            if (rateByConfidence == EnumRateByConfidence.NO) {
                return (float)(truth.expectation / Math.Sqrt(Math.Sqrt(Math.Sqrt(compoundAndTermContext.getTermComplexityOfAndByTermReferer(solution.term) * Parameters.COMPLEXITY_UNIT))));
            }
            else {
                return truth.confidence;
            }
        }

        // TODO< rename to evaluateQualityOfBeliefAsASolutionToProblemAndReward and refactor into functions >
        // https://github.com/opennars/opennars/blob/master/nars_core/nars/inference/TemporalRules.java#L537
        /* ----- Functions used both in direct and indirect processing of tasks ----- */
        /**
         * Evaluate the quality of a belief as a solution to a problem, then reward
         * the belief and de-prioritize the problem
         *
         * \param problem The problem (question or goal) to be solved
         * \param solution The belief as solution
         * \param task The task to be immediately processed, or null for continued process
         * \return The budget for the new task which is the belief activated, if
         * necessary
         */
        public static ClassicalBudgetValue solutionEval(
            ClassicalTask problem,
            ClassicalSentence solution,
            ClassicalTask task,
            DerivationContext ctx
        ) {

            ClassicalBudgetValue budget = null;

            bool feedbackToLinks = false;
            if (task == null) {
                task = ctx.currentTask;
                feedbackToLinks = true;
            }
            bool taskSentenceIsjudgment = task.sentence.isJudgment;
            EnumRateByConfidence rateByConfidence = problem.sentence.term.hasVarQuery() ? EnumRateByConfidence.YES : EnumRateByConfidence.NO; // here its whether its a what or where question for budget adjustment
            float quality = TemporalRules.solutionQuality(rateByConfidence, problem, solution, ctx.memory, ctx.compoundAndTermContext);

            if (problem.sentence.isGoal) {
                ctx.memory.emotion.adjustHappy(quality, task.budget.priority, ctx);
            }

            if (taskSentenceIsjudgment) {
                task.budget.incPriority(quality);
            }
            else {
                float taskPriority = task.budget.priority; // +goal satisfication is a matter of degree - https://groups.google.com/forum/#!topic/open-nars/ZfCM416Dx1M
                budget = new ClassicalBudgetValue(UtilityFunctions.or(taskPriority, quality), task.budget.durability, BudgetFunctions.truthToQuality(solution.truth));
                task.budget.priority = Math.Min(1 - quality, taskPriority);
            }
            
            // TODO< implement links >
            /* LINKS commented because links are not implemented
            if (feedbackToLinks) {
                TaskLink tLink = ctx.currentTaskLink;
                tLink.setPriority(Math.Min(1 - quality, tLink.getPriority()));
                TermLink bLink = ctx.currentBeliefLink;
                bLink.incPriority(quality);
            }*/
            return budget;
        }

        // from https://github.com/opennars/opennars/blob/e844d1ee61a2d9af26e6df91b0cc63be7e7dccfc/nars_core/nars/inference/TemporalRules.java#L568
        public static EnumOrder order(long timeDiff, uint durationCycles) {
            uint halfDuration = durationCycles / 2;
            if (timeDiff > halfDuration) {
                return EnumOrder.FORWARD;
            }
            else if (timeDiff < -halfDuration) {
                return EnumOrder.BACKWARD;
            }
            else {
                return EnumOrder.CONCURRENT;
            }
        }

        // from https://github.com/opennars/opennars/blob/e844d1ee61a2d9af26e6df91b0cc63be7e7dccfc/nars_core/nars/inference/TemporalRules.java#L582
        /** if (relative) event B after (stationary) event A then order=forward;
         *                event B before       then order=backward
         *                occur at the same time, relative to duration: order = concurrent
         */
        public static EnumOrder order(long a, long b, uint durationCycles) {
            if ((a == Stamp.ETERNAL) || (b == Stamp.ETERNAL)) {
                throw new Exception("order() does not compare ETERNAL times");
            }
            return order(b - a, durationCycles);
        }
    }
    
}
