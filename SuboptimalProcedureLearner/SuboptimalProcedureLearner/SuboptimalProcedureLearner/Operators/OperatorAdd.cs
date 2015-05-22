using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner.Operators
{
    class OperatorAdd : Operator
    {
        private enum EnumOperationState
        {
            EXECUTELEFTOPERAND,
            EXECUTERIGHTOPERAND,
            EXECUTE
        }

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
            if( parameterInstances.Count != 2 )
            {
                throw new Exception("OperatorAdd: The must be only two parameterInstances!");
            }

            instance.calleeOperatorInstances = new OperatorInstance[2];
            instance.calleeOperatorInstances[0] = parameterInstances[0];
            instance.calleeOperatorInstances[1] = parameterInstances[1];
        }
        

        override public void initializeOperatorInstance(OperatorInstance instance)
        {
            instance.calleeResults = new List<Variadic>();
        }

        override public ExecutionResult executeSingleStep(OperatorInstance instance)
        {
            EnumOperationState operationState;

            operationState = (EnumOperationState)instance.operationState;

            if( operationState == EnumOperationState.EXECUTELEFTOPERAND )
            {
                return Operator.ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[0]);
            }
            else if( operationState == EnumOperationState.EXECUTERIGHTOPERAND )
            {
                return Operator.ExecutionResult.createExecuteInstance(instance.calleeOperatorInstances[1]);
            }
            else // operationState == EnumOperationState.execute
            {
                bool resultAsFloat;
                Variadic resultVariadic;

                resultVariadic = null;

                resultAsFloat = instance.calleeResults[0].type == Variadic.EnumType.FLOAT || instance.calleeResults[1].type == Variadic.EnumType.FLOAT;

                if( resultAsFloat )
                {
                    float a, b, floatResult;

                    a = getVariadicAsFloat(instance.calleeResults[0]);
                    b = getVariadicAsFloat(instance.calleeResults[1]);

                    floatResult = a + b;

                    resultVariadic = new Variadic(Variadic.EnumType.FLOAT);
                    resultVariadic.valueFloat = floatResult;
                }
                else
                {
                    int a, b, intResult;

                    a = instance.calleeResults[0].valueInt;
                    b = instance.calleeResults[1].valueInt;

                    intResult = a + b;

                    resultVariadic = new Variadic(Variadic.EnumType.INT);
                    resultVariadic.valueInt = intResult;
                }

                return ExecutionResult.createResultInstance(resultVariadic);
            }
        }

        // gets called from the executive if a (requested execution) operator(instance) was executed
        override public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result)
        {
            instance.calleeResults.Add(result);
            instance.operationState++;
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
            return "OperatorAdd";
        }

        static private float getVariadicAsFloat(Variadic value)
        {
            if( value.type == Variadic.EnumType.FLOAT )
            {
                return value.valueFloat;
            }
            else if( value.type == Variadic.EnumType.INT )
            {
                return (float)value.valueInt;
            }
            else
            {
                throw new Exception("Can't convert varidic to float!");
            }
        }
    }
}
