package SuboptimalProcedureLearner;

import Datastructures.Variadic;

import java.util.List;

abstract public class Scaffold extends Operator {
    /**
     *
     * used to tell the executive which operator should be instanced and called with the arguments.
     *
     */
    public static class ExecutionRequest {
        public ExecutionRequest(final Operator callOperator, final List<Variadic> callParameters) {
            this.callOperator = callOperator;
            this.callParameters = callParameters;
        }

        public final Operator callOperator;
        public final List<Variadic> callParameters;
    }

    final public boolean isScaffold() {
        return true;
    }

    public abstract ExecutionRequest executeScaffold();
}
