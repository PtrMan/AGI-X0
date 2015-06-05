package SuboptimalProcedureLearner.Operators;

import Datastructures.Variadic;
import SuboptimalProcedureLearner.Operator;
import SuboptimalProcedureLearner.OperatorInstance;
import ptrman.misc.Assert;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.List;

public class OperatorConstant  extends Operator {
    public OperatorConstant() {
        super();
    }

    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) {
        throw new NotImplementedException();
    }

    public OperatorInstance createOperatorInstance() {
        return new OperatorInstance(this);
    }

    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) {
        Assert.Assert(parameterInstances.size() == 1, "");
        instance.constants = new Variadic[1];
        instance.constants[0] = parameterInstances.get(0).constants[0];
    }

    public void initializeOperatorInstance(OperatorInstance instance) {
    }

    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        return ExecutionResult.createResultInstance(instance.constants[0]);
    }

    // gets called from the executive if a (requested execution) operator(instance) was executed
    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) {
        int x = 0;
    }

    public void operationCleanup(OperatorInstance instance) {
    }

    public String getShortName() {
        return this.getClass().getName();
    }
}
