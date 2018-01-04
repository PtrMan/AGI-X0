module functional.execution.FunctionalInterpreter;

// translated to D from https://github.com/PtrMan/ai2/blob/4d2d983e0090d42de3deb7140b606b30490d0fb4/ai2/ProgramRepresentation/Execution/FunctionalInterpreter.cs

//using System;
//using System.Collections.Generic;

public interface IInvokeDispatcher {
    void dispatchInvokeStart(string[] path, Variadic[] parameters, InterpretationState.ScopeLevel[] calleeScopeLevels, out Datastructures.Variadic result, out EnumInvokeDispatcherCallResult callResult);
    //void dispatchInvokeSucceeding(out Datastructures.Variadic result, out EnumInvokeDispatcherCallResult callResult);
}

//namespace ProgramRepresentation.Execution



final class InterpretationState {
    static final class ScopeLevel {
        public enum EnumMatchState {
            ENTRY, // "match" body is entered the first time

            SCOPEFORVALUEWASINVOKED, // the value for comparision gets calculated with a scope
            COMPARE,                 // value is set, comparing mode
            RETURNRESULT,            // on next call return the result
        }

        public enum EnumForeachState {
            INITIAL,
            ITERATENEXT, // normal iteration over the values
            ITERATESTORE, // store result of the call in the result array
            SCOPEFORARRAYWASINVOKED,
        }

        public enum EnumPassState {
            INITIAL,
            RETURNRESULT,
        }

        public enum EnumSimpleMathType {
            INT,
            FLOAT,
        }

        public enum EnumSimpleMathState {
            INITIAL,
            STOREINITIAL, // first operand in result, transfer to result of operation
            SUCCEEDINGTOCALL, // setup for the next call to the operand
            SUCCEEDINGCALCULATE, // do operation on result of operand
        }

        public enum EnumInvokeState {
            INITIAL, // initial and first call to invoke
            RESOLVEPARAMETERS, // parameters are being resolved (keeps in this mode as long as scoped parameters are remaining)
            PROGRAMINVOKED, // invoke has called the dag internal program allready
            SCOPEFORPARAMETERWASINVOKED,
            INVOKE,
        }

        public enum EnumFoldState {
            INITIAL, // initial and first call to fold
            SUCCEEDING, // succeeding calls
            SCOPEFORARRAYINVOKED, // scope for the iteration array was invoked, on the next entry we have to catch the result value and set it as the array
        }

        public enum EnumSetVariablesState {
            INITIAL,
            RESOLVEVARIABLES, // in this state the interpreter tries to resolve the variables
            RESOLVEVARIABLESSCOPE, // still in resolve state, a scope was invoked to calculate the value of a variable
            INVOKEBODY,
            RETURNVALUE, // body was invoked
        }

        final class Variable {
            string name;
            Variadic value;
        }

        public bool isTerminator = false;

        public int dagElementIndex; /** \brief points at the dag element which got executed or which introduced a variable */
        
        // TODO< dictionary? >
        public Variable[] variables;


        public Datastructures.Variadic calleeResult; /** \brief contains the result of a called function */

        // used by fold
        // -1 means that fold is invoked the first time
        public int foldIterationIndex = -1;
        public Datastructures.Variadic foldCurrentValue; /** \brief is the current value of a fold operation */
        public Variadic[] foldInputArray; /** \brief array with the values for folding, can be calculated or can point at an array inside a variadic */
        public EnumFoldState foldState = EnumFoldState.INITIAL;

        public EnumMatchState matchState = EnumMatchState.ENTRY;
        public Datastructures.Variadic matchValue; /** \brief value for matching */
        public int matchIndexInCompare = 0; /** \brief index starting from 0 of the current compared pattern */

        public EnumForeachState foreachState = EnumForeachState.INITIAL;
        public Variadic[] foreachArray; /** \brief array for iteration */
        public Variadic[] foreachResultArray;
        public int foreachIndexInArray = 0;

        public EnumPassState passState = EnumPassState.INITIAL;

        public int arrayIndex = 0;
        public Variadic[] arrayResultArray;

        public EnumSimpleMathState simpleMathState = EnumSimpleMathState.INITIAL;
        public EnumSimpleMathType simpleMathType = EnumSimpleMathType.INT; /** \brief used to keep the result as a integer and cast it to a float only if necessary */
        public float simpleMathResult;
        public int simpleMathIndex;

        public EnumInvokeState invokeState = EnumInvokeState.INITIAL;
        public int invokeResolveParametersIndex = 0;
        public Variadic[] invokeResolvedParameters;

        public EnumSetVariablesState setVariablesState = EnumSetVariablesState.INITIAL;
        public int setVariablesVariableIndex = 0;
    }

    static final class InvokableProgram {
        public string[] path;
        public int dagIndex = -1;

        // variables are set with the parameters of the invoke
        public string[] variableNames; /** \brief names of the variables for the invoke call */
    }

    public ScopeLevel[] scopeLevels;

    public int currentScopeLevel;

    public Datastructures.Variadic result;

    // TODO< dict? >
    public InvokableProgram[] invokablePrograms;
}



// TODO< on returning of a value search for the highest scope which want to "catch" the value, if no one catches, usefull for comment instructions and pass instructions >
// TODO< return the value as a result if only a value is given and not a scope ??? >
//
final class FunctionalInterpreter {
    enum EnumInvokeDispatcherCallResult {
        DONE,
        SUCCEEDINGINTERNAL, // the invoke needs more succeeding calls, interpreter is not involved of interpreting the "inner" functions/scopes/etc
        PATHINVALID,
    }

    // dispatcher for all invokations of external functionality
    IInvokeDispatcher invokeDispatcher;

    private enum EnumOverwriteExistingVariable {
        YES,
        NO
    }

    public void interpreteStep(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state, out bool terminatorReached) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length-1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        terminatorReached = isTopScopeATerminator(state);
        if( terminatorReached ) {
            return;
        }

        if( currentDagElement.content.type == DagElementData.EnumType.FSCOPE ) {
            if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.INVOKE ) {
                executeInvoke(dag, state);
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.FOLD ) {
                executeFold(dag, state);
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.MATCH ) {
                executeMatch(dag, state);
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.FOREACH ) {
                executeForeach(dag, state);
            }
            else if (currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.PASS ) {
                executePass(dag, state);
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.FSET ) {
                executeSetVariables(dag, state);
            }
            else if(
                currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.ADD ||
                currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.SUB ||
                currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.MUL ||
                currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.DIV
            ) {
                executeSimpleMathOperation(dag, state, (Parser.Functional.ScopeParseTreeElement.EnumType)currentDagElement.content.valueInt);
            }
            else {
                throw new Exception("Internal Error!");
            }

