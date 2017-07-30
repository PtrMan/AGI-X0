using MetaNix.nars.control;
using MetaNix.nars.entity;
using MetaNix.nars.memory;

namespace MetaNix.nars.inference {
    class LocalRules {
        /* -------------------- same contents -------------------- */
        /**
         * The task and belief have the same content
         * <p>
         * called in RuleTables.reason
         *
         * @param task The task
         * @param belief The belief
         * @param memory Reference to the memory
         */
        public static bool match(ClassicalTask task, ClassicalSentence belief, DerivationContext ctx) {
            ClassicalSentence sentence = task.sentence;

            if (sentence.isJudgment) {
                if (revisible(sentence, belief)) {
                    return revision(ctx, sentence, belief, true);
                }
            }
            else {
                // TODO
                /* commented because variables and order are not implemented
                if (matchingOrder(sentence, belief)) {
                    Term[] u = new Term[] { sentence.term, belief.term };
                    if (Variables.unify(Symbols.VAR_QUERY, u)) {
                        trySolution(belief, task, nal, true);
                    }
                }
                */
            }
            return false;
        }

        /**
         * Check if a Sentence provide a better answer to a Question or Goal
         *
         * \param belief The proposed answer
         * \param task The task to be processed
         */
        public static bool trySolution(ClassicalSentence belief, ClassicalTask task, DerivationContext ctx, bool report, Memory memory) {
            trySolution_emotion(belief, task, ctx);

            task.setBestSolution(memory, belief);
            
            //memory.logic.SOLUTION_BEST.commit(task.getPriority());
        
            ClassicalBudgetValue budget = TemporalRules.solutionEval(task, belief, task, ctx);

            if(budget == null || !budget.isAboveThreshold) {
                //memory.emit(Unsolved.class, task, belief, "Insufficient budget");
                return false;
            }
            // Solution was Activated

            // report
            if( task.sentence.isQuestion || task.sentence.isQuest ) {
                if(task.isInput && report) { //only show input tasks as solutions
                    //memory.emit(Answer.class, task, belief); 
                } else {
                    //memory.emit(Output.class, task, belief);   //solution to quests and questions can be always showed   
                }
            } else {
                //memory.emit(Output.class, task, belief);   //goal things only show silence related 
            }
            
            
            /*memory.output(task);
                        
            //only questions and quests get here because else output is spammed
            if(task.sentence.isQuestion() || task.sentence.isQuest()) {
                memory.emit(Solved.class, task, belief);          
            } else {
                memory.emit(Output.class, task, belief);            
            }*/
            
            ctx.addTask(ctx.currentTask, budget, belief, task.getParentBelief());
            return true;
        }

        // does the emotion part of the trySolution function
        private static void trySolution_emotion(ClassicalSentence belief, ClassicalTask task, DerivationContext ctx) {
            ClassicalSentence problem = task.sentence;
            Memory memory = ctx.memory;

            ClassicalSentence oldBest = task.bestSolution;
            if (oldBest != null) {
                TemporalRules.EnumRateByConfidence rateByConfidence =
                    oldBest.term == belief.term ? TemporalRules.EnumRateByConfidence.YES : TemporalRules.EnumRateByConfidence.NO;

                float newQ = TemporalRules.solutionQuality(rateByConfidence, task, belief, memory, ctx.compoundAndTermContext);
                float oldQ = TemporalRules.solutionQuality(rateByConfidence, task, oldBest, memory, ctx.compoundAndTermContext);
                if (oldQ >= newQ) {
                    if (problem.isGoal) {
                        memory.emotion.adjustHappy(oldQ, task.budget.priority, ctx);
                    }
                    //System.out.println("Unsolved: Solution of lesser quality");
                    //java memory.emit(Unsolved.class, task, belief, "Lower quality");               
                    return;
                }
            }
        }

        /**
         * Check whether two sentences can be used in revision
         *
         * @param s1 The first sentence
         * @param s2 The second sentence
         * @return If revision is possible between the two sentences
         */
        public static bool revisible(ClassicalSentence s1, ClassicalSentence s2) {
            /* TODO TODO
            if (!s1.isEternal() && !s2.isEternal() && Math.abs(s1.getOccurenceTime() - s2.getOccurenceTime()) > Parameters.REVISION_MAX_OCCURRENCE_DISTANCE) {
                return false;
            }
            return (s1.getRevisible() &&
                    matchingOrder(s1.getTemporalOrder(), s2.getTemporalOrder()) &&
                    CompoundTerm.cloneDeepReplaceIntervals(s1.term).equals(CompoundTerm.cloneDeepReplaceIntervals(s2.term)) &&
                    !Stamp.baseOverlap(s1.stamp.evidentialBase, s2.stamp.evidentialBase));
            */
            return true; // HACK TODO
        }

        /**
         * Belief revision
         * <p>
         * called from Concept.reviseTable and match
         *
         * @param newBelief The new belief in task
         * @param oldBelief The previous belief with the same content
         * @param feedbackToLinks Whether to send feedback to the links
         * @param memory Reference to the memory
         */
        public static bool revision(DerivationContext ctx, ClassicalSentence newBelief, ClassicalSentence oldBelief, bool feedbackToLinks) {
            if (newBelief.term == null) return false;

            newBelief.stamp.alreadyAnticipatedNegConfirmation = oldBelief.stamp.alreadyAnticipatedNegConfirmation;
            TruthValue newTruth = newBelief.truth;
            TruthValue oldTruth = oldBelief.truth;
            TruthValue truth = TruthFunctions.revision(newTruth, oldTruth);
            ClassicalBudgetValue budget = BudgetFunctions.revise(newTruth, oldTruth, truth, feedbackToLinks, ctx);

            if (budget.isAboveThreshold) {
                if (ctx.doublePremiseTaskRevised(newBelief.term, truth, budget)) {
                    //nal.mem().logic.BELIEF_REVISION.commit();
                    return true;
                }
            }

            return false;
        }
    }
}
