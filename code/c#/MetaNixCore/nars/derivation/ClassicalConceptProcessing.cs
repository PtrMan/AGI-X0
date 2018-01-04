using System.Collections.Generic;

using MetaNix.misc;
using MetaNix.nars.control;
using MetaNix.nars.entity;
using MetaNix.nars.inference;
using MetaNix.nars.memory;
using MetaNix.nars.config;

namespace MetaNix.nars.derivation {
    // all Concept related processing are in this class because the processing is not a task of the ClassicalConcept
    public class ClassicalConceptProcessing {
        private ClassicalConceptProcessing() {} // disable standard ctor
        public ClassicalConceptProcessing(Memory memory, CompoundAndTermContext compoundAndTermContext) {
            this.memory = memory;
            this.compoundAndTermContext = compoundAndTermContext;
        }

        // https://github.com/opennars/opennars/blob/master/nars_core/nars/entity/Concept.java#L193
        public void directProcess(DerivationContext ctx, ClassicalConcept @this, ClassicalTask task) {
            var puctuation = task.sentence.punctation;
            /*final*/ switch(puctuation) {
                case ClassicalSentence.EnumPunctation.JUDGMENT:
                processJudgment(ctx, @this, task);
                break;

                case ClassicalSentence.EnumPunctation.GOAL:
                processGoal(ctx, @this, task);
                break;

                case ClassicalSentence.EnumPunctation.QUESTION:
                case ClassicalSentence.EnumPunctation.QUEST:
                processQuestion(ctx, @this, task);
                break;

            }

            maintainDisappointedAnticipations();

            if( task.isAboveThreshold ) {    // still need to be processed
                                             //memory.logic.LINK_TO_TASK.commit();
                linkToTask(ctx, @this, task);
            }

        }

        private static void maintainDisappointedAnticipations() {
            // TODO< copy stuff from lecacy nars >
        }

        /**
         * Link to a new task from all relevant concepts for continued processing in
         * the near future for unspecified time.
         * <p>
         * The only method that calls the TaskLink constructor.
         *
         * /param task The task to be linked
         * /param content The content of the task
         */
        private static void linkToTask(DerivationContext ctx, ClassicalConcept @this, ClassicalTask task) {
            ClassicalBudgetValue taskBudget = task.budget;

            @this.insertTaskLink(
                new ClassicalTaskLink(
                    task,
                    null,
                    taskBudget,
                    Parameters.TERM_LINK_RECORD_LENGTH),
                ctx);  // link type: SELF

            if (!TermUtilities.isTermCompoundTerm(@this.term) ) {
                return;
            }

            if( @this.termLinkTemplates.isEmpty() ) {
                return;
            }

            ClassicalBudgetValue subBudget = BudgetFunctions.distributeAmongLinks(taskBudget, @this.termLinkTemplates.Count);
            if( !subBudget.isAboveThreshold ) {
                return;
            }
            // else here

            for (int t = 0; t < @this.termLinkTemplates.Count; t++) {
                ClassicalTermLink termLink = @this.termLinkTemplates[t];

                if( termLink.type == ClassicalTermLink.EnumType.TEMPORAL ) {
                    continue;
                }

                TermOrCompoundTermOrVariableReferer componentTerm = termLink.target;

                ClassicalConcept componentConcept = @this.memory.conceptualize(subBudget, componentTerm);

                if (componentConcept != null) {
                    componentConcept.insertTaskLink(
                        new ClassicalTaskLink(
                            task,
                            termLink,
                            subBudget,
                            Parameters.TERM_LINK_RECORD_LENGTH),
                        ctx);
                }
            }

            @this.buildTermLinks(taskBudget);  // recursively insert TermLink
        }

        // https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/Concept.java#L648
        /**
         * To answer a question by existing beliefs
         *
         * \param task The task to be processed
         * \return Whether to continue the processing of the task
         */
        private void processQuestion(DerivationContext ctx, ClassicalConcept @this, ClassicalTask task) {
            ClassicalTask quesTask = task;
            bool newQuestion = true;
            foreach(ClassicalTask t in @this.questions) {
                if (t.sentence.equalsContent(quesTask.sentence)) {
                    quesTask = t;
                    newQuestion = false;
                    break;
                }
            }

            if (newQuestion) {
                if (@this.questions.Count + 1 > Parameters.CONCEPT_QUESTIONS_MAX) {
                    ClassicalTask removed = @this.questions[0];
                    @this.questions.RemoveAt(0);    // FIFO
                    //memory.event.emit(ConceptQuestionRemove.class, this, removed);
                }

                @this.questions.Add(task);
                // memory.event.emit(ConceptQuestionAdd.class, this, task);
            }

            ClassicalSentence ques = quesTask.sentence;
            ClassicalTask newAnswerT = (ques.isQuestion)
                ? selectCandidate(quesTask, @this.beliefs, EnumForRevision.NOREVISION)
                : selectCandidate(quesTask, @this.desires, EnumForRevision.NOREVISION);

            if (newAnswerT != null) {
                LocalRules.trySolution(newAnswerT.sentence, task, ctx, true, memory);
            } 
            // commented because memory events are not implemented
            //else if(task.isInput() && !quesTask.getTerm().hasVarQuery() && quesTask.getBestSolution() != null) { //show previously found solution anyway in case of input
            //memory.emit(Events.Answer.class, quesTask, quesTask.getBestSolution()); 
            //}
        }



