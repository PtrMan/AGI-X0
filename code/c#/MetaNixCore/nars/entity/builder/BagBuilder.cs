using System;
using MetaNix.nars.config;
using MetaNix.nars.memory;

namespace MetaNix.nars.entity.builder {
    // used to create bags
    public class BagBuilder {
        public Bag<ClassicalTaskLink, ClassicalTask> createForConcept_tasksBag() {
            Bag<ClassicalTaskLink, ClassicalTask> result = new ArrayBag<ClassicalTaskLink, ClassicalTask>();
            result.setMaxSize(Parameters.TASK_LINK_BAG_SIZE);
            return result;
        }

        internal Bag<ClassicalTermLink, ClassicalTermLink> createForConcept_termLinksBag() {
            Bag<ClassicalTermLink, ClassicalTermLink> result = new ArrayBag<ClassicalTermLink, ClassicalTermLink>();
            result.setMaxSize(Parameters.TERM_LINK_BAG_SIZE);
            return result;
        }

        internal Bag<ClassicalTaskLink, ClassicalTask> createForConcept_taskLinksBag() {
            Bag<ClassicalTaskLink, ClassicalTask> result = new ArrayBag<ClassicalTaskLink, ClassicalTask>();
            result.setMaxSize(Parameters.TASK_LINK_BAG_SIZE);
            return result;
        }
    }
}
