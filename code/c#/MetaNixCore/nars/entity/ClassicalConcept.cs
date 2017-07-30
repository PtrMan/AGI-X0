using System.Collections.Generic;

using MetaNix.nars.control;
using MetaNix.nars.entity.builder;
using MetaNix.nars.memory;
using MetaNix.nars.config;

namespace MetaNix.nars.entity {
    public class ClassicalConcept : Item<TermOrCompoundTermOrVariableReferer>, INamed<TermOrCompoundTermOrVariableReferer> {

        public TermOrCompoundTermOrVariableReferer term; // The term is the unique ID of the concept

        public Bag<ClassicalTaskLink, ClassicalTask> taskLinks; // Task links for indirect processing
        public Bag<ClassicalTermLink, ClassicalTermLink> termLinks; // Term links between the term and its components and compounds; beliefs

        public Memory memory; /** Reference to the memory to which the Concept belongs */

        Bag<ClassicalTaskLink, ClassicalTask> tasks;
        //uncommented because of some termLink/belief confusion  Bag!(ClassicalBelief, TermOrCompoundTermOrVariableReferer) termLinks;

        // OPTIMIZE< use array instead, could be faster >
        public IList<ClassicalTask>
            questions = new List<ClassicalTask>(), /** Pending Question directly asked about the term */
            desires = new List<ClassicalTask>(); /** Desire values on the term, similar to the above one */

        /**
         * Link templates of TermLink, only in concepts with CompoundTerm Templates
         * are used to improve the efficiency of TermLink building
         */
        public IList<ClassicalTermLink> termLinkTemplates;


        /**
         * Judgments directly made about the term 
         *
         * Uses Array because of access and insertion in the middle
         */
        public IList<ClassicalTask> beliefs = new List<ClassicalTask>();
        
        public ClassicalConcept(CompoundAndTermContext compoundAndTermContext, TermOrCompoundTermOrVariableReferer term, BagBuilder bagBuilder, ClassicalBudgetValue budget, Memory memory) : base(budget) {
            this.memory = memory;
            this.term = term;

            tasks = bagBuilder.createForConcept_tasksBag();

            taskLinks = bagBuilder.createForConcept_taskLinksBag();
		    termLinks = bagBuilder.createForConcept_termLinksBag();

            { // calculate term links
                bool isCompoundTerm = !term.isAtomic && !term.isVariable;
                if( isCompoundTerm ) {
                    // TODO< decide if it is COMPOUND_STATEMENT or COMPOUND >
                    // https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/language/Terms.java#L470;
                    ClassicalTermLink.EnumType type = ClassicalTermLink.EnumType.COMPOUND;

                    this.termLinkTemplates = TermHelper.prepareComponentLinks(compoundAndTermContext, term, type);
                }
            }
        }

        public override TermOrCompoundTermOrVariableReferer name { get {
            return term;
        } }

        // from https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/entity/Concept.java
        /**
         * Select a belief to interact with the given task in inference
         * 
         * get the first qualified one
         *
         * \param task The selected task
         * \return The selected isBelief
         */
        public ClassicalSentence getBelief(DerivationContext nal, ClassicalTask task) {
            Stamp taskStamp = task.sentence.stamp;
            ClassicalSentence taskSentence = task.sentence;
            long currentTime = memory.time;

            int i = 0;
            foreach (ClassicalTask iBelief in beliefs) {
                ClassicalSentence beliefSentence = iBelief.sentence;

                // uncommented because event mechanism is not in place jet
                //nal.emit(EnumXXX.BELIEF_SELECT, iBelief);
                nal.setTheNewStamp(taskStamp, iBelief.sentence.stamp, currentTime);

                // TODO< projection >

                // return the first satisfying belief
                // HINT HUMAN< to return the first value doesn't make much sense without looking at the classical NARS code >
                return beliefs[i].sentence;
            }
            return null;
        }

