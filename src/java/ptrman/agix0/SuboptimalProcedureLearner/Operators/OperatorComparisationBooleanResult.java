package ptrman.agix0.SuboptimalProcedureLearner.Operators;

import ptrman.Datastructures.Variadic;
import ptrman.agix0.SuboptimalProcedureLearner.Operator;
import ptrman.agix0.SuboptimalProcedureLearner.OperatorInstance;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * compares two values (int or float) and returns the result
 * casts to the higher type
 */
public class OperatorComparisationBooleanResult extends Operator {
    private enum EnumOperationState {
        EXECUTELEFTOPERAND,
        EXECUTERIGHTOPERAND,
        EXECUTE
    }

    public enum EnumOperation {
        INVALID,

        GREATERTHAN,
        LESSTHAN,
        GREATEREQUAL,
        LESSEQUAL,
        EQUAL,
        UNEQUAL
    }

    public OperatorComparisationBooleanResult() {
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
        if (parameterInstances.size() != 2) {
            throw new RuntimeException(getShortName() + ": The must be only two parameterInstances!");
        }

        instance.calleeOperatorInstances = new OperatorInstance[2];
        instance.calleeOperatorInstances[0] = parameterInstances.get(0);
        instance.calleeOperatorInstances[1] = parameterInstances.get(1);
    }

    @Override
    public void initializeOperatorInstance(OperatorInstance instance) {
        instance.calleeResults = new ArrayList<>();
    }

    @Override
    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        EnumOperation operation = EnumOperation.values()[instance.constants[0].valueInt];

        EnumOperationState operationState = EnumOperationState.values()[instance.operationState];
        if (operationState == EnumOperationState.EXECUTELEFTOPERAND) {
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[0]);
        }
        else if (operationState == EnumOperationState.EXECUTERIGHTOPERAND) {
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[1]);
        }
        else {
            final Variadic a = instance.calleeResults.get(0);
            final Variadic b = instance.calleeResults.get(1);

            Variadic.EnumType highestNumericType = Variadic.highestNumericType(Arrays.asList(a.type, b.type));
            Variadic aCasted = Variadic.convertNumericTo(a, highestNumericType);
            Variadic bCasted = Variadic.convertNumericTo(b, highestNumericType);
            final boolean compaisationResult = compareValues(aCasted, bCasted, highestNumericType, operation);

            Variadic resultVariadic = new Variadic(Variadic.EnumType.BOOL);
            resultVariadic.valueBool = compaisationResult;

            return ExecutionResult.createResultInstance(resultVariadic);
        }
    }


    @Override
    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) {
        instance.calleeResults.add(result);
        instance.operationState++;
    }

    @Override
    public void operationCleanup(OperatorInstance instance) {
    }

    @Override
    public String getShortName() {
        return this.getClass().getName();
    }

    private static boolean compareValues(final Variadic a, final Variadic b, final Variadic.EnumType commonType, final EnumOperation operation) {
        if( commonType == Variadic.EnumType.FLOAT ) {
            switch (operation) {
                case GREATERTHAN: return a.valueFloat > b.valueFloat;
                case LESSTHAN: return a.valueFloat < b.valueFloat;
                case GREATEREQUAL: return a.valueFloat >= b.valueFloat;
                case LESSEQUAL: return a.valueFloat <= b.valueFloat;
                case EQUAL: return a.valueFloat == b.valueFloat;
                case UNEQUAL: return a.valueFloat != b.valueFloat;
                default: throw new InternalError();
            }
        }
        else if( commonType == Variadic.EnumType.INT ) {
            switch (operation) {
                case GREATERTHAN: return a.valueInt > b.valueInt;
                case LESSTHAN: return a.valueInt < b.valueInt;
                case GREATEREQUAL: return a.valueInt >= b.valueInt;
                case LESSEQUAL: return a.valueInt <= b.valueInt;
                case EQUAL: return a.valueInt == b.valueInt;
                case UNEQUAL: return a.valueInt != b.valueInt;
                default: throw new InternalError();
            }
        }
        else {
            throw new InternalError();
        }

    }

}
