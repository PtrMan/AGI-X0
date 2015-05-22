using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner.Operators
{
    /**
     * 
     * each constant must be an int and the constant array contains the rewiring indices
     * there are no constraints on it
     * 
     */
    class OperatorPermutate : Operator
    {

        override public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices)
        {
            Operator.IntrospectWiringInfo wiringInfo;

            wiringInfo = new IntrospectWiringInfo();
            wiringInfo.type = IntrospectWiringInfo.EnumType.VALIDWIRING;
            wiringInfo.ouputIndices = new int[inputIndices.Length];

            // NOTE< we let the environemnt catch all indexing errors >
            // maybe this is not good

            int i;

            for( i = 0; i < inputIndices.Length; i++ )
            {
                int queriedIndex = inputIndices[i];

                System.Diagnostics.Debug.Assert(instance.constants[queriedIndex].type == Datastructures.Variadic.EnumType.INT);
                int resultIndex = instance.constants[queriedIndex].valueInt;

                wiringInfo.ouputIndices[i] = resultIndex;
            }

            return wiringInfo;
        }

        override public OperatorInstance createOperatorInstance()
        {
            return new OperatorInstance(this);
        }

        override public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances);

        override public void initializeOperatorInstance(OperatorInstance instance);


        override public ExecutionResult executeSingleStep(OperatorInstance instance);

        // gets called from the executive if a (requested execution) operator(instance) was executed
        override public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result);






        override public void operationCleanup(OperatorInstance instance);

        override public bool isScaffold()
        {
            return false;
        }

        override public string getShortName()
        {
            return "OperatorPermutate";
        }
    }
}
