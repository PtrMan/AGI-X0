using System.Collections.Generic;

using MetaNix.nars.memory;
using MetaNix.nars.config;

namespace MetaNix.nars.entity {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_TonyAnticipationStrategy/nars_core/nars/entity/TaskLink.java
    /**
     * Reference to a Task.
     * 
     * The reason to separate a Task and a TaskLink is that the same Task can be
     * linked from multiple Concepts, with different budget.
     * 
     * TaskLinks are unique according to the Task they reference
     */
    public class ClassicalTaskLink : Item<ClassicalTask>, INamed<ClassicalTask> {
        public ClassicalTask targetTask;

        public override ClassicalTask name { get {
            return targetTask;
        }}

        readonly uint recordLength;

        readonly public ClassicalTermLink.EnumType type; /** The type of link */

        /** The index of the component in the component list of the compound, may have up to 4 levels */
        readonly public uint[] index;

        /* Remember the TermLinks, and when they has been used recently with this TaskLink */
        private class Record {
            public readonly ClassicalTermLink link;
            public long time;

            public Record(ClassicalTermLink link, long time) {
                this.link = link;
                this.time = time;
            }
        }
        
        // used as a queue
        // TODO< rewrite to using a stable bloom filter like seth is doing it
        //       see https://github.com/automenta/narchy/blob/f5609a7150e927c67fa963e79ffad457f2597279/util/src/main/java/jcog/bloom/StableBloomFilter.java
        // >
        IList<Record> records = new List<Record>();

        /**
         * /param t The target Task
         * /param template The TermLink template
         * /param budget The budget
         */
        public ClassicalTaskLink(ClassicalTask t, ClassicalTermLink template, ClassicalBudgetValue budget, uint recordLength) : base(budget) {
            this.type =
                    template == null ?
                            ClassicalTermLink.EnumType.SELF :
                            template.type;
            this.index =
                    template == null ?
                            null :
                            template.index;

            this.targetTask = t;

            this.recordLength = recordLength;
        }

        // see https://github.com/opennars/opennars/blob/df5878a54a456c9a692004ae770d4613d31a757d/nars_core/nars/entity/TaskLink.java#L154
        /**
         * To check whether a TaskLink should use a TermLink, return false if they interacted recently
         * <p>
         * called in TermLinkBag only
         *
         * /param termLink The TermLink to be checked
         * /param currentTime The current time
         * /return Whether they are novel to each other
         */
        public bool checkNovel(ClassicalTermLink termLink, long currentTime) {
            TermOrCompoundTermOrVariableReferer bTerm = termLink.target;
            if( TermOrCompoundTermOrVariableReferer.isSameWithId(bTerm, targetTask.sentence.term) ) {
                return false;
            }
            ClassicalTermLink linkKey = termLink.name;

            // iterating the FIFO deque from oldest (first) to newest (last)
            for( int i = 0; i < records.Count; i++ ) {
                Record iRecord = records[i];
                if (linkKey.checkEqual(iRecord.link)) {
                    if (currentTime < iRecord.time + Parameters.NOVELTY_HORIZON) {
                        // too recent, not novel
                        return false;
                    }
                    else {
                        // happened long enough ago that we have forgotten it somewhat, making it seem more novel
                        iRecord.time = currentTime;

                        records.RemoveAt(i);
                        i--;

                        records.Add(iRecord);
                        return true;
                    }
                }
            }

            // keep recordedLinks queue a maximum finite size
            while (records.Count + 1 >= recordLength) {
                records.RemoveAt(0); // remove first
            }

            // add knowledge reference to recordedLinks
            records.Add(new Record(linkKey, currentTime));

            return true;
        }

        override public void wasDiscarded() {
            records.Clear();
        }
    }
}
