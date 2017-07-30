using MetaNix.nars.entity;
using MetaNix.nars.memory;

namespace MetaNix.nars.control {
    /**
     * stores the global concepts
     * is conceptually something between the details of the working cycle and the memory
     *
     * see https://github.com/opennars/opennars/blob/1.6.5_devel17_TonyAnticipationStrategy/nars_core/nars/control/WorkingCycle.java
     * for OpenNars 1.6.x inspiration
     */
    public class WorkingCyclish {
        // only the term or compound term is refered (for lookup)
        public Bag<ClassicalConcept, TermOrCompoundTermOrVariableReferer> concepts; /** all concepts which the memory knows, adressed by term because we have to find it by term */
    }
}
