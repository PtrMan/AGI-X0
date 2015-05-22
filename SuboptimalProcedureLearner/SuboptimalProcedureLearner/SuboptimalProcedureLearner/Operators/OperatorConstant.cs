using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner.Operators
{
    class OperatorConstant : Operator
    {
        override public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices)
        {
            throw new NotImplementedException();
        }

        public override OperatorInstance createOperatorInstance()
        {
            return new OperatorInstance(this);
        }
        
        override public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances)
        {
            System.Diagnostics.Debug.Assert(parameterInstances.Count == 1);

            instance.constants = new Variadic[1];
            instance.constants[0] = parameterInstances[0].constants[0];
        }
        
        override public void initializeOperatorInstance(OperatorInstance instance)
        {
        }


        override public ExecutionResult executeSingleStep(OperatorInstance instance)
        {
            return ExecutionResult.createResultInstance(instance.constants[0]);
        }

        // gets called from the executive if a (requested execution) operator(instance) was executed
        override public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result)
        {
            int x = 0;
        }




        override public void operationCleanup(OperatorInstance instance)
        {
        }

        override public bool isScaffold()
        {
            return false;
        }

        override public string getShortName()
        {
            return "OperatorConstant";
        }
    }
}