        // https://github.com/opennars/opennars/blob/62c814fb0f3e474a176515103394049b2887ec29/nars_core/nars/entity/Concept.java#L440
        /**
         * To accept a new goal, and check for revisions and realization, then
         * decide whether to actively pursue it
         *
         * \param judg The judgment to be accepted
         * \param task The task to be processed
         * \return Whether to continue the processing of the task
         */
        private bool processGoal(DerivationContext ctx, ClassicalConcept @this, ClassicalTask task) {
            ClassicalSentence goal = task.sentence;
            ClassicalTask oldGoalT = selectCandidate(task, @this.desires, EnumForRevision.REVISION); // revise with the existing desire values
            ClassicalSentence oldGoal = null;

            if (oldGoalT != null) {
                oldGoal = oldGoalT.sentence;
                Stamp newStamp = goal.stamp;
                Stamp oldStamp = oldGoal.stamp;

                bool isStampEqual = Stamp.checkEquals(
                    newStamp,
                    oldStamp,
                    Stamp.EnumCompareCreationTime.NO,
                    Stamp.EnumCompareOccurrenceTime.NO,
                    Stamp.EnumCompareEvidentialBaseTime.YES);

                if (isStampEqual) {
                    return false; // duplicate
                }
                if (LocalRules.revisible(goal, oldGoal)) {

                    ctx.setTheNewStamp(newStamp, oldStamp, memory.time);

                    ClassicalSentence projectedGoal = oldGoal.projection(task.sentence.stamp.occurrenceTime, newStamp.occurrenceTime);
                    if (projectedGoal != null) {
                        // if (goal.after(oldGoal, nal.memory.param.duration.get())) { //no need to project the old goal, it will be projected if selected anyway now
                        // nal.singlePremiseTask(projectedGoal, task.budget); 
                        //return;
                        // }
                        ctx.currentBelief = projectedGoal;
                        if (true/* TODO< check if not operation > !(task.sentence.term instanceof Operation)*/) {
                            bool successOfRevision = LocalRules.revision(ctx, task.sentence, projectedGoal, false);
                            if (successOfRevision) { // it is revised, so there is a new task for which this function will be called
                                return false; // with higher/lower desire
                            } // it is not allowed to go on directly due to decision making https://groups.google.com/forum/#!topic/open-nars/lQD0no2ovx4
                        }
                    }
                }
            }

            Stamp s2 = goal.stamp.clone();
            s2.occurrenceTime = memory.time;
            if (s2.after(task.sentence.stamp, ctx.memory.param.duration.value)) { // this task is not up to date we have to project it first
                ClassicalSentence projGoal = task.sentence.projection(memory.time, ctx.memory.param.duration.value);
                if (projGoal != null && projGoal.truth.expectation > ctx.memory.param.decisionThreshold) {
                    ctx.singlePremiseTask(projGoal, task.budget.clone()); // keep goal updated
                    // return false; //outcommented, allowing "roundtrips now", relevant for executing multiple steps of learned implication chains
                }
            }

            if (task.isAboveThreshold) {

                ClassicalTask beliefT = selectCandidate(task, @this.beliefs, EnumForRevision.NOREVISION); // check if the Goal is already satisfied

                double antiSatisfaction = 0.5; // we dont know anything about that goal yet, so we pursue it to remember it because its maximally unsatisfied
                if (beliefT != null) {
                    ClassicalSentence belief = beliefT.sentence;
                    ClassicalSentence projectedBelief = belief.projection(task.sentence.stamp.occurrenceTime, ctx.memory.param.duration.value);
                    LocalRules.trySolution(projectedBelief, task, ctx, true, memory); // check if the Goal is already satisfied (manipulate budget)
                    antiSatisfaction = task.sentence.truth.getExpDifAbs(belief.truth);
                }

                double satisfaction = 1.0 - antiSatisfaction;
                TruthValue t = goal.truth.clone();

                t.frequency = (float)(t.frequency - satisfaction); // decrease frequency according to satisfaction value

                bool fullfilled = antiSatisfaction < Parameters.SATISFACTION_TRESHOLD;

                ClassicalSentence projectedGoal = goal.projection(ctx.memory.time, ctx.memory.time); // NOTE< this projection is fine >

                /* commented because we stil have to implement operations
                if (projectedGoal != null && task.isAboveThreshold && !fullfilled && projectedGoal.truth.expectation > nal.memory.param.decisionThreshold.get()) {

                    try {
                        Operation bestop = null;
                        float bestop_truthexp = 0.0f;
                        TruthValue bestop_truth = null;
                        Task executable_precond = null;
                        //long distance = -1;
                        long mintime = -1;
                        long maxtime = -1;
                        for (Task t: this.executable_preconditions) {
                            Term[] prec = ((Conjunction)((Implication)t.getTerm()).getSubject()).term;
                            Term[] newprec = new Term[prec.length - 3];
                            for (int i = 0; i < prec.length - 3; i++) { //skip the last part: interval, operator, interval
                                newprec[i] = prec[i];
                            }

                            //distance = Interval.magnitudeToTime(((Interval)prec[prec.length-1]).magnitude, nal.memory.param.duration);
                            mintime = nal.memory.time() + Interval.magnitudeToTime(((Interval)prec[prec.length - 1]).magnitude - 1, nal.memory.param.duration);
                            maxtime = nal.memory.time() + Interval.magnitudeToTime(((Interval)prec[prec.length - 1]).magnitude + 2, nal.memory.param.duration);

                            Operation op = (Operation)prec[prec.length - 2];
                            Term precondition = Conjunction.make(newprec, TemporalRules.ORDER_FORWARD);

                            Concept preconc = nal.memory.concept(precondition);
                            long newesttime = -1;
                            Task bestsofar = null;
                            if (preconc != null) { //ok we can look now how much it is fullfilled

                                //check recent events in event bag
                                for (Task p : this.memory.sequenceTasks) {
                                    if (p.sentence.term.equals(preconc.term) && p.sentence.isJudgment() && !p.sentence.isEternal() && p.sentence.getOccurenceTime() > newesttime && p.sentence.getOccurenceTime() <= memory.time()) {
                                        newesttime = p.sentence.getOccurenceTime();
                                        bestsofar = p; //we use the newest for now
                                    }
                                }
                                if (bestsofar == null) {
                                    continue;
                                }
                                //ok now we can take the desire value:
                                TruthValue A = projectedGoal.getTruth();
                                //and the truth of the hypothesis:
                                TruthValue Hyp = t.sentence.truth;
                                //and the truth of the precondition:
                                Sentence projectedPrecon = bestsofar.sentence.projection(memory.time() comment*- distance comment*, memory.time());

                                if (projectedPrecon.isEternal()) {
                                    continue; //projection wasn't better than eternalization, too long in the past
                                }
                                //debug start
                                //long timeA = memory.time();
                                //long timeOLD = bestsofar.sentence.stamp.getOccurrenceTime();
                                //long timeNEW = projectedPrecon.stamp.getOccurrenceTime();
                                //debug end
                                TruthValue precon = projectedPrecon.truth;
                                //and derive the conjunction of the left side:
                                TruthValue leftside = TruthFunctions.desireDed(A, Hyp);
                                //in order to derive the operator desire value:
                                TruthValue opdesire = TruthFunctions.desireDed(precon, leftside);

                                float expecdesire = opdesire.getExpectation();
                                if (expecdesire > bestop_truthexp) {
                                    bestop = op;
                                    bestop_truthexp = expecdesire;
                                    bestop_truth = opdesire;
                                    executable_precond = t;
                                }
                            }
                        }

                        if (bestop != null && bestop_truthexp > memory.param.decisionThreshold.get()) {
                            Task t = new Task(new Sentence(bestop, Symbols.JUDGMENT_MARK, bestop_truth, projectedGoal.stamp), new BudgetValue(1.0f, 1.0f, 1.0f));
                            //System.out.println("used " +t.getTerm().toString() + String.valueOf(memory.randomNumber.nextInt()));
                            if (!task.sentence.stamp.evidenceIsCyclic()) {
                                if (!executeDecision(t)) { //this task is just used as dummy
                                    memory.emit(UnexecutableGoal.class, task, this, nal);
                                } else {
                                    memory.decisionBlock = memory.time() + Parameters.AUTOMATIC_DECISION_USUAL_DECISION_BLOCK_CYCLES;
                                    SyllogisticRules.generatePotentialNegConfirmation(nal, executable_precond.sentence, executable_precond.budget, mintime, maxtime, 2);
                                }
                            }
                        }
                    }
                    catch(Exception ex) {
                        System.out.println("Failure in operation choice rule, analyze!");
                    }
                


                    questionFromGoal(task, ctx);


                    addToTable(task, false, desires, Parameters.CONCEPT_GOALS_MAX, ConceptGoalAdd.class, ConceptGoalRemove.class);
                
                    InternalExperience.InternalExperienceFromTask(memory, task,false);
                
                    if(nal.memory.time >= memory.decisionBlock && !executeDecision(task)) {
                        // commented because we haven't implemented the observer until now    memory.emit(UnexecutableGoal.class, task, this, nal);
                        return true; // it was made true by itself
                    }
                    return false;
                } // big if
                */

                return fullfilled;
            }
            return false;
        }