        // see https://github.com/opennars/opennars/blob/c3564fc6c176ceda168a9631d27a07be05faff32/nars_core/nars/entity/Concept.java#L1191
        /**
         * Replace default to prevent repeated inference, by checking TaskLink
         *
         * /param taskLink The selected TaskLink
         * /param time The current time
         * /return The selected TermLink
         */
        public ClassicalTermLink selectTermLink(ClassicalTaskLink taskLink, long time) {
            maintainDisappointedAnticipations();
            uint toMatch = Parameters.TERM_LINK_MAX_MATCHED;
            for (int i = 0; (i < toMatch) && (termLinks.size > 0); i++) {
                ClassicalTermLink termLink = termLinks.takeNext();
                if (termLink == null)
                    break;

                if (taskLink.checkNovel(termLink, time)) {
                    //return, will be re-inserted in caller method when finished processing it
                    return termLink;
                }

                returnTermLink(termLink);
            }
            return null;
        }
        
        // see https://github.com/opennars/opennars/blob/c3564fc6c176ceda168a9631d27a07be05faff32/nars_core/nars/entity/Concept.java#L1212
        public void returnTermLink(ClassicalTermLink termLink) {
            termLinks.putBack(termLink, memory.convertDurationToCycles(memory.param.termLinkForgetDurations), memory);
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/Concept.java#L1152
        private void maintainDisappointedAnticipations() {
            // TODO LOW< transfer from OpenNARS >
        }
        
        override public void wasDiscarded() {
            foreach( var iQuestion in questions ) {
                iQuestion.wasDiscarded();
            }
            questions.Clear();

            // commented because there are no quests
            //foreach( var iQuest in quests ) {
            //    iQuest.wasDiscarded();
            //}
            //quests.Clear();

            desires.Clear();
            //evidentalDiscountBases.clear();
            termLinks.clear();
            taskLinks.clear();
            beliefs.Clear();
            //commented because termLinkTemplates are not implemented  termLinkTemplates.clear();
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/Concept.java#L923
        /**
         * Recursively build TermLinks between a compound and its components
         * <p>
         * called only from Memory.continuedProcess
         *
         * /param taskBudget The BudgetValue of the task
         */
        public void buildTermLinks(ClassicalBudgetValue taskBudget) {
            if( termLinkTemplates.Count == 0 ) {
                return;
            }
            
            ClassicalBudgetValue subBudget = taskBudget; // HACK // uncommented because not yet implemented, TODO     distributeAmongLinks(taskBudget, termLinkTemplates.size());
            
            if( !subBudget.isAboveThreshold ) {
                return;
            }
            
            foreach (ClassicalTermLink template in termLinkTemplates) {
                if (template.type == ClassicalTermLink.EnumType.TRANSFORM) {
                    continue;
                }
                
                TermOrCompoundTermOrVariableReferer target = template.target;
                
                ClassicalConcept concept = memory.conceptualize(taskBudget, target);
                if (concept == null) {
                    continue;
                }
                
                // this termLink to that
                insertTermLink(ClassicalTermLink.makeFromTemplate(target, template, subBudget));
                
                // that termLink to this
                concept.insertTermLink(ClassicalTermLink.makeFromTemplate(term, template, subBudget));
                
                if( TermUtilities.isTermCompoundTerm(target) && template.type != ClassicalTermLink.EnumType.TEMPORAL ) {
                    concept.buildTermLinks(subBudget);
                }
            }
        }

        /**
         * Insert a TermLink into the TermLink bag
         * <p>
         * called from buildTermLinks only
         *
         * /param termLink The termLink to be inserted
         */
        public bool insertTermLink(ClassicalTermLink termLink) {
            ClassicalTermLink removed = termLinks.putIn(termLink);
            if (removed != null) {
                if (removed == termLink) {
                    //OpenNARS memory.emit(TermLinkRemove.class, termLink, this);
                    return false;
                }
                else {
                    //OpenNARS memory.emit(TermLinkRemove.class, removed, this);
                }
            }
            //OpenNARS memory.emit(TermLinkAdd.class, termLink, this);
            return true;
        }



        /**
         * Insert a TaskLink into the TaskLink bag for indirect processing
         *
         * /param taskLink The termLink to be inserted
         */
        public bool insertTaskLink(ClassicalTaskLink taskLink, DerivationContext nal) {
            ClassicalTask target = taskLink.targetTask;
            ClassicalTask ques = taskLink.targetTask;

            // TODO< implement if variables are implemented >
            /* commented and not translated because variables not implemented
            if((ques.sentence.isQuestion() || ques.sentence.isQuest()) && ques.getTerm().hasVarQuery()) { //ok query var, search
                boolean newAnswer = false;
            
                for(TaskLink t : this.taskLinks) {
                
                    Term[] u = new Term[] { ques.getTerm(), t.getTerm() };
                    if(!t.getTerm().hasVarQuery() && Variables.unify(Symbols.VAR_QUERY, u)) {
                        Concept c = nal.memory.concept(t.getTerm());
                        if(c != null && ques.sentence.isQuestion() && c.beliefs.size() > 0) {
                            final Task taskAnswer = c.beliefs.get(0);
                            if(taskAnswer!=null) {
                                newAnswer |= trySolution(taskAnswer.sentence, ques, nal, false); //order important here
                            }
                        }
                        if(c != null && ques.sentence.isQuest() &&  c.desires.size() > 0) {
                            final Task taskAnswer = c.desires.get(0);
                            if(taskAnswer!=null) {
                                newAnswer |= trySolution(taskAnswer.sentence, ques, nal, false); //order important here
                            }
                        }
                    }
                }
                if(newAnswer && ques.isInput()) {
                    memory.emit(Events.Answer.class, ques, ques.getBestSolution()); 
                }
            }*/

            // TODO< implement if variables are implemented >
            /* commented and not translated because variables not implemented
            //belief side:
            Task t = taskLink.getTarget();
            if(t.sentence.isJudgment()) { //ok query var, search
                for(TaskLink quess: this.taskLinks) {
                    ques = quess.getTarget();
                    if((ques.sentence.isQuestion() || ques.sentence.isQuest()) && ques.getTerm().hasVarQuery()) {
                        boolean newAnswer = false;
                        Term[] u = new Term[] { ques.getTerm(), t.getTerm() };
                        if(!t.getTerm().hasVarQuery() && Variables.unify(Symbols.VAR_QUERY, u)) {
                            Concept c = nal.memory.concept(t.getTerm());
                            if(c != null && ques.sentence.isQuestion() && c.beliefs.size() > 0) {
                                final Task taskAnswer = c.beliefs.get(0);
                                if(taskAnswer!=null) {
                                    newAnswer |= trySolution(taskAnswer.sentence, ques, nal, false); //order important here
                                }
                            }
                            if(c != null && ques.sentence.isQuest() &&  c.desires.size() > 0) {
                                final Task taskAnswer = c.desires.get(0);
                                if(taskAnswer!=null) {
                                    newAnswer |= trySolution(taskAnswer.sentence, ques, nal, false); //order important here
                                }
                            }
                        }
                        if(newAnswer && ques.isInput()) {
                            memory.emit(Events.Answer.class, ques, ques.getBestSolution()); 
                        }
                    }
                }
            }
            */

            { // handle max per concept
                // if taskLinks already contain a certain amount of tasks with same content then one has to go
                bool isEternal = target.sentence.stamp.isEternal;
                int nSameContent = 0;
                float lowest_priority = float.MaxValue;
                ClassicalTaskLink lowest = null;
                foreach( ClassicalTaskLink tl in taskLinks ) {
                    ClassicalSentence s = tl.targetTask.sentence;
                    if( s.term == taskLink.targetTask.sentence.term && s.stamp.isEternal == isEternal ) {
                        nSameContent++; // same content and occurrence-type, so count +1
                        if( tl.budget.priority < lowest_priority ) { //the current one has lower priority so save as lowest
                            lowest_priority = tl.budget.priority;
                            lowest = tl;
                        }
                        if( nSameContent > Parameters.TASKLINK_PER_CONTENT ) { // ok we reached the maximum so lets delete the lowest
                            taskLinks.takeElement(lowest);
                            // commented because events not implemented yet  memory.emit(TaskLinkRemove.class, lowest, this);
                            break;
                        }
                    }
                }
            }        
        
            ClassicalTaskLink removed = taskLinks.putIn(taskLink);
            if( removed!=null ) {
                if (removed == taskLink) {
                    // commented because events not implemented yet memory.emit(TaskLinkRemove.class, taskLink, this);
                    return false;
                }
                else {
                    // commented because events not implemented yet memory.emit(TaskLinkRemove.class, removed, this);
                }

                removed.wasDiscarded();
            }
            // commented because events not implemented yet memory.emit(TaskLinkAdd.class, taskLink, this);
            return true;
        }
    }
}
