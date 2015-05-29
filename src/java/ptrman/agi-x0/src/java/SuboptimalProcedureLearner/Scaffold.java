package SuboptimalProcedureLearner;

import Datastructures.Variadic;

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

    final public boolean isScaffold() {
        return true;
    }

    public abstract ExecutionRequest executeScaffold();
}
