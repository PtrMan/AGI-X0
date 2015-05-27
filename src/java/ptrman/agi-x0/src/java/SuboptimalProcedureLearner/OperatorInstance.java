package SuboptimalProcedureLearner;

import Datastructures.Variadic;

import java.util.ArrayList;
import java.util.List;

public class OperatorInstance {
    public OperatorInstance(Operator correspondingOperator) {
        this.correspondingOperator = correspondingOperator;
    }

    // used by the Executive to query informations(wiring, dependencies, etc)
    public Operator getCorrespondingOperator() {
        return correspondingOperator;
    }

    public OperatorInstance[] calleeOperatorInstances;
    public List<Variadic> calleeResults = new ArrayList<>();
    public int operationState = 0;
    private Operator correspondingOperator;
    public Variadic[] constants;
}
