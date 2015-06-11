package ptrman.agix0.src.java.SuboptimalProcedureLearner;

import ptrman.Datastructures.Variadic;

import java.util.List;

abstract public class Scaffold extends AbstractOperatorBase {
    /**
     *
     * used to tell the executive which operator should be instanced and called with the arguments.
     *
     */
    public static class ExecutionRequest {
        public ExecutionRequest(final int callOperatorIndex, final List<Variadic> callParameters) {
            this.callOperatorIndex = callOperatorIndex;
            this.callParameters = callParameters;
        }

        public final int callOperatorIndex;
        public final List<Variadic> callParameters;
    }

    public Scaffold() {
        super(EnumType.SCAFFOLD);
    }

    public abstract ExecutionRequest executeScaffold();

    // gets called from the executive if a AbstractOperatorBase was executed
    abstract public void feedResult(Variadic result);
}
