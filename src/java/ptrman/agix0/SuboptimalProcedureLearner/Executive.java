package ptrman.agix0.SuboptimalProcedureLearner;

import ptrman.Datastructures.Variadic;
import ptrman.misc.Assert;

import java.util.ArrayList;
import java.util.List;

public class Executive {
    // for testing public
    public static class Fiber {
        public static class StackElement {
            public enum EnumExecutionState {
                /*
                 public enum EnumExecutionState
                 {
                    ITERATINGOVERINPUTS,

                    ITERATINGOVERINPUTS,
                    BEFOREEXECUTION,
                    EXECUTED
                 };
                                
                 public EnumExecutionState operatorExcutionState = EnumExecutionState.ITERATINGOVERINPUTS;
                 public List<int> remainingSlotIndices;
                 public int currentIterationParameterIndex = 0;
                 public int cachedParameterNumber;
                 public List<int> cachedPath; // path used by instance of OperatorPlan
                 public List<Variadic> collectedParameters;
                 */
                INITIALIZE,
                // sets parameter-instances and calls the initialisation method
                EXECUTESINGLESTEP
            }
            public EnumExecutionState operatorExcutionState = EnumExecutionState.INITIALIZE;
            public OperatorInstance operatorInstance;
            public List<Integer> cachedPath = new ArrayList<>();
        }

        // path used by instance of OperatorPlan
        public Variadic executionResult;
        public List<StackElement> operatorStack = new ArrayList<>();

        // just temporary until a better solution is found
        // add to an context? >
        public OperatorPlan operatorPlan;
        private int executionStepCounter;
        private int maxExecutionStepCounter;

        public void configure(int maxExecutionStepCounter) {
            Assert.Assert(maxExecutionStepCounter > 0, "");
            this.maxExecutionStepCounter = maxExecutionStepCounter;
        }

        public boolean isExecutionFinished() {
            return executionResult != null;
        }

        public boolean isExecutionStepCounterOverLimit() {
            return executionStepCounter > maxExecutionStepCounter;
        }

        public void resetExecution() {
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
        public void step()  {
            List<OperatorInstance> parameterOperatorInstances = new ArrayList<>();
            Operator.ExecutionResult operatorExecutionResult;
            StackElement currentStackElement = operatorStack.get(operatorStack.size() - 1);
            OperatorInstance currentOperatorInstance = currentStackElement.operatorInstance;
            AbstractOperatorBase correspondingAbstractOperatorBase = currentOperatorInstance.getCorrespondingOperator();

            if( correspondingAbstractOperatorBase.isOperator() ) {
                Operator correspondingOperator = (Operator)correspondingAbstractOperatorBase;

                switch(currentStackElement.operatorExcutionState) {
                    case INITIALIZE:
                        parameterOperatorInstances = operatorPlan.getParameterInstancesByPath(currentStackElement.cachedPath);
                        correspondingOperator.setParameterOperatorInstances(currentOperatorInstance, parameterOperatorInstances);
                        correspondingOperator.initializeOperatorInstance(currentOperatorInstance);
                        currentStackElement.operatorExcutionState = StackElement.EnumExecutionState.EXECUTESINGLESTEP;
                        break;
                    case EXECUTESINGLESTEP:
                        increaseExecutionStepCounter();
                        operatorExecutionResult = correspondingOperator.executeSingleStep(currentOperatorInstance);
                        checkExecutionResultForErrorAndThrow(correspondingOperator, operatorExecutionResult);
                        if (operatorExecutionResult.resultState == Operator.ExecutionResult.EnumResultState.EXECUTEOPERATORINSTANCE) {
                            pushNewOperator(operatorExecutionResult.instanceToExecute);
                        }
                        else if (operatorExecutionResult.resultState == Operator.ExecutionResult.EnumResultState.RESULT) {
                            propagateResultDown(operatorExecutionResult.resultValue);
                            operatorStack.remove(operatorStack.size() - 1);
                        }
                        else {
                            throw new RuntimeException("Internal Error");
                        }
                        break;

                }
            }
            else {
                Scaffold correspondingScaffold = (Scaffold)correspondingAbstractOperatorBase;

                // NOTE 30.5.2015 dunno what to write here and how to handle a scaffold

                switch(currentStackElement.operatorExcutionState) {
                    case INITIALIZE:

                        // TODO
                        currentStackElement.operatorExcutionState = StackElement.EnumExecutionState.EXECUTESINGLESTEP;
                        break;
                    case EXECUTESINGLESTEP:
                        // TODO
                        int debug = 0;

                        break;
                }

                // TODO

                // we execute the step of a scaffold
            }


        }

        // pop
        private void propagateResultDown(Variadic value) {
            Assert.Assert(operatorStack.size() >= 1, "");
            // check if it is at the bottom of the stack
            // if it is the case we return the result to the environment
            if( operatorStack.size() == 1 ) {
                executionResult = value;
                return ;
            }
             
            // TODO< check if it is at the bottom of the stack > if so, write result to result value >
            // else
            StackElement stackElementBelow = operatorStack.get(operatorStack.size() - 2);
            OperatorInstance operatorInstanceBelow = stackElementBelow.operatorInstance;

            if( operatorInstanceBelow.getCorrespondingOperator().isOperator() ) {
                Operator corespondingOperator = (Operator)operatorInstanceBelow.getCorrespondingOperator();
                corespondingOperator.feedOperatorInstanceResult(operatorInstanceBelow, value);
            }
            else if( operatorInstanceBelow.getCorrespondingOperator().isScaffold() ) {
                Scaffold corespondingScaffold = (Scaffold)operatorInstanceBelow.getCorrespondingOperator();
                corespondingScaffold.feedResult(value);
            }
            else {
                throw new RuntimeException("Internal Error");
            }
        }

        private void pushNewOperator(OperatorInstance instanceToExecute) {
            StackElement currentStackElement = operatorStack.get(operatorStack.size() - 1);
            // clone
            List<Integer> currentPath = new ArrayList<Integer>();
            currentPath.addAll(currentStackElement.cachedPath);
            currentPath.add(currentStackElement.operatorInstance.calleeResults.size());

            StackElement createdStackElement = new StackElement();
            createdStackElement.operatorInstance = instanceToExecute;
            createdStackElement.cachedPath = currentPath;
            operatorStack.add(createdStackElement);
        }

        private static void checkExecutionResultForErrorAndThrow(Operator correspondingOperator, Operator.ExecutionResult executionResult) {
            if (executionResult.resultState == Operator.ExecutionResult.EnumResultState.ERROR) {
                throw new RuntimeException("Internal Error inside Operator " + correspondingOperator.getShortName());
            }
             
        }

        // operator has returned ExecutionResult.EnumResultState.ERROR or any other unrecognized returncode
        // TODO< different exception for better outer filtering >
        private void increaseExecutionStepCounter() {
            executionStepCounter++;
        }
    }
}