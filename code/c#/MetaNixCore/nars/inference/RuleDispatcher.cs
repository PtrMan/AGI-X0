using System;

using MetaNix.nars.control;
using MetaNix.nars.entity;
using MetaNix.nars.memory;
using MetaNix.nars.derivation;
using System.Collections.Generic;

namespace MetaNix.nars.inference {
    // a immediate layer between the GeneralInferenceControl and DeriverCaller
    public class RuleDispatcher {
        // for debugging
        public static CompoundAndTermContext compoundAndTermContext;

        // function prototype and some of the methode is like https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/inference/RuleTables.java#L73
        internal static void reason(
            ClassicalTaskLink taskLink,
            ClassicalTermLink beliefLink,
            DerivationContext ctx
        ) {

            { // debugging
                int taskId = taskLink.targetTask.name.term.getAtomicOrTerm;
                int beliefId = beliefLink.target.getAtomicOrTerm;

                if( taskId == 300002 && beliefId == 300004 ) {
                    int breakpointHere2 = 1;
                }
                else if( taskId == 300004 && beliefId == 300002 ) {
                    int breakpointHere2 = 1;
                }

                int breakpointHere1 = 1;
            }

            Memory memory = ctx.memory;

            memory.emotion.manageBusy(ctx);

            ClassicalTask task = ctx.currentTask;
            ClassicalSentence taskSentence = task.sentence;

            TermOrCompoundTermOrVariableReferer taskTerm = taskSentence.term;
            TermOrCompoundTermOrVariableReferer beliefTerm = beliefLink.target;

            // commented because not jet translated
            //if (equalSubTermsInRespectToImageAndProduct(taskTerm, beliefTerm))
            //    return;

            ClassicalConcept beliefConcept = memory.translateTermToConcept(beliefTerm);

            ClassicalSentence belief = (beliefConcept != null) ? beliefConcept.getBelief(ctx, task) : null;

            ctx.currentBelief = belief;

            if (belief != null) {
                beliefTerm = belief.term; //because interval handling that differs on conceptual level
                

                // too restrictive, its checked for non-deductive inference rules in derivedTask (also for single prem)
                if (Stamp.checkBaseOverlap(task.sentence.stamp, belief.stamp)) {
                    ctx.evidentalOverlap = true;
                    if (!task.sentence.stamp.isEternal || !belief.stamp.isEternal) {
                        return; // only allow for eternal reasoning for now to prevent derived event floods
                    }
                    //return; // preparisons are made now to support this nicely
                }
                // comment out for recursive examples, this is for the future, it generates a lot of potentially useless tasks

                //ctx.emit(Events.BeliefReason.class, belief, beliefTerm, taskTerm, nal);
            
                if( LocalRules.match(task, belief, ctx) ) { // new tasks resulted from the match, so return
                    return;
                }
            }

            // current belief and task may have changed, so set again:
            ctx.currentBelief = belief;
            ctx.currentTask = task;

            // HACK< derivation must be made by combining compounds >
            if( !TermUtilities.isTermCompoundTerm(taskTerm) || !TermUtilities.isTermCompoundTerm(beliefTerm) )  {
                return;
            }
            
            // derive and create new tasks for the results
            {
                IList<TemporaryDerivedCompoundWithDecorationAndTruth> derivedCompoundTermsWithDecorationAndTruth = new List<TemporaryDerivedCompoundWithDecorationAndTruth>();
                bool insert = true;

                DeriverCaller.deriverCaller(
                    ctx.compoundAndTermContext,
                    ctx.compoundAndTermContext.translateToCompoundChecked(taskTerm),
                    ctx.compoundAndTermContext.translateToCompoundChecked(beliefTerm),
                    out derivedCompoundTermsWithDecorationAndTruth, insert);

                // translate derivedCompoundTermsWithDecorationAndTruth to tasks and add them
                // for this we have to call DerivationContext.doublePremiseTask() to generate the tasks
                foreach ( var iDerivedWithDecorationAndTruth in derivedCompoundTermsWithDecorationAndTruth ) {
                    TermOrCompoundTermOrVariableReferer content = iDerivedWithDecorationAndTruth.derivedCompoundWithDecoration.termReferer;

                    bool temporalInduction = false; // TODO< decide by rule? from the deriver? >
                    bool overlapAllowed = false; // TODO< decide by rule? from the deriver? >

                    TruthValue truth = null;
                    truth = RuleTable.calcTruthDoublePremise(task.sentence.truth, belief.truth, iDerivedWithDecorationAndTruth.truthfunction);

                    ClassicalBudgetValue budget = new ClassicalBudgetValue(0.5f, 0.5f, 0.5f); // TODO< calculate budget by table >

                    ctx.doublePremiseTask(content,
                        truth,
                        budget,
                        temporalInduction,
                        overlapAllowed
                    );
                }
            }

        }
    }
}