        // https://github.com/opennars/opennars/blob/master/nars_core/nars/entity/Concept.java#L233
        /**
         * To accept a new judgment as belief, and check for revisions and solutions
         *
         * \param judg The judgment to be accepted
         * \param task The task to be processed
         * \return Whether to continue the processing of the task
         */
        private void processJudgment(DerivationContext ctx, ClassicalConcept @this, ClassicalTask task) {
            ClassicalSentence judgement = task.sentence;

            ClassicalTask oldBeliefTask = selectCandidate(task, @this.beliefs, EnumForRevision.REVISION);// only revise with the strongest
                                                                               // QUESTION< how about projection? >
            ClassicalSentence oldBelief = null;
            if (oldBeliefTask != null) {
                oldBelief = oldBeliefTask.sentence;
                Stamp newStamp = judgement.stamp;
                Stamp oldStamp = oldBelief.stamp;       // when table is full, the latter check is especially important, too
                if(
                    Stamp.checkEquals(
                        newStamp,
                        oldStamp,
                        Stamp.EnumCompareCreationTime.NO,
                        Stamp.EnumCompareOccurrenceTime.YES,
                        Stamp.EnumCompareEvidentialBaseTime.YES) &&
                    task.sentence.truth == oldBelief.truth
                ) {

                    memory.removeTask(task, "Duplicated");
                    return;
                }
                else if (LocalRules.revisible(judgement, oldBelief)) {
                    // commented because we have to implement revision like in the legacy system
                    // projection can occur either with the NARS default or the new exponential decay model
                    // QUESTION< which one is better ? >

                    ctx.setTheNewStamp(newStamp, oldStamp, memory.time);
                    ClassicalSentence projectedBelief = oldBelief.projection(memory.time, newStamp.occurrenceTime);
                    if (projectedBelief != null) {
                        if (projectedBelief.stamp.occurrenceTime != oldBelief.stamp.occurrenceTime) {
                            // nal.singlePremiseTask(projectedBelief, task.budget);
                        }
                        ctx.currentBelief = projectedBelief;
                        LocalRules.revision(ctx, judgement, projectedBelief, false);
                    }
                }
            }

            if (task.isAboveThreshold) {
                foreach( var iQuestion in @this.questions) {
                    LocalRules.trySolution(judgement, iQuestion, ctx, true, memory);
                }

                TableMaintenance.addToTable(task, false, @this.beliefs, Parameters.CONCEPT_BELIEFS_MAX, null/*ConceptBeliefAdd.class*/, null/*ConceptBeliefRemove.class*/);
                
        
                // if taskLink predicts this concept then add to predictive 
                // TODO
            }
        }
        
