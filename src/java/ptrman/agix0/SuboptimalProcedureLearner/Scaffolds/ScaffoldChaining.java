package ptrman.agix0.SuboptimalProcedureLearner.Scaffolds;


import ptrman.Datastructures.Variadic;
import ptrman.agix0.SuboptimalProcedureLearner.Scaffold;

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
    public ScaffoldChaining() {
        super();
    }

    @Override
    public String getShortName() {
        return this.getClass().getName();
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

        return new ExecutionRequest(internalOperatorIndex, Arrays.asList(leftSide, rightSide));
    }

    @Override
    public void feedResult(Variadic result) {
        // TODO
        throw new RuntimeException("TODO");
    }

    private int internalOperatorIndex;
    private List<Variadic> allRemainingArguments;

    private Variadic resultSoFar = null; // is null if it is the first call
}
