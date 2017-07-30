using System.Collections.Generic;

using MetaNix.nars.config;
using MetaNix.nars.entity;
using MetaNix.nars.memory;
using MetaNix.nars.inference;

namespace MetaNix.nars.control {
    public class DerivationContext {
        public interface DerivationFilter /*extends Plugin */{
            /** returns null if allowed to derive, or a String containing a short rejection reason for logging */
            string reject(DerivationContext nal, ClassicalTask task, bool revised, bool single, ClassicalTask parent, ClassicalSentence otherBelief);

            /* uncommented because plugin system is not imported
            @Override
            public default boolean setEnabled(NAR n, boolean enabled) {
                return true;
            }
            */
        }

    private DerivationContext() {} // disable standard ctor
        public DerivationContext(Memory memory, CompoundAndTermContext compoundAndTermContext) {
            this.memory = memory;
            this.compoundAndTermContext = compoundAndTermContext;
        }

        protected Stamp newStamp;
        public StampBuilder newStampBuilder;

        public ClassicalTask currentTask;
        public TermOrCompoundTermOrVariableReferer currentTerm;
        public ClassicalConcept currentConcept;
        public ClassicalSentence currentBelief;

        public ClassicalTermLink currentBeliefLink;
        public ClassicalTaskLink currentTaskLink;

        public bool evidentalOverlap = false;

        public CompoundAndTermContext compoundAndTermContext;


        /** tasks added with this method will be remembered by this NAL instance; useful for feedback */
        public void addTask(ClassicalTask t, string reason) {
            if (t.sentence.term == null) {
                return;
            }
            memory.addNewTask(t, reason);
        }

        /**
         * Activated task called in MatchingRules.trySolution and
         * Concept.processGoal
         *
         * @param budget The budget value of the new Task
         * @param sentence The content of the new Task
         * @param candidateBelief The belief to be used in future inference, for
         * forward/backward correspondence
         */
        public void addTask(ClassicalTask currentTask, ClassicalBudgetValue budget, ClassicalSentence sentence, ClassicalSentence candidateBelief) {
            ClassicalTask.MakeParameters taskParameters = new ClassicalTask.MakeParameters();
            taskParameters.sentence = sentence;
            taskParameters.budget = budget;
            taskParameters.parentTask = currentTask;
            taskParameters.parentBelief = sentence;
            taskParameters.bestSolution = candidateBelief;

            addTask(ClassicalTask.make(taskParameters), "Activated");
        }

        /**
         * Derived task comes from the inference rules.
         *
         * \param task the derived task
         * \param overlapAllowed //https://groups.google.com/forum/#!topic/open-nars/FVbbKq5En-M
         */
        public bool derivedTask(
            ClassicalTask task,
            bool revised,
            bool single,
            ClassicalTask parent,
            ClassicalSentence occurence2,
            bool overlapAllowed
        ) {
            if (derivationFilters != null) {
                foreach(DerivationFilter iDerivationFilter in derivationFilters) {
                    string rejectionReason = iDerivationFilter.reject(this, task, revised, single, parent, occurence2);
                    if (rejectionReason != null) {
                        memory.removeTask(task, rejectionReason);
                        return false;
                    }
                }
            }

            ClassicalSentence occurence = parent != null ? parent.sentence : null;


            if (!task.budget.isAboveThreshold) {
                memory.removeTask(task, "Insufficient Budget");
                return false;
            }

            if (task.sentence != null && task.sentence.truth != null) {
                float conf = task.sentence.truth.confidence;
                if (conf == 0) {
                    //no confidence - we can delete the wrongs out that way.
                    memory.removeTask(task, "Ignored (zero confidence)");
                    return false;
                }
            }


            /* uncommented because operations are not jet implemented
             * TODO< translate to C# if operations are implemented
             */
            /*
            if (task.sentence.term instanceof Operation) {
                Operation op = (Operation)task.sentence.term;
                if (op.getSubject() instanceof Variable || op.getPredicate() instanceof Variable) {
                    memory.removeTask(task, "Operation with variable as subject or predicate");
                    return false;
                }
            }
            */



            Stamp stamp = task.sentence.stamp;
            if (occurence != null && !occurence.stamp.isEternal) {
                stamp.occurrenceTime = occurence.stamp.occurrenceTime;
            }
            if (occurence2 != null && !occurence2.stamp.isEternal) {
                stamp.occurrenceTime = occurence2.stamp.occurrenceTime;
            }

            // it is revision, of course its cyclic, apply evidental base policy
            // TODO< reconsider >
            if (
                !overlapAllowed &&
                this.evidentalOverlap &&
                Stamp.checkOverlap(stamp)
            ) {
                memory.removeTask(task, "Overlapping Evidenctal Base");
                //"(i=" + i + ",j=" + j +')'
                return false;
            }
            
            task.setElementOfSequenceBuffer(false);
            if( !revised ) {
                task.budget.durability = (task.budget.durability * Parameters.DERIVATION_DURABILITY_LEAK);
                task.budget.priority = (task.budget.priority * Parameters.DERIVATION_PRIORITY_LEAK);
            }
            //memory.event.emit(Events.TaskDerive.class, task, revised, single, occurence, occurence2);
            //memory.logic.TASK_DERIVED.commit(task.budget.getPriority());

            addTask(task, "Derived");
            return true;
        }