            return;
        }
        else if(
            currentDagElement.content.type == DagElementData.EnumType.CONSTINT ||
            currentDagElement.content.type == DagElementData.EnumType.CONSTFLOAT ||
            currentDagElement.content.type == DagElementData.EnumType.CONSTBOOL
        ) {
            returnValue(currentDagElement.content, state);

            return;
        }
        else if( currentDagElement.content.type == DagElementData.EnumType.FARRAY ) {
            executeReturnArray(dag, state);
        }
        else if( currentDagElement.content.type == DagElementData.EnumType.IDENTIFIERNAME ) {
            executeReturnVariable(dag, state);
        }
        else {
            throw new Exception("Tried to execute unexecutable instrution!");
        }
    }

    private static bool isTopScopeATerminator(InterpretationState state) {
        assert(state.scopeLevels.length >= 1);
        return state.scopeLevels[state.scopeLevels.length - 1].isTerminator;
    }

    /**
     * 
     * returns the value of the dag element as the result
     * 
     */
    private static void returnValue(ProgramRepresentation.DagElementData dagElementData, InterpretationState state) {
        Datastructures.Variadic resultValue;

        if( dagElementData.type == DagElementData.EnumType.CONSTINT ) {
            resultValue = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            resultValue.valueInt = dagElementData.valueInt;
        }
        else if( dagElementData.type == DagElementData.EnumType.CONSTFLOAT ) {
            resultValue = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
            resultValue.valueFloat = dagElementData.valueFloat;
        }
        else if( dagElementData.type == DagElementData.EnumType.CONSTBOOL ) {
            resultValue = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
            resultValue.valueBool = dagElementData.valueBool;
        }
        else {
            // also TODO< handling for other types >
            throw new Exception("Internal Error");
        }

        returnResult(state, resultValue);
        removeTopScope(state);
    }


    private void executeInvoke(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        Datastructures.Variadic invokeResult;
        EnumInvokeDispatcherCallResult callResult;

        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;
        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length-1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( topScope.invokeState == InterpretationState.ScopeLevel.EnumInvokeState.INITIAL ) {
            topScope.invokeState = InterpretationState.ScopeLevel.EnumInvokeState.RESOLVEPARAMETERS;
            topScope.invokeResolvedParameters = new Variadic[]();

            return;
        }
        else if( topScope.invokeState == InterpretationState.ScopeLevel.EnumInvokeState.RESOLVEPARAMETERS ) {
            int dagIndexOfArrayWithParameters;
            int[] dagIndicesOfArrayContentForParameters;

            // retrieve and try to resolve remaining parameters

            dagIndexOfArrayWithParameters = currentDagElement.childIndices[1];
            dagIndicesOfArrayContentForParameters = dag.elements[dagIndexOfArrayWithParameters].childIndices;

            for(;;) {
                int dagIndexOfParameter;

                // check if we are done with the resolving
                System.Diagnostics.Debug.Assert(topScope.invokeResolveParametersIndex <= dagIndicesOfArrayContentForParameters.length);
                if( topScope.invokeResolveParametersIndex == dagIndicesOfArrayContentForParameters.length ) {
                    topScope.invokeState = InterpretationState.ScopeLevel.EnumInvokeState.INVOKE;

                    return;
                }

                // check if the current parameter is a scope
                // if it is the case, we open the scope and transfer control
                // if its not the case we can try to resolve the parameter as usual and continue
                dagIndexOfParameter = dagIndicesOfArrayContentForParameters[topScope.invokeResolveParametersIndex];
                
                if( dag.elements[dagIndexOfParameter].content.type == DagElementData.EnumType.FSCOPE ) {
                    InterpretationState.ScopeLevel createdScopeLevel;

                    topScope.invokeResolveParametersIndex++;
                    topScope.invokeState = InterpretationState.ScopeLevel.EnumInvokeState.SCOPEFORPARAMETERWASINVOKED;

                    // build new scope
                    createdScopeLevel = new InterpretationState.ScopeLevel();
                    state.scopeLevels.Add(createdScopeLevel);

                    createdScopeLevel.dagElementIndex = dagIndexOfParameter;

                    // do this so the next step executes the scope
                    state.currentScopeLevel++;

                    return;
                }
                else {
                    Datastructures.Variadic currentResolvedParameter;

                    currentResolvedParameter = resolveVariableAndConstant(dag, dagIndexOfParameter, state);
                    topScope.invokeResolvedParameters.Add(currentResolvedParameter);
                    topScope.invokeResolveParametersIndex++;
                }
            }
            
        }
        else if( topScope.invokeState == InterpretationState.ScopeLevel.EnumInvokeState.SCOPEFORPARAMETERWASINVOKED ) {
            Datastructures.Variadic parameterFromScope;
            
            // the scope for an parameter was invoked and the result is on the stack

            // * get the result from the stack
            // * add it to the parameters
            // * go back in the state

            // NOTE< we just move the reference >
            parameterFromScope = topScope.calleeResult;
            topScope.invokeResolvedParameters.Add(parameterFromScope);

            topScope.invokeState = InterpretationState.ScopeLevel.EnumInvokeState.RESOLVEPARAMETERS;

            return;
        }
        else if( topScope.invokeState == InterpretationState.ScopeLevel.EnumInvokeState.INVOKE ) {
            int dagIndexOfArrayWithPath;
            int[] dagIndicesOfArrayContentForPath;
            string[] pathOfInvokedProgram;

            bool isInterpretableInvoke;
            int dagIndexOfInvokedScope;
            string[] invokeVariableNames;


            // build the path of the invoked program
            dagIndexOfArrayWithPath = currentDagElement.childIndices[0];

            if (dag.elements[dagIndexOfArrayWithPath].content.type != DagElementData.EnumType.FARRAY) {
                throw new Exception("Invoke parameter [0] was not an Array as expected!");
            }

            // grab the indices of the array
            dagIndicesOfArrayContentForPath = dag.elements[dagIndexOfArrayWithPath].childIndices;
            // and put them into the function which executes building stuff for the path for the invoke
            pathOfInvokedProgram = getPathOfInvokeScopeInstruction(dag, dagIndicesOfArrayContentForPath, state);


            // try to lookup the path inside the list with all interpretable programs (which can be invoked)
            invokeVariableNames = new string[]();
            lookupInvokePath(state, pathOfInvokedProgram, out isInterpretableInvoke, out dagIndexOfInvokedScope, invokeVariableNames);

            if( isInterpretableInvoke ) {
                InterpretationState.ScopeLevel createdScopeLevel;
                int variableI;

                // set state
                topScope.invokeState = InterpretationState.ScopeLevel.EnumInvokeState.PROGRAMINVOKED;

                // check if number of parameter is correct
                enforce(topScope.invokeResolvedParameters.length == invokeVariableNames.length);

                // build new scope
                createdScopeLevel = new InterpretationState.ScopeLevel();
                state.scopeLevels.Add(createdScopeLevel);

                createdScopeLevel.dagElementIndex = dagIndexOfInvokedScope;

                // set variables
                for (variableI = 0; variableI < invokeVariableNames.length; variableI++) {
                    InterpretationState.ScopeLevel.Variable createdVariable;

                    createdVariable = new InterpretationState.ScopeLevel.Variable();
                    createdVariable.name = invokeVariableNames[variableI];
                    createdVariable.value = topScope.invokeResolvedParameters[variableI];

                    createdScopeLevel.variables.Add(createdVariable);
                }

                // do this so the next step executes the scope
                state.currentScopeLevel++;

                return;
            }
            else {
                // dispatch invoke
                invokeDispatcher.dispatchInvokeStart(pathOfInvokedProgram, topScope.invokeResolvedParameters, state.scopeLevels, out invokeResult, out callResult);


                // TODO< maybe logic for searching of callee >
                System.Diagnostics.Debug.Assert(state.currentScopeLevel > 0);

                // if we are done
                // return result to caller (in state stack)
                // if the call failed we return false
                if (callResult == EnumInvokeDispatcherCallResult.DONE) {
                    state.scopeLevels[state.currentScopeLevel - 1].calleeResult = invokeResult;

                    // remove scope of call
                    state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);
                    state.currentScopeLevel--;

                    return;
                }
                else if (callResult == EnumInvokeDispatcherCallResult.PATHINVALID) {
                    state.scopeLevels[state.currentScopeLevel - 1].calleeResult = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
                    state.scopeLevels[state.currentScopeLevel - 1].calleeResult.valueBool = false;

                    // remove scope of call
                    state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);
                    state.currentScopeLevel--;

                    return;
                }
                else if (callResult == EnumInvokeDispatcherCallResult.SUCCEEDINGINTERNAL) {
                    System.Diagnostics.Debug.Assert(false, "SUCCEEDINGINTERNAL not supported!");
                    throw new Exception("Internal Error!");
                }

                return;
            }
        }
        else if( topScope.invokeState == InterpretationState.ScopeLevel.EnumInvokeState.PROGRAMINVOKED ) {
            // TODO< maybe logic for searching of callee >
            assert(state.currentScopeLevel > 0);

            state.scopeLevels[state.currentScopeLevel - 1].calleeResult = state.scopeLevels[state.currentScopeLevel].calleeResult;

            // remove scope of call
            state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);
            state.currentScopeLevel--;

            return;
        }
        else {
            throw new Exception("Internal Error!");
        }

        throw new Exception("unreachable!");
    }

    private void executeFold(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;
        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( topScope.foldState == InterpretationState.ScopeLevel.EnumFoldState.INITIAL ) {
            int dagIndexOfArrayWithValuesOrScope;
            DagElementData.EnumType arrayWithValuesOrScopeType;

            dagIndexOfArrayWithValuesOrScope = currentDagElement.childIndices[1];
            arrayWithValuesOrScopeType = dag.elements[dagIndexOfArrayWithValuesOrScope].content.type;

            if (arrayWithValuesOrScopeType == DagElementData.EnumType.FARRAY) {
                int[] dagIndicesOfVariablesOrConstantsOfArray = dag.elements[dagIndexOfArrayWithValuesOrScope].childIndices;
                topScope.foldInputArray = resolveVariablesAndConstants(dag, dagIndicesOfVariablesOrConstantsOfArray, state);

                topScope.foldState = InterpretationState.ScopeLevel.EnumFoldState.SUCCEEDING;

                // not return
            }
            else if( arrayWithValuesOrScopeType == DagElementData.EnumType.FSCOPE ) {
                int dagIndexOfCalled;
                InterpretationState.ScopeLevel createdScopeLevel;

                topScope.foldState = InterpretationState.ScopeLevel.EnumFoldState.SCOPEFORARRAYINVOKED;

                // open scope ...
                createdScopeLevel = new InterpretationState.ScopeLevel();
                state.scopeLevels.Add(createdScopeLevel);

                dagIndexOfCalled = dagIndexOfArrayWithValuesOrScope;
                createdScopeLevel.dagElementIndex = dagIndexOfCalled;

                // do this so the next step executes the scope
                state.currentScopeLevel++;

                return;
            }
            else {
                throw new Exception("Internal Error : Invalid not handable parameter for fold");
            }
        }
        else if( topScope.foldState == InterpretationState.ScopeLevel.EnumFoldState.SCOPEFORARRAYINVOKED ) {
            // check if result is an array
            if( topScope.calleeResult.type != Datastructures.Variadic.EnumType.ARRAY ) {
                throw new Exception("fold second parameter is not an array!");
            }

            // set array as working array
            topScope.foldInputArray = topScope.calleeResult.valueArray;

            topScope.foldState = InterpretationState.ScopeLevel.EnumFoldState.SUCCEEDING;

            // no return
        }

        assert(topScope.foldState == InterpretationState.ScopeLevel.EnumFoldState.SUCCEEDING);

        if (topScope.foldIterationIndex == -1) {
            // first iteration
            if (topScope.foldInputArray.length == 0) {
                // no value in array, return false

                state.scopeLevels[state.currentScopeLevel - 1].calleeResult = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
                state.scopeLevels[state.currentScopeLevel - 1].calleeResult.valueBool = false;

                // remove scope of fold
                state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);

                return;
            }
            else if (topScope.foldInputArray.length == 1) {
                // one value in array, return it
                Datastructures.Variadic result;

                result = topScope.foldInputArray[0];
                returnResult(state, result);

                // remove scope of fold
                //BUG? state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);
                // TODO< investigate >
                removeTopScope(state);

                return;
            }
            else {
                Datastructures.Variadic firstValue;
                Datastructures.Variadic secondValue;
                int dagIndexOfCalled;
                InterpretationState.ScopeLevel createdScopeLevel;

                // two or more values in the array

                // * fetch values
                // * check if called is a dag-scope
                // * open scope and push the dag-scope for execution 
                // * set variables

                // fetch values
                firstValue = topScope.foldInputArray[0];
                secondValue = topScope.foldInputArray[1];

                // check
                dagIndexOfCalled = currentDagElement.childIndices[0];
                if (dag.elements[dagIndexOfCalled].content.type != DagElementData.EnumType.FSCOPE) {
                    throw new Exception("fold: first parameter must be a scope!");
                }

                // open scope ...
                createdScopeLevel = new InterpretationState.ScopeLevel();
                state.scopeLevels.Add(createdScopeLevel);

                createdScopeLevel.dagElementIndex = dagIndexOfCalled;

                // (set variables)
                createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
                createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
                createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
                createdScopeLevel.variables[0].name = "accu";
                createdScopeLevel.variables[0].value = firstValue;
                createdScopeLevel.variables[1].name = "other";
                createdScopeLevel.variables[1].value = secondValue;
                createdScopeLevel.variables[2].name = "index";
                createdScopeLevel.variables[2].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                createdScopeLevel.variables[2].value.valueInt = 0;

                topScope.foldIterationIndex = 1;

                // do this so the next step executes the scope
                state.currentScopeLevel++;

                return;
            }
        }
        else {
            // following iterations

            // * check if array is walked
            //   if not ->
            //   * fetch value
            //   * evaluate value
            //   * open scope and push the dag-scope for execution 
            //   * set variables
            //   if ->
            //   * return accumulated value

            int valueArrayListLength;
            Datastructures.Variadic evaluatedValue;
            InterpretationState.ScopeLevel createdScopeLevel;
            int dagIndexOfCalled;

            valueArrayListLength = topScope.foldInputArray.length;

            dagIndexOfCalled = currentDagElement.childIndices[0];

            // check if array is walked
            System.Diagnostics.Debug.Assert(topScope.foldIterationIndex <= valueArrayListLength);
            if (topScope.foldIterationIndex == valueArrayListLength) {
                // it is walked

                // return the result
                returnResult(state, topScope.calleeResult);
                

                // remove scope of call
                removeTopScope(state);

                return;
            }
            // else we are here


            // fetch value
            // evaluate
            evaluatedValue = topScope.foldInputArray[topScope.foldIterationIndex];

            // open scope and push the dag-scope for execution 
            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            createdScopeLevel.dagElementIndex = dagIndexOfCalled;

            // set variables
            createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
            createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
            createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
            createdScopeLevel.variables[0].name = "accu";
            createdScopeLevel.variables[0].value = topScope.calleeResult;
            createdScopeLevel.variables[1].name = "other";
            createdScopeLevel.variables[1].value = evaluatedValue;
            createdScopeLevel.variables[2].name = "index";
            createdScopeLevel.variables[2].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            createdScopeLevel.variables[2].value.valueInt = topScope.foldIterationIndex;


            // misc

            // increment the index for the next call
            topScope.foldIterationIndex++;

            // do this so the next step executes the scope
            state.currentScopeLevel++;

            return;
        }
    }

    private void executeMatch(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( topScope.matchState == InterpretationState.ScopeLevel.EnumMatchState.ENTRY ) {
            int dagIndexOfValueOrScope;
            bool valueIsPresentAsConstantOrVariable;
            Datastructures.Variadic valueToCompare;

            // check number of childrens, must be odd
            if( (currentDagElement.childIndices.length % 2) != 1 ) {
                throw new Exception("match was executed with even number of parameters!");
            }

            dagIndexOfValueOrScope = currentDagElement.childIndices[0];

            valueIsPresentAsConstantOrVariable = isConstantOrVariable(dag, dagIndexOfValueOrScope);
            if( valueIsPresentAsConstantOrVariable ) {
                valueToCompare = resolveVariableAndConstant(dag, dagIndexOfValueOrScope, state);

                // set value to compare of "match"
                topScope.matchValue = valueToCompare;

                topScope.matchState = InterpretationState.ScopeLevel.EnumMatchState.COMPARE;

                return;
            }
            else {
                InterpretationState.ScopeLevel createdScopeLevel;

                // * setup current scope to "catch" return value
                // * open scope and push the dag-scope for execution

                // TODO< setup current scope to "catch" return value >

                // open scope and push the dag-scope for execution

                createdScopeLevel = new InterpretationState.ScopeLevel();
                state.scopeLevels.Add(createdScopeLevel);

                createdScopeLevel.dagElementIndex = dagIndexOfValueOrScope;

                // misc

                // do this so the next step executes the scope
                state.currentScopeLevel++;


                topScope.matchState = InterpretationState.ScopeLevel.EnumMatchState.SCOPEFORVALUEWASINVOKED;

                return;
            }
        }
        else if( topScope.matchState == InterpretationState.ScopeLevel.EnumMatchState.RETURNRESULT ) {
            // TODO< propagate result value >

            returnResult(state, topScope.calleeResult);

            // remove scope of call
            removeTopScope(state);

            return;
        }
        else if (topScope.matchState == InterpretationState.ScopeLevel.EnumMatchState.SCOPEFORVALUEWASINVOKED) {
            // the scope for the calculation of the comparison value was executed
            // now we need to store the value and compare

            topScope.matchValue = topScope.calleeResult;

            topScope.matchState = InterpretationState.ScopeLevel.EnumMatchState.COMPARE;

            // no return
        }
        else if( topScope.matchState == InterpretationState.ScopeLevel.EnumMatchState.COMPARE ) {
            // fall through
        }
        else {
            System.Diagnostics.Debug.Assert(false);
            throw new Exception("Internal Error!");
        }

        // if we are here we can either compare or open a scope to calculate the result of the "match"
        if( topScope.matchState == InterpretationState.ScopeLevel.EnumMatchState.COMPARE ) {
            int dagIndexOfComparePattern;
            int numberOfMatchArguments;
            int numberOfMatches;

            // check for end and return false if it reached the end without finding a match

            numberOfMatchArguments = currentDagElement.childIndices.length;
            numberOfMatches = (numberOfMatchArguments-1)/2;
            System.Diagnostics.Debug.Assert(topScope.matchIndexInCompare <= numberOfMatches);
            if( topScope.matchIndexInCompare == numberOfMatches ) {
                Datastructures.Variadic resultValue;
                // end reached without a match
                // return false

                resultValue = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
                resultValue.valueBool = false;
                returnResult(state, resultValue);

                // remove scope of call
                removeTopScope(state);

                return;
            }

            // we are here if there are enougth elements

            dagIndexOfComparePattern = currentDagElement.childIndices[1 + topScope.matchIndexInCompare * 2]; // can be unsafe

            if(
                dag.elements[dagIndexOfComparePattern].content.type == DagElementData.EnumType.CONSTINT ||
                dag.elements[dagIndexOfComparePattern].content.type == DagElementData.EnumType.CONSTBOOL
            ) {
                bool isConvertableTo;
                Datastructures.Variadic convertedPattern;
                bool doesMatch;

                // try to convert

                isConvertableTo = isDagElementTypeTheSameOrConvertableToType(dag.elements[dagIndexOfComparePattern], topScope.matchValue.type);
                if( !isConvertableTo ) {
                    throw new Exception("match: Type of compared value is not convertable or equal to pattern type!");
                }

                convertedPattern = convertTo(dag.elements[dagIndexOfComparePattern], topScope.matchValue.type);

                // match
                doesMatch = doesVariableMatchPattern(topScope.matchValue, convertedPattern);

                if( doesMatch ) {
                    InterpretationState.ScopeLevel createdScopeLevel;
                    int dagIndexOfExecutedBranch;

                    // set value so next call terminates and returns the result
                    topScope.matchState = InterpretationState.ScopeLevel.EnumMatchState.RETURNRESULT;

                    // execution of scope if it does match on the next step

                    createdScopeLevel = new InterpretationState.ScopeLevel();
                    state.scopeLevels.Add(createdScopeLevel);

                    dagIndexOfExecutedBranch = currentDagElement.childIndices[1 + topScope.matchIndexInCompare * 2 + 1]; // can be unsafe
                    createdScopeLevel.dagElementIndex = dagIndexOfExecutedBranch;

                    // misc

                    // do this so the next step executes the scope
                    state.currentScopeLevel++;

                    return;
                }
                else {
                    // continue in the next step with the next pattern
                    
                    topScope.matchIndexInCompare++;

                    return;
                }
            }
            else {
                // also TODO

                System.Diagnostics.Debug.Assert(false);
                throw new Exception("Internal Error!");
            }
        }
        else {
            // also TODO

            System.Diagnostics.Debug.Assert(false);
            throw new Exception("Internal Error!");
        }
    }

    private void executeForeach(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;
        
        int dagIndexForIteratorScope;

        
        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( topScope.foreachState == InterpretationState.ScopeLevel.EnumForeachState.INITIAL ) {
            // dag index for the array to iterate over or for a variable which holds the array
            int dagIndexForArgument;

            // check if number of parameters is two
            if( currentDagElement.childIndices.length != 2 ) {
                throw new Exception("foreach: must take two arguments!");
            }

            dagIndexForIteratorScope = currentDagElement.childIndices[0];
            dagIndexForArgument = currentDagElement.childIndices[1];

            // check if first parameter is a scope
            if( dag.elements[dagIndexForIteratorScope].content.type != DagElementData.EnumType.FSCOPE ) {
                throw new Exception("first parameter must be a scope!");
            }

            // setup
            topScope.foreachResultArray = new Variadic[]();

            // check if second parameter is
            // * a array
            // * a scope
            // * a variable
            // TODO< if not, check if it is a scope, call the scope, check if it is a array >
            //     < if it is a variable, retrive the variable and check if it is an array, if not, throw something
            if( dag.elements[dagIndexForArgument].content.type == DagElementData.EnumType.FARRAY ) {
                List<int> dagIndicesOfContentOfArray;

                // retrive and resolve array

                dagIndicesOfContentOfArray = dag.elements[dagIndexForArgument].childIndices;
                topScope.foreachArray = resolveVariablesAndConstants(dag, dagIndicesOfContentOfArray, state);

                topScope.foreachState = InterpretationState.ScopeLevel.EnumForeachState.ITERATENEXT;

                // no return
            }
            else if( dag.elements[dagIndexForArgument].content.type == DagElementData.EnumType.FSCOPE ) {
                InterpretationState.ScopeLevel createdScopeLevel;
                int dagIndexOfCalled;

                // it is a scope
                topScope.foreachState = InterpretationState.ScopeLevel.EnumForeachState.SCOPEFORARRAYWASINVOKED;

                // setup the scope
                createdScopeLevel = new InterpretationState.ScopeLevel();
                state.scopeLevels.Add(createdScopeLevel);

                dagIndexOfCalled = dagIndexForArgument;
                createdScopeLevel.dagElementIndex = dagIndexOfCalled;

                // misc

                // do this so the next step executes the scope
                state.currentScopeLevel++;

                return;
            }
            else if( dag.elements[dagIndexForArgument].content.type == DagElementData.EnumType.IDENTIFIERNAME ) {
                // it is a variable

                System.Diagnostics.Debug.Assert(false, "TODO");
            }
            else {
                throw new Exception("foreach: second parameter must be a array or a scope or a variable which holds an array!");
            }
        }
        else if( topScope.foreachState == InterpretationState.ScopeLevel.EnumForeachState.SCOPEFORARRAYWASINVOKED ) {

            // scope for the array to iterate over was invoked

            // * check if result from scope is an array
            // * set array
            // * set state so it iterates over the array as usual

            if( topScope.calleeResult.type != Datastructures.Variadic.EnumType.ARRAY ) {
                throw new Exception("foreach  result from scope for variable must be an array");
            }

            topScope.foreachArray = topScope.calleeResult.valueArray;

            topScope.foreachState = InterpretationState.ScopeLevel.EnumForeachState.ITERATENEXT;

            // no return
        }


        // when w are here the state can either be
        // * ITERATENEXT
        //   we need to try to fetch the next item and feed it into the scope
        // * ITERATESTORE
        //   we need to retrive the result of the called scope and append it to the result array

        if( topScope.foreachState == InterpretationState.ScopeLevel.EnumForeachState.ITERATENEXT ) {
            Datastructures.Variadic currentIterationValue;
            InterpretationState.ScopeLevel createdScopeLevel;
            int dagIndexOfCalled;

            dagIndexForIteratorScope = currentDagElement.childIndices[0];

            // * check if we are at the end of the array
            //   if so -> return result array
            // * fetch value
            // * build scope
            // * set variable "element"

            // check if we are the end
            System.Diagnostics.Debug.Assert(topScope.foreachIndexInArray <= topScope.foreachArray.length);
            if (topScope.foreachIndexInArray == topScope.foreachArray.length) {
                Datastructures.Variadic resultValue;

                resultValue = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
                resultValue.valueArray = topScope.foreachResultArray;

                returnResult(state, resultValue);

                // remove scope of call
                removeTopScope(state);

                return;
            }

            // fetch value
            currentIterationValue = topScope.foreachArray[topScope.foreachIndexInArray];
            

            // build scope

            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            dagIndexOfCalled = dagIndexForIteratorScope;
            createdScopeLevel.dagElementIndex = dagIndexOfCalled;

            // set variables
            createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
            createdScopeLevel.variables.Add(new InterpretationState.ScopeLevel.Variable());
            createdScopeLevel.variables[0].name = "element";
            createdScopeLevel.variables[0].value = currentIterationValue;
            createdScopeLevel.variables[1].name = "index";
            createdScopeLevel.variables[1].value = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            createdScopeLevel.variables[1].value.valueInt = topScope.foreachIndexInArray;
            
            // misc
            // do this so the next step executes the scope
            state.currentScopeLevel++;

            // misc
            topScope.foreachIndexInArray++;

            // state must be different
            topScope.foreachState = InterpretationState.ScopeLevel.EnumForeachState.ITERATESTORE;

            return;
        }
        else if( topScope.foreachState == InterpretationState.ScopeLevel.EnumForeachState.ITERATESTORE ) {
            topScope.foreachResultArray.Add(topScope.calleeResult.deepCopy());

            topScope.foreachState = InterpretationState.ScopeLevel.EnumForeachState.ITERATENEXT;

            return;
        }
        else {
            throw new Exception("Internal Error!");
        }

        System.Diagnostics.Debug.Assert(false);
        throw new Exception("unreachable");
    }

    private void executePass(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( currentDagElement.childIndices.length != 1 ) {
            throw new Exception("pass invalid number of arguments!");
        }

        if( topScope.passState == InterpretationState.ScopeLevel.EnumPassState.INITIAL ) {
            InterpretationState.ScopeLevel createdScopeLevel;
            int dagIndexOfCalled;

            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            dagIndexOfCalled = currentDagElement.childIndices[0];
            createdScopeLevel.dagElementIndex = dagIndexOfCalled;

            // misc
            // do this so the next step executes the scope
            state.currentScopeLevel++;

            topScope.passState = InterpretationState.ScopeLevel.EnumPassState.RETURNRESULT;
        }
        else if( topScope.passState == InterpretationState.ScopeLevel.EnumPassState.RETURNRESULT ) {
            returnResult(state, topScope.calleeResult);

            // remove scope of call
            removeTopScope(state);
        }
        else {
            throw new Exception("Internal Error");
        }
    }

    private void executeReturnArray(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;

        InterpretationState.ScopeLevel createdScopeLevel;
        int dagIndexOfCalled;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];


        // if it is the first call -> create result array
        // else -> store result
        if (topScope.arrayIndex == 0) {
            topScope.arrayResultArray = new Variadic[]();
        }
        else {
            topScope.arrayResultArray ~= topScope.calleeResult;
        }


        System.Diagnostics.Debug.Assert(topScope.arrayIndex <= currentDagElement.childIndices.length);
        if( topScope.arrayIndex == currentDagElement.childIndices.length ) {
            Datastructures.Variadic resultVariadic;

            resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
            resultVariadic.valueArray = topScope.arrayResultArray;

            returnResult(state, resultVariadic);

            // remove scope of call
            removeTopScope(state);

            return;
        }
        // else we are here


        // build scope and setup

        createdScopeLevel = new InterpretationState.ScopeLevel();
        state.scopeLevels.Add(createdScopeLevel);

        dagIndexOfCalled = currentDagElement.childIndices[topScope.arrayIndex];
        createdScopeLevel.dagElementIndex = dagIndexOfCalled;


        // misc
        // do this so the next step executes the scope
        state.currentScopeLevel++;

        topScope.arrayIndex++;

        return;
    }

    static private void executeReturnVariable(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;
        bool variableWasResolved;
        Datastructures.Variadic resolvedVariable;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( currentDagElement.content.type != DagElementData.EnumType.IDENTIFIERNAME ) {
            // type must be an identifier
            throw new Exception("Internal Error");
        }

        // look the identifier up
        resolvedVariable = resolveVariable(currentDagElement.content.identifier, state, out variableWasResolved);
        if( !variableWasResolved ) {
            throw new Exception("Variable \"" + currentDagElement.content.identifier + "\" could not be resolved.");
        }

        // return variable
        returnResult(state, resolvedVariable);

        // remove scope of call
        removeTopScope(state);

        return;
    }

    private void executeSimpleMathOperation(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state, Parser.Functional.ScopeParseTreeElement.EnumType type) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( currentDagElement.childIndices.length == 0 ) {
            throw new Exception("mathematical operation : It is invalid that it has no operands!");
        }

        if( topScope.simpleMathState == InterpretationState.ScopeLevel.EnumSimpleMathState.INITIAL ) {
            InterpretationState.ScopeLevel createdScopeLevel;
            int dagIndexOfCalled;

            // invoke first value

            topScope.simpleMathState = InterpretationState.ScopeLevel.EnumSimpleMathState.STOREINITIAL;

            // build scope and setup

            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            dagIndexOfCalled = currentDagElement.childIndices[0];
            createdScopeLevel.dagElementIndex = dagIndexOfCalled;


            // misc
            // do this so the next step executes the scope
            state.currentScopeLevel++;

            return;
        }
        else if( topScope.simpleMathState == InterpretationState.ScopeLevel.EnumSimpleMathState.STOREINITIAL ) {
            // we have to store the initial value (is in calleeResult)

            if( topScope.calleeResult.type == Datastructures.Variadic.EnumType.INT ) {
                topScope.simpleMathResult = (float)topScope.calleeResult.valueInt;
                topScope.simpleMathType = InterpretationState.ScopeLevel.EnumSimpleMathType.INT;
            }
            else if( topScope.calleeResult.type == Datastructures.Variadic.EnumType.FLOAT ) {
                topScope.simpleMathResult = topScope.calleeResult.valueFloat;
                topScope.simpleMathType = InterpretationState.ScopeLevel.EnumSimpleMathType.FLOAT;
            }
            else {
                throw new Exception("arithmetic  value at [0] is not a number!");
            }

            topScope.simpleMathState = InterpretationState.ScopeLevel.EnumSimpleMathState.SUCCEEDINGTOCALL;
            topScope.simpleMathIndex = 1;
        }
        else if( topScope.simpleMathState == InterpretationState.ScopeLevel.EnumSimpleMathState.SUCCEEDINGTOCALL ) {
            bool isAtEndOfChildrens;

            InterpretationState.ScopeLevel createdScopeLevel;
            int dagIndexOfCalled;

            System.Diagnostics.Debug.Assert(topScope.simpleMathIndex <= currentDagElement.childIndices.length);
            isAtEndOfChildrens = topScope.simpleMathIndex == currentDagElement.childIndices.length;

            if( isAtEndOfChildrens ) {
                Datastructures.Variadic resultVariadic;

                if( topScope.simpleMathType == InterpretationState.ScopeLevel.EnumSimpleMathType.INT ) {
                    resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                    resultVariadic.valueInt = (int)topScope.simpleMathResult;
                }
                else {
                    resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                    resultVariadic.valueFloat = topScope.simpleMathResult;
                }

                returnResult(state, resultVariadic);

                // remove scope of call
                removeTopScope(state);

                return;
            }
            // else we are here


            // invoke n'th value

            topScope.simpleMathState = InterpretationState.ScopeLevel.EnumSimpleMathState.SUCCEEDINGCALCULATE;

            // build scope and setup

            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            dagIndexOfCalled = currentDagElement.childIndices[topScope.simpleMathIndex];
            createdScopeLevel.dagElementIndex = dagIndexOfCalled;


            // misc
            // do this so the next step executes the scope
            state.currentScopeLevel++;


            topScope.simpleMathIndex++;

            return;
        }
        else if( topScope.simpleMathState == InterpretationState.ScopeLevel.EnumSimpleMathState.SUCCEEDINGCALCULATE ) {
            float calleeResult;

            if( topScope.calleeResult.type == Datastructures.Variadic.EnumType.INT ) {
                calleeResult = (float)topScope.calleeResult.valueInt;
            }
            else if( topScope.calleeResult.type == Datastructures.Variadic.EnumType.FLOAT ) {
                calleeResult = topScope.calleeResult.valueFloat;
                topScope.simpleMathType = InterpretationState.ScopeLevel.EnumSimpleMathType.FLOAT;
            }
            else {
                throw new Exception("arithmetic  value at [" + (topScope.simpleMathIndex-1).ToString() + "] is not a number!");
            }

            // do real calculate
            if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.ADD ) {
                topScope.simpleMathResult += calleeResult;
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.SUB ) {
                topScope.simpleMathResult -= calleeResult;
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.MUL ) {
                topScope.simpleMathResult *= calleeResult;
            }
            else if( currentDagElement.content.valueInt == (int)Parser.Functional.ScopeParseTreeElement.EnumType.DIV ) {
                topScope.simpleMathResult /= calleeResult;
            }
            else {
                throw new Exception("Internal error!");
            }

            topScope.simpleMathState = InterpretationState.ScopeLevel.EnumSimpleMathState.SUCCEEDINGTOCALL;

            return;
        }
        else {
            throw new Exception("Internal Error!");
        }

        // unreachable
    }

    private void executeSetVariables(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, InterpretationState state) {
        int currentDagElementIndex;
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element currentDagElement;

        InterpretationState.ScopeLevel topScope;

        currentDagElementIndex = state.scopeLevels[state.scopeLevels.length - 1].dagElementIndex;
        currentDagElement = dag.elements[currentDagElementIndex];

        topScope = state.scopeLevels[state.currentScopeLevel];

        if( topScope.setVariablesState == InterpretationState.ScopeLevel.EnumSetVariablesState.INITIAL ) {
            int dagIndexOfVariablesArray;
            bool numberOfVariableArrayElementsValid;
            
            dagIndexOfVariablesArray = currentDagElement.childIndices[0];

            if( dag.elements[dagIndexOfVariablesArray].content.type != DagElementData.EnumType.FARRAY ) {
                throw new Exception("set  first argument must be a array!");
            }

            numberOfVariableArrayElementsValid = (dag.elements[dagIndexOfVariablesArray].childIndices.length % 2) == 0;
            if( !numberOfVariableArrayElementsValid ) {
                throw new Exception("set  number of elements of the variable array is invalid!");
            }

            topScope.setVariablesState = InterpretationState.ScopeLevel.EnumSetVariablesState.RESOLVEVARIABLES;

            return;
        }
        else if( topScope.setVariablesState == InterpretationState.ScopeLevel.EnumSetVariablesState.RESOLVEVARIABLES ) {
            int numberOfVariables;
            int dagIndexOfVariablesArray;
            List<int> variableArrayDagIndices;

            dagIndexOfVariablesArray = currentDagElement.childIndices[0];

            variableArrayDagIndices = dag.elements[dagIndexOfVariablesArray].childIndices;
            numberOfVariables = variableArrayDagIndices.length / 2;

            for(;;) {
                int indexInArrayOfVariableIdentifier;
                int indexInArrayOfVariableValue;

                int dagIndexOfVariableIndentifier;
                int dagIndexOfVariableValue;

                string identifierOfVariableToSet;

                System.Diagnostics.Debug.Assert(topScope.setVariablesVariableIndex <= numberOfVariables);
                if( topScope.setVariablesVariableIndex == numberOfVariables ) {
                    topScope.setVariablesState = InterpretationState.ScopeLevel.EnumSetVariablesState.INVOKEBODY;

                    return;
                }

                indexInArrayOfVariableIdentifier = topScope.setVariablesVariableIndex*2;
                indexInArrayOfVariableValue = indexInArrayOfVariableIdentifier + 1;

                dagIndexOfVariableIndentifier = variableArrayDagIndices[indexInArrayOfVariableIdentifier];
                dagIndexOfVariableValue = variableArrayDagIndices[indexInArrayOfVariableValue];

                if( dag.elements[dagIndexOfVariableIndentifier].content.type != DagElementData.EnumType.IDENTIFIERNAME ) {
                    throw new Exception("set  Array content at absolute index " + dagIndexOfVariableIndentifier.ToString() + " must be an identifier!");
                }

                identifierOfVariableToSet = dag.elements[dagIndexOfVariableIndentifier].content.identifier;

                if( dag.elements[dagIndexOfVariableValue].content.type == DagElementData.EnumType.IDENTIFIERNAME ) {
                    bool variableWasResolved;
                    string sourceVariableName;
                    Datastructures.Variadic resolvedVariable;

                    // source is a variable

                    // * try to resolve the variable
                    // * set value of new variable (can also override an existing one in this scope)

                    sourceVariableName = dag.elements[dagIndexOfVariableValue].content.identifier;
                    resolvedVariable = resolveVariable(sourceVariableName, state, out variableWasResolved);
                    if( !variableWasResolved ) {
                        throw new Exception("set  variable " + sourceVariableName + " could not be resolved!");
                    }

                    // set value of new variable
                    setVariableInScope(identifierOfVariableToSet, EnumOverwriteExistingVariable.YES, resolvedVariable, topScope);

                    topScope.setVariablesVariableIndex++;

                    continue;
                }
                else {
                    int dagIndexOfCalled;
                    InterpretationState.ScopeLevel createdScopeLevel;

                    // source is not a variable

                    // open new scope to calculate the value for the variable

                    
                    topScope.setVariablesState = InterpretationState.ScopeLevel.EnumSetVariablesState.RESOLVEVARIABLESSCOPE;

                    // open scope ...
                    createdScopeLevel = new InterpretationState.ScopeLevel();
                    state.scopeLevels.Add(createdScopeLevel);

                    dagIndexOfCalled = dagIndexOfVariableValue;
                    createdScopeLevel.dagElementIndex = dagIndexOfCalled;

                    // do this so the next step executes the scope
                    state.currentScopeLevel++;

                    return;
                }

                // unreachable
                throw new Exception("Unreachable!");
            }

            // unreachable
            throw new Exception("Unreachable!");
        }
        else if( topScope.setVariablesState == InterpretationState.ScopeLevel.EnumSetVariablesState.RESOLVEVARIABLESSCOPE ) {
            Datastructures.Variadic variableValue;
            int indexInArrayOfVariableIdentifier;
            int dagIndexOfVariableIndentifier;
            string identifierOfVariableToSet;

            int dagIndexOfVariablesArray;
            List<int> variableArrayDagIndices;

            dagIndexOfVariablesArray = currentDagElement.childIndices[0];

            variableArrayDagIndices = dag.elements[dagIndexOfVariablesArray].childIndices;

            indexInArrayOfVariableIdentifier = topScope.setVariablesVariableIndex * 2;
            dagIndexOfVariableIndentifier = variableArrayDagIndices[indexInArrayOfVariableIdentifier];
            identifierOfVariableToSet = dag.elements[dagIndexOfVariableIndentifier].content.identifier;


            variableValue = topScope.calleeResult;

            setVariableInScope(identifierOfVariableToSet, EnumOverwriteExistingVariable.YES, variableValue, topScope);


            topScope.setVariablesVariableIndex++;

            topScope.setVariablesState = InterpretationState.ScopeLevel.EnumSetVariablesState.RESOLVEVARIABLES;

            return;
        }
        else if( topScope.setVariablesState == InterpretationState.ScopeLevel.EnumSetVariablesState.INVOKEBODY ) {
            int dagIndexOfCalled;
            InterpretationState.ScopeLevel createdScopeLevel;

            int dagIndexOfScope;

            dagIndexOfScope = currentDagElement.childIndices[1];

            topScope.setVariablesState = InterpretationState.ScopeLevel.EnumSetVariablesState.RETURNVALUE;

            // open scope ...
            createdScopeLevel = new InterpretationState.ScopeLevel();
            state.scopeLevels.Add(createdScopeLevel);

            dagIndexOfCalled = dagIndexOfScope;
            createdScopeLevel.dagElementIndex = dagIndexOfCalled;

            // do this so the next step executes the scope
            state.currentScopeLevel++;

            return;
        }
        else if( topScope.setVariablesState == InterpretationState.ScopeLevel.EnumSetVariablesState.RETURNVALUE ) {
            returnResult(state, topScope.calleeResult);

            // remove scope of call
            removeTopScope(state);

            return;
        }
        else {
            throw new Exception("Internal error");
        }
    }

    private static void lookupInvokePath(InterpretationState state, string[] pathOfInvokedProgram, out bool isInterpretableInvoke, out int dagIndexOfInvokedScope, string[] variableNames) {
        isInterpretableInvoke = false;
        dagIndexOfInvokedScope = -1;

        foreach( InterpretationState.InvokableProgram iterationInvokableProgram in state.invokablePrograms ) {
            bool pathIsTheSame;

            pathIsTheSame = isStringArrayTheSame(pathOfInvokedProgram, iterationInvokableProgram.path);
            if( !pathIsTheSame ) {
                continue;
            }

            dagIndexOfInvokedScope = iterationInvokableProgram.dagIndex;

            variableNames = iterationInvokableProgram.variableNames;

            isInterpretableInvoke = true;
            return;
        }
    }

    private static void setVariableInScope(
        string variablename,
        EnumOverwriteExistingVariable overwriteExistingVariable,
        Datastructures.Variadic value,
        InterpretationState.ScopeLevel topScope
    ) {
        int foundVariableIndex;
        int i;

        foundVariableIndex = -1;

        for( i = 0; i < topScope.variables.length; i++ ) {
            if( topScope.variables[i].name == variablename ) {
                foundVariableIndex = i;
                break;
            }
        }

        if( foundVariableIndex != -1 ) {
            if( overwriteExistingVariable == EnumOverwriteExistingVariable.NO ) {
                // variable allready existed, but can't be overwritten
                throw new Exception("variable " + variablename + " exists in scope but can't be overwritten!");
            }

            topScope.variables[foundVariableIndex].value = value;
        }
        else {
            InterpretationState.ScopeLevel.Variable newVariable;

            newVariable = new InterpretationState.ScopeLevel.Variable();
            newVariable.name = variablename;
            newVariable.value = value;

            topScope.variables.Add(newVariable);
        }
    }

    // helper method
    private static bool isStringArrayTheSame(string[] a, string[] b) {
        int i;

        if( a.length != b.length ) {
            return false;
        }

        for( i = 0; i < a.length; i++ ) {
            if( a[i] != b[i] ) {
                return false;
            }
        }

        return true;
    }

    private static void returnResult(InterpretationState state, Datastructures.Variadic result) {
        System.Diagnostics.Debug.Assert(state.scopeLevels.length >= 1);
        if (state.scopeLevels.length == 1) {
            state.result = result;
        }
        else {
            state.scopeLevels[state.currentScopeLevel - 1].calleeResult = result;
        }
    }

    /**
     * 
     * removes the scope with all variables and so on on the top
     * 
     */
    private static void removeTopScope(InterpretationState state) {
        state.scopeLevels.RemoveAt(state.scopeLevels.length - 1);
        state.currentScopeLevel--;
    }

    /**
     * 
     * builds the path for a invoke scope command, executes eventually called functions etc
     * 
     */
    private static string[] getPathOfInvokeScopeInstruction(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, int[] dagIndicesOfPathElements, InterpretationState state) {
        string[] result;

        foreach( int iterationDagIndex in dagIndicesOfPathElements ) {
            Datastructures.Dag<ProgramRepresentation.DagElementData>.Element iterationDagElement;

            iterationDagElement = dag.elements[iterationDagIndex];
             

            if( iterationDagElement.content.type == DagElementData.EnumType.CONSTSTRING ) {
                result ~= iterationDagElement.content.valueString;
            }
            else {
                // TODO< only strings are until now implemented
                //       later we need to support any operations >

                throw new Exception("Internal Error");
            }
        }

        return result;
    }

    private static Variadic[] resolveVariablesAndConstants(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, int[] dagIndicesOfVariablesOrConstants, InterpretationState state) {
        Variadic[] result;

        foreach( int iterationDagIndexOfVariableOrConstant in dagIndicesOfVariablesOrConstants ) {
            result.Add(resolveVariableAndConstant(dag, iterationDagIndexOfVariableOrConstant, state));
        }

        return result;
    }

    /**
     * 
     * only resolves variables and gets the value of constants etc
     * doesn't execute any invokes/function calls
     * 
     */
    private static Datastructures.Variadic resolveVariableAndConstant(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, int dagIndexOfVariablesOrConstant, InterpretationState state) {
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element dagElement;

        dagElement = dag.elements[dagIndexOfVariablesOrConstant];

        if( dagElement.content.type == DagElementData.EnumType.IDENTIFIERNAME ) {
            bool wasResolved;
            Datastructures.Variadic resultVariable;

            // it is a identifier (variable or a placeholder)
            // try to search it and return the value
            resultVariable = resolveVariable(dagElement.content.identifier, state, out wasResolved);
            if( !wasResolved ) {
                throw new Exception("variable " + dagElement.content.identifier + " could not be resolved!");
            }

            return resultVariable;
        }
        else if( dagElement.content.type == DagElementData.EnumType.CONSTSTRING ) {
            // TODO
            System.Diagnostics.Debug.Assert(false, "TODO");

            return null;
        }
        else if( dagElement.content.type == DagElementData.EnumType.CONSTINT ) {
            Datastructures.Variadic resultVariadic;

            resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            resultVariadic.valueInt = dagElement.content.valueInt;

            return resultVariadic;
        }
        else if( dagElement.content.type == DagElementData.EnumType.FARRAY ) {
            Datastructures.Variadic resultVariadic;

            resultVariadic = new Datastructures.Variadic(Datastructures.Variadic.EnumType.ARRAY);
            resultVariadic.valueArray = resolveVariablesAndConstants(dag, dagElement.childIndices, state);

            return resultVariadic;
        }
        // TODO< const float >
        else {
            System.Diagnostics.Debug.Assert(false, "TODO");
            throw new Exception("internal error");
        }
    }

    private static Datastructures.Variadic resolveVariable(string variableName, InterpretationState state, out bool wasResolved) {
        int scopeLevelIndex;

        wasResolved = false;

        for( scopeLevelIndex = state.scopeLevels.length-1;; scopeLevelIndex-- ) {
            InterpretationState.ScopeLevel currentScope;

            currentScope = state.scopeLevels[scopeLevelIndex];

            // search inside he variables for the variable
            foreach( InterpretationState.ScopeLevel.Variable iterationVariable in currentScope.variables ) {
                if( iterationVariable.name == variableName ) {
                    wasResolved = true;
                    return iterationVariable.value;
                }
            }

            if( scopeLevelIndex == 0 ) {
                return null;
            }
        }
    }

    private static bool isConstantOrVariable(Datastructures.Dag<ProgramRepresentation.DagElementData> dag, int dagIndex) {
        Datastructures.Dag<ProgramRepresentation.DagElementData>.Element dagElement;
        
        dagElement = dag.elements[dagIndex];

        return dagElement.content.type == DagElementData.EnumType.IDENTIFIERNAME ||
               dagElement.content.type == DagElementData.EnumType.CONSTSTRING ||
               dagElement.content.type == DagElementData.EnumType.CONSTINT ||
               dagElement.content.type == DagElementData.EnumType.CONSTFLOAT;
    }

    /**
     * 
     * checks if the Dag-Element type is the type or is convertable to it
     * 
     * 
     */
    private static bool isDagElementTypeTheSameOrConvertableToType(Datastructures.Dag<ProgramRepresentation.DagElementData>.Element dagElement, Datastructures.Variadic.EnumType toType) {
        if( dagElement.content.type == DagElementData.EnumType.CONSTINT ) {
            if( toType == Datastructures.Variadic.EnumType.INT || toType == Datastructures.Variadic.EnumType.FLOAT ) {
                return true;
            }
        }
        else if( dagElement.content.type == DagElementData.EnumType.CONSTBOOL ) {
            if( toType == Datastructures.Variadic.EnumType.BOOL ) {
                return true;
            }
            // ASK< are numbers implicitly convertable ? >
        }
        // TODO< others >

        return false;
    }

    private static Datastructures.Variadic convertTo(Datastructures.Dag<ProgramRepresentation.DagElementData>.Element dagElement, Datastructures.Variadic.EnumType toType) {
        Datastructures.Variadic result;

        if( dagElement.content.type == DagElementData.EnumType.CONSTINT ) {
            if( toType == Datastructures.Variadic.EnumType.INT ) {
                result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
                result.valueInt = dagElement.content.valueInt;

                return result;
            }
            else if( toType == Datastructures.Variadic.EnumType.FLOAT ) {
                result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                result.valueFloat = (float)Convert.ToDouble(dagElement.content.valueInt);

                return result;
            }
            else {
                System.Diagnostics.Debug.Assert(false, "TODO");
                throw new Exception("TODO");
            }
        }
        else if( dagElement.content.type == DagElementData.EnumType.CONSTFLOAT ) {
            if (toType == Datastructures.Variadic.EnumType.INT) {
                result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                result.valueFloat = (float)dagElement.content.valueInt;

                return result;
            }
            else if (toType == Datastructures.Variadic.EnumType.FLOAT) {
                result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.FLOAT);
                result.valueFloat = dagElement.content.valueFloat;

                return result;
            }
            else {
                System.Diagnostics.Debug.Assert(false, "TODO");
                throw new Exception("TODO");
            }
        }
        else if( dagElement.content.type == DagElementData.EnumType.CONSTBOOL ) {
            if( toType == Datastructures.Variadic.EnumType.BOOL ) {
                result = new Datastructures.Variadic(Datastructures.Variadic.EnumType.BOOL);
                result.valueBool = dagElement.content.valueBool;

                return result;
            }
            else {
                throw new Exception("bool is not convertable to <type>");
            }
        }

        System.Diagnostics.Debug.Assert(false);
        throw new Exception("Unreachable!");
    }

    private static bool doesVariableMatchPattern(Datastructures.Variadic variable, Datastructures.Variadic pattern) {
        System.Diagnostics.Debug.Assert(variable.type == pattern.type);

        return Datastructures.Variadic.isEqual(variable, pattern, 0.0001f);
    }

}

