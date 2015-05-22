using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    class OperatorInstance
    {
        public OperatorInstance(Operator correspondingOperator)
        {
            this.correspondingOperator = correspondingOperator;
        }

        // used by the Executive to query informations(wiring, dependencies, etc)
        public Operator getCorrespondingOperator()
        {
            return correspondingOperator;
        }

        public OperatorInstance[] calleeOperatorInstances;
        public List<Variadic> calleeResults = new List<Variadic>();

        public int operationState = 0;

        private Operator correspondingOperator;

        public Variadic[] constants;
    }
}
