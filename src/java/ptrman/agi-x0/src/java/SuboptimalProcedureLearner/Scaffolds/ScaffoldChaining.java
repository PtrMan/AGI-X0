package SuboptimalProcedureLearner.Scaffolds;

import Datastructures.Variadic;
import SuboptimalProcedureLearner.Operator;
import SuboptimalProcedureLearner.OperatorInstance;
import SuboptimalProcedureLearner.Scaffold;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.Arrays;
import java.util.List;

/**
 *
 * chains the Inner operator to the input
 * is mainly used for matching sequences.
 *
 * A B C D E F
 *
 * A B
 * \ /
 *  op
 *  |
 *  res0
 *
 *
 *  res0 C
 *   \  /
 *    op
 *    |
 *    res1
 *
 *       res1 D
 *         \ /
 *         op
 *          |
 *         res2
 *
 *  ...
 */
public class ScaffoldChaining extends Scaffold {
    @Override
    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) {
        throw new NotImplementedException();
    }

    @Override
    public OperatorInstance createOperatorInstance() {
        throw new RuntimeException("Scaffolds are not applicable with scaffolds!");
    }

    @Override
    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) {
        throw new RuntimeException("setParameterOperatorInstances() not applicable with scaffolds!");
    }

    @Override
    public void initializeOperatorInstance(OperatorInstance instance) {

    }

    @Override
    public ExecutionResult executeSingleStep(OperatorInstance instance) {
        throw new RuntimeException("executeSingleStep() not applicable with scaffolds!");
    }

    @Override
    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) {
        resultSoFar = result;
    }

    @Override
    public void operationCleanup(OperatorInstance instance) {

    }

    @Override
    public String getShortName() {
        return "ScaffoldChaining";
    }

    @Override
    public ExecutionRequest executeScaffold() {

        Variadic leftSide;
        Variadic rightSide;

        if( resultSoFar == null ) {
            leftSide = allRemainingArguments.get(0);
            rightSide = allRemainingArguments.get(1);

            allRemainingArguments.remove(0);
            allRemainingArguments.remove(0);
        }
        else {
            leftSide = resultSoFar;
            rightSide = allRemainingArguments.get(0);

            allRemainingArguments.remove(0);
        }

        return new ExecutionRequest(internalOperator, Arrays.asList(leftSide, rightSide));
    }

    private Operator internalOperator;
    private List<Variadic> allRemainingArguments;

    private Variadic resultSoFar = null; // is null if it is the first call
}
