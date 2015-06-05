package SuboptimalProcedureLearner.Operators;

import Datastructures.Variadic;
import SuboptimalProcedureLearner.Operator;
import SuboptimalProcedureLearner.OperatorInstance;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.List;

public class OperatorArithmetic extends Operator {
    private enum EnumOperationState {
        EXECUTELEFTOPERAND,
        EXECUTERIGHTOPERAND,
        EXECUTE
    }

    public enum EnumType {
        ADD,
        SUB,
        MUL,
        DIV,
        MAX,
        MIN,
    }

    public OperatorArithmetic() {
        super();
    }

    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) {
        throw new NotImplementedException();
    }

    public OperatorInstance createOperatorInstance() {
        return new OperatorInstance(this);
    }

    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) {
        if (parameterInstances.size() != 2) {
            throw new RuntimeException(getShortName() + ": The must be only two parameterInstances!");
        }

        instance.calleeOperatorInstances = new OperatorInstance[2];
        instance.calleeOperatorInstances[0] = parameterInstances.get(0);
        instance.calleeOperatorInstances[1] = parameterInstances.get(1);
    }

    public void initializeOperatorInstance(OperatorInstance instance) {
        instance.calleeResults = new ArrayList<>();
    }

    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        EnumType type = EnumType.values()[instance.constants[0].valueInt];

        EnumOperationState operationState = EnumOperationState.values()[instance.operationState];
        if (operationState == EnumOperationState.EXECUTELEFTOPERAND) {
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[0]);
        }
        else if (operationState == EnumOperationState.EXECUTERIGHTOPERAND) {
            return ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[1]);
        }
        else {
            // operationState == EnumOperationState.execute
            Variadic resultVariadic = null;
            boolean resultAsFloat = instance.calleeResults.get(0).type == Variadic.EnumType.FLOAT || instance.calleeResults.get(1).type == Variadic.EnumType.FLOAT;
            if (resultAsFloat) {
                float a = getVariadicAsFloat(instance.calleeResults.get(0));
                float b = getVariadicAsFloat(instance.calleeResults.get(1));
                float floatResult;

                if( type == EnumType.ADD ) {
                    floatResult = a + b;
                }
                else if( type == EnumType.SUB ) {
                    floatResult = a - b;
                }
                else if( type == EnumType.MUL ) {
                    floatResult = a * b;
                }
                else if( type == EnumType.DIV ) {
                    floatResult = a / b;
                }
                else if( type == EnumType.MAX ) {
                    floatResult = java.lang.Math.max(a, b);
                }
                else if( type == EnumType.MIN ) {
                    floatResult = java.lang.Math.min(a, b);
                }
                else {
                    throw new InternalError();
                }

                resultVariadic = new Variadic(Variadic.EnumType.FLOAT);
                resultVariadic.valueFloat = floatResult;
            }
            else {
                int a = instance.calleeResults.get(0).valueInt;
                int b = instance.calleeResults.get(1).valueInt;
                int intResult;

                if( type == EnumType.ADD ) {
                    intResult = a + b;
                }
                else if( type == EnumType.SUB ) {
                    intResult = a - b;
                }
                else if( type == EnumType.MUL ) {
                    intResult = a * b;
                }
                else if( type == EnumType.DIV ) {
                    intResult = a / b;
                }
                else if( type == EnumType.MAX ) {
                    intResult = java.lang.Math.max(a, b);
                }
                else if( type == EnumType.MIN ) {
                    intResult = java.lang.Math.min(a, b);
                }
                else {
                    throw new InternalError();
                }

                resultVariadic = new Variadic(Variadic.EnumType.INT);
                resultVariadic.valueInt = intResult;
            } 
            return ExecutionResult.createResultInstance(resultVariadic);
        }  
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

    static private float getVariadicAsFloat(Variadic value) {
        if (value.type == Variadic.EnumType.FLOAT) {
            return value.valueFloat;
        }
        else if (value.type == Variadic.EnumType.INT) {
            return (float)value.valueInt;
        }
        else {
            throw new RuntimeException("Can't convert varidic to float!");
        }  
    }
}
