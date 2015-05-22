using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    class Executive
    {
        // for testing public
        public class Fiber
        {
            public class StackElement
            {
                /*
                public enum EnumExecutionState
                {
                    ITERATINGOVERINPUTS,
                    
                    
                    
                    
                    ITERATINGOVERINPUTS,
                    BEFOREEXECUTION,
                    EXECUTED
                };

                
                

                public EnumExecutionState excutionState = EnumExecutionState.ITERATINGOVERINPUTS;
                public List<int> remainingSlotIndices;




                public int currentIterationParameterIndex = 0;
                public int cachedParameterNumber;

                public List<int> cachedPath; // path used by instance of OperatorPlan

                public List<Variadic> collectedParameters;
                */

                public enum EnumExecutionState
                {
                    INITIALIZE, // sets parameter-instances and calls the initialisation method
                    EXECUTESINGLESTEP,
                }

                public EnumExecutionState excutionState = EnumExecutionState.INITIALIZE;

                public OperatorInstance operatorInstance;

                public List<int> cachedPath; // path used by instance of OperatorPlan
            }

            public Variadic executionResult;
            public List<StackElement> operatorStack = new List<StackElement>();

            // just temporary until a better solution is found
            // add to an context? >
            public OperatorPlan operatorPlan;

            private int executionStepCounter;
            private int maxExecutionStepCounter;

            public void configure(int maxExecutionStepCounter)
            {
                System.Diagnostics.Debug.Assert(maxExecutionStepCounter > 0);

                this.maxExecutionStepCounter = maxExecutionStepCounter;
            }

            public bool isExecutionFinished()
            {
                return executionResult != null;
            }

            public bool isExecutionStepCounterOverLimit()
            {
                return executionStepCounter > maxExecutionStepCounter;
            }

            public void resetExecution()
            {
                executionResult = null;
                
            }

            /**
             * 
             * execute on demand:
             * 
             * - instance gets before a call all (attached) OperatorInstances (there is also a null instance for the nulls)
             * - operator is executed for one step
             * 
             * the result can be
             *  * execute operator (and feed later the result into the calleeOperator)
             *  * finished (with result)
             * 
             * 
             * 
             */
            public void step()
            {
                OperatorInstance currentOperatorInstance;
                Operator correspondingOperator;
                StackElement currentStackElement;
                List<OperatorInstance> parameterOperatorInstances;
                Operator.ExecutionResult operatorExecutionResult; 

                currentStackElement = operatorStack[operatorStack.Count-1];
                currentOperatorInstance = currentStackElement.operatorInstance;

                correspondingOperator = currentOperatorInstance.getCorrespondingOperator();

                switch( currentStackElement.excutionState )
                {
                    case StackElement.EnumExecutionState.INITIALIZE:
                    parameterOperatorInstances = operatorPlan.getParameterInstancesByPath(currentStackElement.cachedPath);
                    correspondingOperator.setParameterOperatorInstances(currentOperatorInstance, parameterOperatorInstances);

                    correspondingOperator.initializeOperatorInstance(currentOperatorInstance);

                    currentStackElement.excutionState = StackElement.EnumExecutionState.EXECUTESINGLESTEP;
                    break;

                    case StackElement.EnumExecutionState.EXECUTESINGLESTEP:
                    increaseExecutionStepCounter();
                    operatorExecutionResult = correspondingOperator.executeSingleStep(currentOperatorInstance);
                    checkExecutionResultForErrorAndThrow(correspondingOperator, operatorExecutionResult);

                    if( operatorExecutionResult.resultState == Operator.ExecutionResult.EnumResultState.EXECUTEOPERATORINSTANCE )
                    {
                        pushNewOperator(operatorExecutionResult.instanceToExecute);
                    }
                    else if( operatorExecutionResult.resultState == Operator.ExecutionResult.EnumResultState.RESULT )
                    {
                        propagateResultDown(operatorExecutionResult.resultValue);
                        operatorStack.RemoveAt(operatorStack.Count-1); // pop
                    }
                    else
                    {
                        throw new Exception("Internal Error");
                    }

                    break;
                }
            }

            private void propagateResultDown(Variadic value)
            {
                OperatorInstance operatorInstanceBelow;

                System.Diagnostics.Debug.Assert(operatorStack.Count >= 1);

                // check if it is at the bottom of the stack
                // if it is the case we return the result to the environment
                if( operatorStack.Count == 1 )
                {
                    executionResult = value;
                    return;
                }

                // TODO< check if it is at the bottom of the stack > if so, write result to result value >

                // else
                StackElement stackElementBelow = operatorStack[operatorStack.Count - 2];
                operatorInstanceBelow = stackElementBelow.operatorInstance;

                operatorInstanceBelow.getCorrespondingOperator().feedOperatorInstanceResult(operatorInstanceBelow, value);
            }

            private void pushNewOperator(OperatorInstance instanceToExecute)
            {
                StackElement createdStackElement;
                List<int> currentPath;
                StackElement currentStackElement;

                currentStackElement = operatorStack[operatorStack.Count-1];

                // clone
                currentPath = new List<int>();
                currentPath.AddRange(currentStackElement.cachedPath);

                currentPath.Add(currentStackElement.operatorInstance.calleeResults.Count);

                createdStackElement = new StackElement();
                createdStackElement.operatorInstance = instanceToExecute;
                createdStackElement.cachedPath = currentPath;

                operatorStack.Add(createdStackElement);
            }

            private static void checkExecutionResultForErrorAndThrow(Operator correspondingOperator, Operator.ExecutionResult executionResult)
            {
                if (executionResult.resultState == Operator.ExecutionResult.EnumResultState.ERROR)
                {
                    // operator has returned ExecutionResult.EnumResultState.ERROR or any other unrecognized returncode
                    // TODO< different exception for better outer filtering >
                    throw new Exception("Internal Error inside Operator " + correspondingOperator.getShortName());
                }
            }

            private void increaseExecutionStepCounter()
            {
                executionStepCounter++;
            }
        }
    }
}
