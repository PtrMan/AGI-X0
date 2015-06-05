package SuboptimalProcedureLearner.Operators;

import Datastructures.Variadic;
import SuboptimalProcedureLearner.Operator;
import SuboptimalProcedureLearner.OperatorInstance;
import ptrman.misc.Assert;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.List;

/**
 * Selects (at each execution after the execution of the selected operand or at the begining) between the input operators and executes them
 */
public class OperatorSelect extends Operator {
    private enum EnumOperationState {
        EXECUTELEFTOPERAND,
        EXECUTESELECTED,
        RETURN
    }

    public OperatorSelect() {
        super();
    }

    @Override
    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) {
        throw new NotImplementedException();
    }

    @Override
    public OperatorInstance createOperatorInstance() {
        return new OperatorInstance(this);
    }

    @Override
    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) {
        if( parameterInstances.size() >= 1 ) {
            throw new RuntimeException(getShortName() + ": The must at least two parameterInstances!");
        }

        instance.calleeOperatorInstances = new OperatorInstance[parameterInstances.size()];

        for( int i = 0; i < parameterInstances.size(); i++ ) {
            instance.calleeOperatorInstances[i] = parameterInstances.get(i);
        }
    }

    @Override
    public void initializeOperatorInstance(OperatorInstance instance) {
        instance.calleeResults = new ArrayList<>();
    }

    @Override
    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        EnumOperationState operationState = EnumOperationState.values()[instance.operationState];
        if (operationState == EnumOperationState.EXECUTELEFTOPERAND) {
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[0]);
        }
        else if (operationState == EnumOperationState.EXECUTESELECTED) {
            // TODO< throw catchable error ? >
            Assert.Assert(instance.calleeResults.get(0).type == Variadic.EnumType.INT, "");
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[1 + instance.calleeResults.get(0).valueInt]);
        }
        else if( operationState == EnumOperationState.RETURN ) {
            Variadic result = instance.calleeResults.get(1);
            return ExecutionResult.createResultInstance(result);
        }
        else {
            throw new InternalError();
        }
    }

    @Override
    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) {
        instance.calleeResults.add(result);

        // we assume that the instance is created before each execution, if not we have to wrap around
        Assert.Assert(instance.operationState <= 1, "");
        instance.operationState++;
    }

    @Override
    public void operationCleanup(OperatorInstance instance) {
    }

    @Override
    public String getShortName() {
        return this.getClass().getName();
    }
}