        /* --------------- new task building --------------- */
        /**
         * Shared final operations by all double-premise rules, called from the
         * rules except StructuralRules
         *
         * \param newContent The content of the sentence in task
         * \param newTruth The truth value of the sentence in task
         * \param newBudget The budget value in task
         */
        public bool doublePremiseTaskRevised(TermOrCompoundTermOrVariableReferer newContent, TruthValue newTruth, ClassicalBudgetValue newBudget) {
            ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters makeSentenceParameters = new ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters();
            makeSentenceParameters.term = newContent;
            makeSentenceParameters.punctation = currentTask.sentence.punctation;
            makeSentenceParameters.truth = newTruth;
            makeSentenceParameters.stamp = returnTheNewStamp();
            ClassicalSentence newSentence = ClassicalSentence.makeByTermPunctuationTruthStampNormalize(makeSentenceParameters);
            
            ClassicalTask.MakeParameters makeTaskParameters = new ClassicalTask.MakeParameters();
            makeTaskParameters.sentence = newSentence;
            makeTaskParameters.budget = newBudget;
            makeTaskParameters.parentTask = currentTask;
            makeTaskParameters.parentBelief = currentBelief;
            ClassicalTask newTask = ClassicalTask.make(makeTaskParameters);
            return derivedTask(newTask, true, false, null, null, true); // allows overlap since overlap was already checked on revisable( function
                                                                        // which is not the case for other single premise tasks
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/control/DerivationContext.java#L217
        /**
         * Shared final operations by all double-premise rules, called from the
         * rules except StructuralRules
         *
         * /param newContent The content of the sentence in task
         * /param newTruth The truth value of the sentence in task
         * /param newBudget The budget value in task
         * /param temporalInduction
         * /param overlapAllowed // https://groups.google.com/forum/#!topic/open-nars/FVbbKq5En-M
         */
        public IList<ClassicalTask> doublePremiseTask(
            TermOrCompoundTermOrVariableReferer newContent,
            TruthValue newTruth,
            ClassicalBudgetValue newBudget,
            bool temporalInduction,
            bool overlapAllowed
        ) {

            IList<ClassicalTask> ret = new List<ClassicalTask>();
            if (newContent == null) {
                return null;
            }

            if (!newBudget.isAboveThreshold) {
                return null;
            }

            if(
                newContent == null  /* commented because not implemented   ||
                ((newContent instanceof Interval)) ||
                ((newContent instanceof Variable))*/
            ) {

                return null;
            }

            /* commented because not implemented
            if (newContent.subjectOrPredicateIsIndependentVar()) {
                return null;
            }*/

            ClassicalSentence newSentence;
            ClassicalTask newTask;

            ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters makeSentenceParameters = new ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters();
            makeSentenceParameters.term = newContent;
            makeSentenceParameters.punctation = currentTask.sentence.punctation;
            makeSentenceParameters.truth = newTruth;
            makeSentenceParameters.stamp = returnTheNewStamp();

            newSentence = ClassicalSentence.makeByTermPunctuationTruthStampNormalize(makeSentenceParameters);
            newSentence.producedByTemporalInduction = temporalInduction;

            ClassicalTask.MakeParameters taskMakeParameters = new ClassicalTask.MakeParameters();
            taskMakeParameters.sentence = newSentence;
            taskMakeParameters.budget = newBudget;
            taskMakeParameters.parentTask = currentTask;
            taskMakeParameters.parentBelief = currentBelief;
            
            newTask = ClassicalTask.make(taskMakeParameters);

            if (newTask != null) {
                bool added = derivedTask(newTask, false, false, null, null, overlapAllowed);
                if (added) {
                    ret.Add(newTask);
                }
            }
            
            // "Since in principle it is always valid to eternalize a tensed belief"
            if( temporalInduction && Parameters.IMMEDIATE_ETERNALIZATION ) { // temporal induction generated ones get eternalized directly
                TruthValue truthEternalized = TruthFunctions.eternalize(newTruth);
                Stamp st = returnTheNewStamp().clone();
                st.isEternal = true;

                makeSentenceParameters = new ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters();
                makeSentenceParameters.term = newContent;
                makeSentenceParameters.punctation = currentTask.sentence.punctation;
                makeSentenceParameters.truth = truthEternalized;
                makeSentenceParameters.stamp = st;

                newSentence = ClassicalSentence.makeByTermPunctuationTruthStampNormalize(makeSentenceParameters);
                newSentence.producedByTemporalInduction = temporalInduction;

                taskMakeParameters = new ClassicalTask.MakeParameters();
                taskMakeParameters.sentence = newSentence;
                taskMakeParameters.budget = newBudget;
                taskMakeParameters.parentTask = currentTask;
                taskMakeParameters.parentBelief = currentBelief;

                newTask = ClassicalTask.make(taskMakeParameters);
                if (newTask != null) {
                    bool added = derivedTask(newTask, false, false, null, null, overlapAllowed);
                    if (added) {
                        ret.Add(newTask);
                    }
                }
            }

            return ret;
        }

        public bool singlePremiseTask(ClassicalSentence newSentence, ClassicalBudgetValue newBudget) {
            if (!newBudget.isAboveThreshold) {
                return false;
            }

            ClassicalTask.MakeParameters newTaskParameters = new ClassicalTask.MakeParameters();
            newTaskParameters.sentence = newSentence;
            newTaskParameters.budget = newBudget;
            newTaskParameters.parentTask = currentTask;

            ClassicalTask newTask = ClassicalTask.make(newTaskParameters);
            return derivedTask(newTask, false, true, null, null, false);
        }

        /**
         * \return the newStamp
         */
        public Stamp returnTheNewStamp()
        {
            if (newStamp == null) {
                //if newStamp==null then newStampBuilder must be available. cache it's return value as newStamp
                newStamp = newStampBuilder.build();
                newStampBuilder = null;
            }
            return newStamp;
        }

        /** creates a lazy/deferred StampBuilder which only constructs the stamp if getTheNewStamp() is actually invoked */
        public void setTheNewStamp(Stamp first, Stamp second, long time) {
            newStamp = null;
            newStampBuilder = new DefaultStampBuilder(first, second, time);
        }

        public interface StampBuilder {
            Stamp build();
        }

        // helper class
        class DefaultStampBuilder : StampBuilder {
            private long time;
            private Stamp second, first;

            public DefaultStampBuilder(Stamp first, Stamp second, long time) {
                this.first = first;
                this.second = second;
                this.time = time;
            }

            public Stamp build() {
                return Stamp.zipWithTime(first, second, time);
            }
        }

        public Memory memory;

        protected IList<DerivationFilter> derivationFilters = null;
    }
}
