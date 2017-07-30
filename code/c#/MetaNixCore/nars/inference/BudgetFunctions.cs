using MetaNix.nars.entity;
using System;

namespace MetaNix.nars.inference {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/inference/BudgetFunctions.java
    /**
     * Budget functions for resources allocation
     */
    public static class BudgetFunctions {
        /**
         * Determine the rank of a judgment by its quality and originality (stamp baseLength), called from Concept
         *
         * \param judg The judgment to be ranked
         * \return The rank of the judgment, according to truth value only
         */
        public static float rankBelief(ClassicalSentence judg, bool rankTruthExpectation) {
            if (rankTruthExpectation) {
                return judg.truth.expectation;
            }
            float confidence = judg.truth.confidence;
            // float originality = judg.stamp.getOriginality();
            return confidence; //or(confidence, originality);
        }

        /* ----------------------- Belief evaluation ----------------------- */
        /**
         * Determine the quality of a judgment by its truth value alone
         * <p>
         * Mainly decided by confidence, though binary judgment is also preferred
         *
         * \param t The truth value of a judgment
         * \return The quality of the judgment, according to truth value only
         */
        public static float truthToQuality(TruthValue t) {
            float exp = t.expectation;
            return (float)Math.Max(exp, (1 - exp) * 0.75);
        }

        /**
         * Evaluate the quality of a revision, then de-prioritize the premises
         *
         * \param tTruth The truth value of the judgment in the task
         * \param bTruth The truth value of the belief
         * \param truth The truth value of the conclusion of revision
         * \return The budget for the new task
         */
        static public ClassicalBudgetValue revise(TruthValue tTruth, TruthValue bTruth, TruthValue truth, bool feedbackToLinks, nars.control.DerivationContext nal) {
            float difT = truth.getExpDifAbs(tTruth);
            ClassicalTask task = nal.currentTask;
            task.budget.decPriority(1 - difT);
            task.budget.decDurability(1 - difT);

            /* commented because links are not jet implemented
            if (feedbackToLinks) {
                TaskLink tLink = nal.currentTaskLink;
                tLink.decPriority(1 - difT);
                tLink.decDurability(1 - difT);
                TermLink bLink = nal.currentBeliefLink;
                float difB = truth.getExpDifAbs(bTruth);
                bLink.decPriority(1 - difB);
                bLink.decDurability(1 - difB);
            } */
            float dif = truth.confidence - Math.Max(tTruth.confidence, bTruth.confidence);
            float priority = UtilityFunctions.or(dif, task.budget.priority);
            float durability = UtilityFunctions.aveAri(dif, task.budget.durability);
            float quality = truthToQuality(truth);

            /*
            if (priority < 0) {
                memory.nar.output(ERR.class, 
                        new RuntimeException("BudgetValue.revise resulted in negative priority; set to 0"));
                priority = 0;
            }
            if (durability < 0) {
                memory.nar.output(ERR.class, 
                        new RuntimeException("BudgetValue.revise resulted in negative durability; set to 0; aveAri(dif=" + dif + ", task.getDurability=" + task.getDurability() +") = " + durability));
                durability = 0;
            }
            if (quality < 0) {
                memory.nar.output(ERR.class, 
                        new RuntimeException("BudgetValue.revise resulted in negative quality; set to 0"));
                quality = 0;
            }
            */

            return new ClassicalBudgetValue(priority, durability, quality);
        }

        /* ----------------------- Links ----------------------- */
        /**
         * Distribute the budget of a task among the links to it
         *
         * /param b The original budget
         * /param n Number of links
         * /return Budget value for each link
         */
        public static ClassicalBudgetValue distributeAmongLinks(ClassicalBudgetValue b, int n) {
            float priority = (float)(b.priority / Math.Sqrt(n));
            return new ClassicalBudgetValue(priority, b.durability, b.quality);
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/inference/BudgetFunctions.java#L192
        // ATTENTION RELATED
        /**
         * Decrease Priority after an item is used, called in Bag.
         * After a constant time, p should become d*p. Since in this period, the
         * item is accessed c*p times, each time p-q should multiple d^(1/(c*p)).
         * The intuitive meaning of the parameter "forgetRate" is: after this number
         * of times of access, priority 1 will become d, it is a system parameter
         * adjustable in run time.
         *
         * /param budget The previous budget value
         * /param forgetCycles The budget for the new item
         * /param relativeThreshold The relative threshold of the bag
         */
        public static float applyForgetting(ClassicalBudgetValue budget, float forgetCycles, float relativeThreshold) {
            float rescaledQuality = budget.quality * relativeThreshold;
            float p = budget.priority - rescaledQuality; // priority above quality
            if (p > 0) {
                rescaledQuality += p * (float)Math.Pow(budget.durability, 1.0 / (forgetCycles * p));
            }    // priority Durability
            budget.priority = rescaledQuality;
            return rescaledQuality;
        }

        /**
         * Merge an item into another one in a bag, when the two are identical
         * except in budget values
         *
         * \param b The budget baseValue to be modified
         * \param a The budget adjustValue doing the adjusting
         */
        public static void merge(ClassicalBudgetValue b, ClassicalBudgetValue a) {
            b.priority = Math.Max(b.priority, a.priority);
            b.durability = Math.Max(b.durability, a.durability);
            b.quality = Math.Max(b.quality, a.quality);
        }
    }

}
