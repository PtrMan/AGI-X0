using System;

using MetaNix.nars.config;
using MetaNix.nars.entity;
using MetaNix.nars.memory;
using MetaNix.nars.inference;

namespace MetaNix.nars.control {
    // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/control/GeneralInferenceControl.java
    /** Concept reasoning context - a concept is "fired" or activated by applying the reasoner */
    public class GeneralInferenceControl {
        private GeneralInferenceControl() {} // disable ctor

        public static void selectConceptForInference(Memory mem, CompoundAndTermContext compoundAndTermContext) {
            ClassicalConcept currentConcept = mem.workingCyclish.concepts.takeNext();
            if (currentConcept == null) {
                return;
            }

            if (currentConcept.taskLinks.size == 0) { // remove concepts without tasklinks and without termlinks
                mem.workingCyclish.concepts.take(currentConcept.term);
                mem.conceptWasRemoved(currentConcept);
                return;
            }

            if (currentConcept.termLinks.size == 0) {  // remove concepts without tasklinks and without termlinks
                mem.workingCyclish.concepts.take(currentConcept.term);
                mem.conceptWasRemoved(currentConcept);
                return;
            }

            DerivationContext ctx = new DerivationContext(mem, compoundAndTermContext);
            ctx.currentConcept = currentConcept;
            fireConcept(ctx, 1);
        }

        public static void fireConcept(DerivationContext ctx, int numTaskLinks) {
            for( int i = 0; i < numTaskLinks; i++ ) {

                if( ctx.currentConcept.taskLinks.size == 0 ) {
                    return;
                }

                ctx.currentTaskLink = ctx.currentConcept.taskLinks.takeNext();
                if (ctx.currentTaskLink == null) {
                    return;
                }

                if ( ctx.currentTaskLink.budget.isAboveThreshold ) {
                    fireTaskLink(ctx, Parameters.TERMLINK_MAX_REASONED);
                }

                ctx.currentConcept.taskLinks.putBack(ctx.currentTaskLink, ctx.memory.convertDurationToCycles(ctx.memory.param.taskLinkForgetDurations), ctx.memory);
            }

            float forgetCycles = ctx.memory.convertDurationToCycles(ctx.memory.param.conceptForgetDurations);
            ctx.memory.workingCyclish.concepts.putBack(ctx.currentConcept, forgetCycles, ctx.memory);
        }
        
        protected static void fireTaskLink(DerivationContext ctx, uint termLinks) {
            ClassicalTask task = ctx.currentTaskLink.targetTask;
            ctx.currentTerm = ctx.currentConcept.term;
            ctx.currentTaskLink = ctx.currentTaskLink; // TODO PATRICK!!!!!!!!
            ctx.currentBeliefLink = null;
            ctx.currentTask = task; // one of the two places where this variable is set

            /* commented because PATRICK screwed up again, TODO MEDIUM< clarify with patrick after his study induced inactivity time
            if( ctx.currentTaskLink.type == ClassicalTaskLink.EnumType.TRANSFORM ) {
                ctx.currentBelief = null;

                RuleTables.transformTask(ctx.currentTaskLink, ctx); // to turn this into structural inference as below?
            }
            else */ {
                while (termLinks > 0) {
                    ClassicalTermLink termLink = ctx.currentConcept.selectTermLink(ctx.currentTaskLink, ctx.memory.time);
                    if (termLink == null) {
                        break;
                    }
                    fireTermlink(termLink, ctx);
                    ctx.currentConcept.returnTermLink(termLink);
                    termLinks--;
                }
            }

            //ctx.memory.emit(Events.ConceptFire.class, nal);
            //memory.logic.TASKLINK_FIRE.commit(currentTaskLink.budget.getPriority());
        }

        public static bool fireTermlink(ClassicalTermLink termLink, DerivationContext ctx) {
            ctx.currentBeliefLink = termLink;
            // commented because it absorbs all exception, which is bad
            ////try {
                RuleDispatcher.reason(ctx.currentTaskLink, termLink, ctx);
            ////}
            ////catch (Exception ex) {
            ////    if (Parameters.DEBUG) {
                    // uncommented because we need to log this
                    //System.out.println("issue in inference");
            ////    }
            ////}

            //ctx.memory.emit(Events.TermLinkSelect.class, termLink, nal.currentConcept, nal);
            //memory.logic.REASON.commit(termLink.getPriority());                    
            return true;
        }
    }
}
