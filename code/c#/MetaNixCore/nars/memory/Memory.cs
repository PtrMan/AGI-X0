using System.Collections.Generic;

using MetaNix.nars.control;
using MetaNix.nars.entity;
using MetaNix.nars.entity.builder;
using MetaNix.nars.control.attention;
using MetaNix.nars.derivation;
using MetaNix.nars.plugin.mental;
using MetaNix.nars.config;

namespace MetaNix.nars.memory {
    // TODO 31.05.2017 : transfer from newTasks to attention buffer somehow and work on all tasks in taskBuffer like it's done in 1.6.5

    // see https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/storage/Memory.java
    public class Memory {
        public RuntimeParameters param; // System parameters that can be changed at runtime
        
        public Emotions emotion = new Emotions(); // emotion meter keeping track of global emotion

        /** List of new tasks accumulated in one cycle, to be processed in the next cycle */
        public Queue<ClassicalTask> newTasks = new Queue<ClassicalTask>();


        // mutation of https://github.com/opennars/opennars/blob/4c428cd39c03a676da5247400bc962ad0f84a948/nars_core/nars/storage/Memory.java#L116
        /* like the taskBuffer in pei's docs, New tasks with novel composed terms, for delayed and selective processing
         *
         * the difference here is that we don't hardcode the attention mechanism for novel tasks
         * 
         */
        IAttentionMechanism<ClassicalTask> attention;

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/storage/Memory.java#L365
        public void conceptWasRemoved(ClassicalConcept concept) {
            // emit(Events.ConceptForget.class, c);
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/storage/Memory.java#L169
        /**
         * Get an existing Concept for a given name
         *
         * /param t the name of a concept
         * /return a Concept or null
         */
        public ClassicalConcept translateTermToConcept(TermOrCompoundTermOrVariableReferer term) {
            return workingCyclish.concepts.referenceByKey(term);
        }

        BagBuilder bagBuilder = new BagBuilder();

        public WorkingCyclish workingCyclish;

        public Memory(
            CompoundAndTermContext compoundAndTermContext,
            RuntimeParameters runtimeParameters,
            IAttentionMechanism<ClassicalTask> attention
        ) {

            this.compoundAndTermContext = compoundAndTermContext;
            this.param = runtimeParameters;
            this.attention = attention;

            conceptProcessing = new ClassicalConceptProcessing(this, compoundAndTermContext);

            workingCyclish = new WorkingCyclish();
            workingCyclish.concepts = new ArrayBag<ClassicalConcept, TermOrCompoundTermOrVariableReferer>();
            workingCyclish.concepts.setMaxSize(Parameters.CONCEPT_BAG_SIZE);
        }

        private long currentStampSerial = 0;
        public long newStampSerial() {
            return currentStampSerial++;
        }

        /**
         * add new task that waits to be processed in the next cycleMemory
         */
        public void addNewTask(ClassicalTask t, string reason) {
            newTasks.Enqueue(t);
            //  logic.TASK_ADD_NEW.commit(t.getPriority());
            //emit(Events.TaskAdd.class, t, reason);
            //output(t);
        }

        public void perceptNewTaskWithBudgetCheck(ClassicalTask t) {
            if( !t.isAboveThreshold ) {
                return;
            }

            addNewTask(t, "perceived");
        }

        public void removeTask(ClassicalTask task, string reason) {
            //emit(TaskRemove.class, task, reason);
            task.wasDiscarded();        
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/storage/Memory.java#L187
        /**
         * Get the Concept associated to a Term, or create it.
         * 
         * Doesn't take it from the bag and put it back to save some cycles and to allow better concurrency
         * Doesn't adjust budget
         * 
         * if it fails with an soft error (can't take it from the bag etc) it returns null
         *
         * doesn't displace concepts
         * 
         * @param term indicating the concept
         * @return an existing Concept, or a new one, or null 
         */
        public ClassicalConcept conceptualize(ClassicalBudgetValue budget, TermOrCompoundTermOrVariableReferer term) {

            ClassicalConcept concept = workingCyclish.concepts.referenceByKey(term);
            if (concept == null) {
                // CONCURRENCY< lock and put back >

                // create new concept, with the applied budget
                concept = new ClassicalConcept(compoundAndTermContext, term, bagBuilder, budget, this);

                workingCyclish.concepts.putIn(concept);

                // from java code base
                // TODO ASK< implement 
                //emit(Events.ConceptNew.class, concept);                
            }
            else {
                // legacy java code applies budget here, we don't
            }

            return concept;
        }

        public long time {
            get {
                return privateCycle;
            }
        }

        public void cycle() {
            void insertNewTasks() {
                // we do nothing here because we call addTask from outside already
            }

            insertNewTasks();

            processNewTasks();

            ClassicalTask currentTask = selectAttentional(); // get the task which has the highest attention
            if( currentTask != null ) {
                localInference(currentTask);
            }

            GeneralInferenceControl.selectConceptForInference(this, compoundAndTermContext);
        }
        
        /**
         * Uses the attention menchamism to select the next task to get processed
         * 
         * 
         */
        private ClassicalTask selectAttentional() {
            ClassicalTask taskWithAttention = attention.getNextAttentionElement();
            return taskWithAttention;
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/storage/Memory.java#L392
        void localInference(ClassicalTask task) {
            DerivationContext context = new DerivationContext(this, compoundAndTermContext);
            context.currentTask = task;
            context.currentTerm = task.sentence.term;
            context.currentConcept = conceptualize(task.budget, context.currentTerm);
            if (context.currentConcept != null) {
                conceptProcessing.directProcess(context, context.currentConcept, task);
            }
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/storage/Memory.java#L417
        void processNewTasks() {
            while(newTasks.Count != 0) {
                ClassicalTask task = newTasks.Dequeue();

                if ( // new input or existing concept
                    task.isInput ||
                    task.sentence.isQuest ||
                    task.sentence.isQuestion
                    /*|| concept(task.sentence.term) != null*/
                ) {

                    localInference(task);
                }
                else {
                    ClassicalSentence sentence = task.sentence;
                    if (sentence.isJudgment || sentence.isGoal) {
                        double expectation = sentence.truth.expectation;

                        // new concept formation
                        if(
                            (sentence.isJudgment && expectation > Parameters.DEFAULT_CREATION_EXPECTATION) ||
                            (sentence.isGoal && expectation > Parameters.DEFAULT_CREATION_EXPECTATION_GOAL)
                        ) {
                            attention.addNovelTask(task);
                        }
                        else {
                            removeTask(task, "Neglected");
                        }
                    }
                }
            }
        }

        // see https://github.com/opennars/opennars/blob/62c814fb0f3e474a176515103394049b2887ec29/nars_core/nars/storage/Memory.java#L699
        /** converts durations to cycles */
        public float convertDurationToCycles(double durations) {
            return (float)param.duration.value * (float)durations;
        }

        ClassicalConceptProcessing conceptProcessing;

        private long privateCycle; /* System clock, relatively defined to guarantee the repeatability of behaviors */
        CompoundAndTermContext compoundAndTermContext;
    }
}