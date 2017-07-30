using MetaNix.nars.config;
using MetaNix.nars.memory;
using MetaNix.nars.entity;
using MetaNix.nars.control.attention;

namespace MetaNix.nars {
    // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/NAR.java
    public class Nar {
        public static Nar make(CompoundAndTermContext compoundAndTermContext, RuntimeParameters runtimeParameters) {
            Nar created = new Nar(compoundAndTermContext, runtimeParameters);
            return created;
        }

        private Nar(CompoundAndTermContext compoundAndTermContext, RuntimeParameters runtimeParameters) {
            Memory m = createMemory(compoundAndTermContext, runtimeParameters);
            this.memory = m;
            this.param = runtimeParameters;
        }

        static Memory createMemory(CompoundAndTermContext compoundAndTermContext, RuntimeParameters runtimeParameters) {
            IAttentionMechanism<ClassicalTask> attentionMechanism = new BagBasedAttentionMechanism();
            attentionMechanism.setMaxSize(Parameters.NOVEL_TASK_BAG_SIZE);

            return new Memory(
                compoundAndTermContext,
                runtimeParameters,
                attentionMechanism
                //commented because OpenNARS version   new ArrayBag<>(Parameters.SEQUENCE_BAG_LEVELS, Parameters.SEQUENCE_BAG_SIZE)
            );
        }

        public void inputTask(ClassicalTask task) {
            memory.perceptNewTaskWithBudgetCheck(task);
        }

        public void cycle() {
            memory.cycle();
        }

        public Memory memory;
        RuntimeParameters param;
    }
}
