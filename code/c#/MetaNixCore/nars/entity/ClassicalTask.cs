using MetaNix.nars.memory;
using System;

namespace MetaNix.nars.entity {
    // see https://github.com/opennars/opennars/blob/1.6.5_devel17_TonyAnticipationStrategy/nars_core/nars/entity/Task.java
    public class ClassicalTask : Item<ClassicalSentence>, INamed<ClassicalSentence> {
        static ClassicalTask makeFromSentence(ClassicalSentence sentence) {
            return new ClassicalTask(sentence);
        }

        private ClassicalTask(ClassicalSentence sentence, bool isInput = false) {
		    this.sentence = sentence;
            this.isInput = isInput;
        }

        public class MakeParameters {
            public ClassicalSentence sentence;
            public ClassicalBudgetValue budget;
            public ClassicalTask parentTask;
            public ClassicalSentence parentBelief;
            public ClassicalSentence bestSolution = null; // is optional
        }

        public static ClassicalTask make(MakeParameters parameters) {
            ClassicalTask created = new ClassicalTask(parameters.sentence);
            created.budget = parameters.budget;
            created.parentTask = new WeakReference<ClassicalTask>(parameters.parentTask);
            created.parentBelief = new WeakReference<ClassicalSentence>(parameters.parentBelief);
            created.bestSolution = parameters.bestSolution;
            return created;
        }
        
        public WeakReference<ClassicalSentence> parentBelief; /* Belief from which the Task is derived, or null if derived from a theorem*/

        public ClassicalSentence sentence;

        WeakReference<ClassicalTask> parentTask;

        public ClassicalSentence bestSolution; // for question and goal: best solution found so far

        public readonly bool isInput; // is it an input task

        /**
         * Get the parent belief of a task
         *
         * \return The belief from which the task is derived
         */
        public ClassicalSentence getParentBelief() {
            if (parentBelief == null) return null;
            ClassicalSentence result = null;
            parentBelief.TryGetTarget(out result);
            return result;
        }

        /** flag to indicate whether this Event Task participates in tempporal induction */
        public void setElementOfSequenceBuffer(bool b) {
            this.partOfSequenceBuffer = b;
        }


        public override ClassicalSentence name{ get {
            return sentence;
        }}

        public bool isAboveThreshold { get {
                return budget.isAboveThreshold;
        }}

        // https://github.com/opennars/opennars/blob/master/nars_core/nars/entity/Task.java#L216
        /**
         * Set the best-so-far solution for a Question or Goal, and report answer
         * for input question
         *
         * \param bestSolution The solution to be remembered
         */
        public void setBestSolution(Memory memory, ClassicalSentence bestSolution) {
            // JAVA< InternalExperience.InternalExperienceFromBelief(memory, this, judg); >
            this.bestSolution = bestSolution;
        }

        bool partOfSequenceBuffer = false;
    }
}