        enum EnumForRevision {
            REVISION,
            NOREVISION
        }
        
        // see https://github.com/opennars/opennars/blob/master/nars_core/nars/entity/Concept.java#L779
        /**
         * Select a belief value or desire value for a given query
         *
         * @param query The query to be processed
         * @param list The list of beliefs or desires to be used
         * @return The best candidate selected
         */
        ClassicalTask selectCandidate(ClassicalTask query, IList<ClassicalTask> list, EnumForRevision forRevision) {
            var rateByConfidence = TemporalRules.EnumRateByConfidence.YES; //table vote, yes/no question / local processing
            
            // TODO< paralelize somehow >

            float currentBest = 0;
            float beliefQuality;
            ClassicalTask candidate = null;
            //synchronized(list) {
                for (int i = 0; i < list.Count; i++) {
                    ClassicalTask judgT = list[i];
                    ClassicalSentence judg = judgT.sentence;
                    beliefQuality = TemporalRules.solutionQuality(rateByConfidence, query, judg, memory, compoundAndTermContext); // makes revision explicitly search for 
                    if (beliefQuality > currentBest /*&& (!forRevision || judgT.sentence.equalsContent(query)) */ /*&& (!forRevision || !Stamp.baseOverlap(query.stamp.evidentialBase, judg.stamp.evidentialBase)) */) {
                        currentBest = beliefQuality;
                        candidate = judgT;
                    }
                }
            //}
            return candidate;

        }

        private Memory memory;
        private CompoundAndTermContext compoundAndTermContext;
    }
}
