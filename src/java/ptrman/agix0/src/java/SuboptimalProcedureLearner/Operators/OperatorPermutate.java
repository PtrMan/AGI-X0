package ptrman.agix0.src.java.SuboptimalProcedureLearner.Operators;


import ptrman.Datastructures.Variadic;
import ptrman.agix0.src.java.SuboptimalProcedureLearner.Operator;
import ptrman.agix0.src.java.SuboptimalProcedureLearner.OperatorInstance;
import ptrman.misc.Assert;

import java.util.List;

/**
 *
 * Permutate the bits of the input
 *
 * each constant must be an int and the constant array contains the rewiring indices
 * there are no constraints on it
 *
 */
public class OperatorPermutate extends Operator {
    private enum EnumOperationState {
        EXECUTEOPERAND,
        EXECUTE,
    }

    public OperatorPermutate() {
        super();
    }

    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) {
        IntrospectWiringInfo wiringInfo = new IntrospectWiringInfo();
        wiringInfo.type = IntrospectWiringInfo.EnumType.VALIDWIRING;
        wiringInfo.ouputIndices = new int[inputIndices.length];
        // NOTE< we let the environemnt catch all indexing errors >
        // maybe this is not good
        for( int i = 0;i < inputIndices.length; i++ ) {
            int queriedIndex = inputIndices[i];
            Assert.Assert(instance.constants[queriedIndex].type == Variadic.EnumType.INT, "");
            int resultIndex = instance.constants[queriedIndex].valueInt;
            wiringInfo.ouputIndices[i] = resultIndex;
        }
        return wiringInfo;
    }

    public OperatorInstance createOperatorInstance() {
        return new OperatorInstance(this);
    }

    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) {
    }

    public void initializeOperatorInstance(OperatorInstance instance) {
    }

    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        EnumOperationState operationState = EnumOperationState.values()[instance.operationState];
        if( operationState == EnumOperationState.EXECUTEOPERAND ) {
            return ExecutionResult.createExecuteInstance(instance);
        }
        else if( operationState == EnumOperationState.EXECUTE ) {
            // should we throw something?
            Assert.Assert(instance.calleeResults.get(0).type == Variadic.EnumType.INT, "");

            int permutatedResult = permutate(instance.calleeResults.get(0).valueInt, instance.constants);
            Variadic result = new Variadic(Variadic.EnumType.INT);
            result.valueInt = permutatedResult;
            return ExecutionResult.createResultInstance(result);
        }
        else {
            throw new InternalError();
        }
    }

    private static int permutate(final int sourceValue, final Variadic[] constants) {
        int result = 0;

        for( int sourceIndex = 0; sourceIndex < constants.length; sourceIndex++ ) {
            final int resultIndex = constants[sourceIndex].valueInt;

            final boolean sourceBit = ((sourceValue >> sourceIndex) & 1) != 0;

            if( sourceBit ) {
                result |= (1 << resultIndex);
            }
        }

        return result;
    }

    // gets called from the executive if a (requested execution) operator(instance) was executed
    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) {
        instance.calleeResults.add(result);
        instance.operationState++;
    }

    public void operationCleanup(OperatorInstance instance) {
    }

    public String getShortName() {
        return this.getClass().getName();
    }
}
