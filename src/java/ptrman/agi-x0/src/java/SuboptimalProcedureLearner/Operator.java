package SuboptimalProcedureLearner;

import Datastructures.Variadic;

import java.util.List;

abstract public class Operator extends AbstractOperatorBase {
    // transfers information about the
    public static class IntrospectWiringInfo {
        public enum EnumType {
            FUSINGOPERATOR,
            // the operator does at some point fuse the input indices together with an operation
            VALIDWIRING,
            NOWIRING
        }
        // there is no wiring for the chosen input
        public EnumType type = EnumType.FUSINGOPERATOR;
        public int[] ouputIndices;
    }

    public static class ExecutionResult {
        public enum EnumResultState {
            ERROR,
            EXECUTEOPERATORINSTANCE,
            // the executive has to execute the operator
            RESULT
        }
        public static ExecutionResult createExecuteInstance(OperatorInstance instanceToExecute) {
            ExecutionResult executionResult = new ExecutionResult();
            executionResult.resultState = EnumResultState.EXECUTEOPERATORINSTANCE;
            executionResult.instanceToExecute = instanceToExecute;
            return executionResult;
        }

        public static ExecutionResult createResultInstance(Variadic resultValue) {
            ExecutionResult executionResult = new ExecutionResult();
            executionResult.resultState = EnumResultState.RESULT;
            executionResult.resultValue = resultValue;
            return executionResult;
        }

        public EnumResultState resultState = EnumResultState.ERROR;
        public OperatorInstance instanceToExecute;
        public Variadic resultValue;
    }

    public Operator() {
        super(EnumType.OPERATOR);
    }

    abstract public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices);

    abstract public OperatorInstance createOperatorInstance();

    abstract public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances);

    abstract public void initializeOperatorInstance(OperatorInstance instance);

    abstract public ExecutionResult executeSingleStep(OperatorInstance instance);

    // gets called from the executive if a (requested execution) operator(instance) was executed
    abstract public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result);

    //abstract public List<int> operationGetSlotIndicesBefore(OperatorInstance instance);
    /* gets the operator which must be executed next before this operator can be executed
     *
     * throws an error if the index is not valid (out of range or unused)
     *
     * \param instance the instance of the operator to be executed in the future/asked
     * \param slotIndex the slot to be asked
     * \param callParameters will be filled with the parameters for the to be called operator
     */
    //abstract public Operator operationOperatorBefore(OperatorInstance instance, int slotIndex, List<Variadic> callParameters);
    /*
     * gets the value of the parameter of the operator which should be called before this operator
     *
     * \param instance the instance of the operator to be executed in the future/asked
     * \param slotIndex ...
     * \param subslotIndex ...
     */
    //abstract public Variadic operationOperatorBeforeGetParameterValueBySlot(OperatorInstance instance, int slotIndex, int subslotIndex);
    //abstract public ExecutionResult operationExecute(OperatorInstance instance, List<Variadic> parameters);
    /**
     * gets called after the execution of the operator
     *
     *
     */
    abstract public void operationCleanup(OperatorInstance instance);
}
