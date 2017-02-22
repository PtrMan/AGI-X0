using System;
using System.Collections.Generic;

namespace MetaNix {
    class Register {
        public long valueInt;
    }

    class PrimitiveInterpretationContext {
        public Register[] registers;

        public int ip; // instruction pointer (at the current level)

        public bool conditionFlag; // set by comparision and used by conditional
    }

    public abstract class Interpreter<ContextType> {
        public void interpret(ContextType interpretationContext, ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments, IList<string> argumentNames) {
            // is just a small layer over interpret2

            Ensure.ensureHard(arguments.Count <= argumentNames.Count);
            interpret2(interpretationContext, node, arguments, argumentNames);
        }

        protected abstract void interpret2(ContextType interpretationContext, ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments, IList<string> argumentNames);
    }

    public class FunctionalInterpretationContext {
        Dictionary<string, ImmutableNodeReferer> valuesByName = new Dictionary<string, ImmutableNodeReferer>();

        public bool existsVariableByName(string name) {
            return valuesByName.ContainsKey(name);
        }

        public void assignVariable(string name, ImmutableNodeReferer node) {
            Ensure.ensureHard(!existsVariableByName(name));
            valuesByName.Add(name, node);
        }

        internal ImmutableNodeReferer getVariableByName(string name) {
            Ensure.ensureHard(existsVariableByName(name));
            return valuesByName[name];
        }

        internal void resetVariables() {
            valuesByName.Clear();
        }
    }



    // used for tracing the functional interpretation
    public interface IFunctionalInterpretationTracer {
        int callEnter(string callName, ImmutableNodeReferer node);
        void callExit(int callId);

        /**
         * \param callId the id of the call returned by callEnter
         * \param node the corresponding node which got evaluated for the value of the argument
         * \param value the Node which carries or is the current argument
         * \param idx the index of the argument
         */
        void callArgument(int callId, ImmutableNodeReferer node, ImmutableNodeReferer value, int idx);

        void callResult(int callId, ImmutableNodeReferer resultNode);


        int sequenceEnter(ImmutableNodeReferer node);
        void sequenceExit(int sequenceId);

        // called when the interpreter is invoked for the function node with the arguments
        void interpreterEnter(ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments);
        // called when the interpreter is done doing it's job
        void interpreterExit();
    }

    public class NullFunctionalInterpreterTracer : IFunctionalInterpretationTracer {
        public void callArgument(int callId, ImmutableNodeReferer node, ImmutableNodeReferer value, int idx) {}
        public int callEnter(string callName, ImmutableNodeReferer node) {
            return 0;
        }

        public void callExit(int callId) {}
        public void callResult(int callId, ImmutableNodeReferer resultNode) {}

        public int sequenceEnter(ImmutableNodeReferer node) {
            return 0;
        }

        public void sequenceExit(int sequenceId) {}

        public void interpreterEnter(ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments) {}
        public void interpreterExit() {}
    }

    // interpreter for functional expressions
    public class FunctionalInterpreter : Interpreter<FunctionalInterpretationContext> {
        public IFunctionalInterpretationTracer tracer;

        protected override void interpret2(FunctionalInterpretationContext interpretationContext, ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments, IList<string> argumentNames) {
            resetVariablesToParameters(interpretationContext, arguments, argumentNames);

            tracer.interpreterEnter(node, arguments);
            recursiveInterpret(interpretationContext, node);
            tracer.interpreterExit();
        }

        private void resetVariablesToParameters(FunctionalInterpretationContext interpretationContext, IList<ImmutableNodeReferer> arguments, IList<string> argumentNames) {
            interpretationContext.resetVariables();
            int argumentIdx = 0;
            foreach (ImmutableNodeReferer iArgument in arguments) {
                string argumentName = argumentNames[argumentIdx];
                interpretationContext.assignVariable(argumentName, iArgument);
                argumentIdx++;
            }
        }

        static bool isCallValid(string callName) {
            switch (callName) {
                // arithmetic
                case "+":
                case "-":
                case "*":
                case "/":
                
                // bit manipulation
                case "shl":
                case "shr":
                case "bAnd": // binary and
                case "bOr": // binary or
                
                return true;

                default:
                return false;
            }
        }

        static bool isArithmetic(string callName) {
            switch (callName) {
                // arithmetic
                case "+":
                case "-":
                case "*":
                case "/":
                return true;

                default:
                return false;
            }
        }


        static void tryToWidenArithmeticType(ref Variant value, Variant.EnumType type) {
            if( value.type == type ) {
                return; // no widening required
            }

            if( value.type == Variant.EnumType.FLOAT && type == Variant.EnumType.INT ) {
                // no widening required because it's already "big" enough
                return;
            }

            if( type == Variant.EnumType.FLOAT && value.type == Variant.EnumType.INT ) {
                value = Variant.makeFloat(value.valueInt);
                return;
            }

            // if we are here then something gone wrong while checking for wrong types
            throw new Exception("INTERPRETATIONEXCEPTION internal error while widening");
        }

        private Variant castVariantTo(Variant argument, Variant.EnumType type) {
            if( argument.type == type ) { // no cast necessary
                return argument;
            }

            if( type == Variant.EnumType.FLOAT && argument.type == Variant.EnumType.INT ) {
                return Variant.makeFloat(argument.valueInt);
            }
            else if( type == Variant.EnumType.INT && argument.type == Variant.EnumType.FLOAT ) {
                return Variant.makeInt((long)argument.valueFloat);
            }

            throw new Exception("INTERPRETATIONEXCEPTION Intrnal error while casting, not handled case");
        }



        delegate void primitiveCalcResultForAdditionalArgumentType(Variant argument);


        void recursiveInterpret(FunctionalInterpretationContext interpretationContext, ImmutableNodeReferer node) {
            if( !node.isBranch ) {
                // it's it's own result

                node.interpretationResult = node;
                return;
            }

            // if we are here it must be an branch which must be executable


            // figure out if it is a native function call or a registered function call or a variable

            // if it is a string then this means that it indicates an variablename and not an call
            if( !node.children[0].isBranch && node.children[0].type == ValueNode.EnumType.STRING_DATATYPE ) {
                string variableName = getNativeString(node.children[0]);

                Ensure.ensure(interpretationContext.existsVariableByName(variableName));
                node.interpretationResult = interpretationContext.getVariableByName(variableName);
                return;
            }
            string operationName = getNativeString(node.children[0]);
            bool isSequence = operationName == "seq";
            bool isLet = operationName == "let";
            bool isSpecial = isSequence || isLet;
            string callName = "";
            if( !isSpecial ) {
                callName = operationName;

                bool nativeCallValid = isCallValid(callName);
                if (nativeCallValid) {
                    // nothing by purpose
                }
                else {
                    throw new Exception("INTERPRETATIONEXCEPTION name \"" + operationName + "\"is not a native function call or a registered function call or a variable");
                }
            }
            
            int callId = -1, sequenceId = -1; // illegal
            if (isSequence) {
                sequenceId = tracer.sequenceEnter(node);
            }
            else if(isLet) {
                // nothing by purpose
            }
            else {
                Ensure.ensureHard(!isSpecial);
                callId = tracer.callEnter(callName, node);
            }

            // is null if the result initialization is dependent on the callName
            Variant? interpretationResult = null;
            bool isFirstArgument = true;

            primitiveCalcResultForAdditionalArgumentType primitiveCalcResultForAdditionalArgument = delegate (Variant argument) {
                if( isFirstArgument ) {
                    isFirstArgument = false;
                    interpretationResult = argument;
                    return;
                }

                { // scope just for the null mess of interpretationResult
                    Variant interpretationResultTemp = interpretationResult.Value;
                    tryToWidenArithmeticType(ref interpretationResultTemp, argument.type);
                    interpretationResult = interpretationResultTemp;
                }
                Variant castedArgument = castVariantTo(argument, interpretationResult.Value.type);

                if ( callName == "+" ) {
                    if(castedArgument.type == Variant.EnumType.FLOAT) {
                        interpretationResult = Variant.makeFloat(interpretationResult.Value.valueFloat + castedArgument.valueFloat);
                    }
                    else if(castedArgument.type == Variant.EnumType.INT) {
                        interpretationResult = Variant.makeInt(interpretationResult.Value.valueInt + castedArgument.valueInt);
                    }
                }
                else if (callName == "-") {
                    if (castedArgument.type == Variant.EnumType.FLOAT) {
                        interpretationResult = Variant.makeFloat(interpretationResult.Value.valueFloat - castedArgument.valueFloat);
                    }
                    else if (castedArgument.type == Variant.EnumType.INT) {
                        interpretationResult = Variant.makeInt(interpretationResult.Value.valueInt - castedArgument.valueInt);
                    }
                }
                else if (callName == "*") {
                    if (castedArgument.type == Variant.EnumType.FLOAT) {
                        interpretationResult = Variant.makeFloat(interpretationResult.Value.valueFloat * castedArgument.valueFloat);
                    }
                    else if (castedArgument.type == Variant.EnumType.INT) {
                        interpretationResult = Variant.makeInt(interpretationResult.Value.valueInt * castedArgument.valueInt);
                    }
                }
                else if (callName == "/") {
                    if (castedArgument.type == Variant.EnumType.FLOAT) {
                        interpretationResult = Variant.makeFloat(interpretationResult.Value.valueFloat / castedArgument.valueFloat);
                    }
                    else if (castedArgument.type == Variant.EnumType.INT) {
                        interpretationResult = Variant.makeInt(interpretationResult.Value.valueInt / castedArgument.valueInt);
                    }
                }

            };

            if( isLet ) {
                Ensure.ensure(node.children.Count == 1+2); // let and variableAssignmentNode an executionNode
                ImmutableNodeReferer variableAssignmentArrayNode = node.children[1];
                ImmutableNodeReferer executionNode = node.children[2];

                Ensure.ensure(variableAssignmentArrayNode.isBranch);

                Ensure.ensure(variableAssignmentArrayNode.children.Count != 0);
                Ensure.ensure(getNativeString(variableAssignmentArrayNode.children[0]) == "array");

                Ensure.ensure((variableAssignmentArrayNode.children.Count - 1) % 2 == 0); // array minus the array prefix must have a length divisable by two
                int numberOfAssingments = (variableAssignmentArrayNode.children.Count - 1) / 2;
                for ( int assigmentI = 0; assigmentI < numberOfAssingments; assigmentI++ ) {
                    string assignmentVariableName = getNativeString(variableAssignmentArrayNode.children[(1+ assigmentI*2)]);
                    ImmutableNodeReferer assignmentNode = variableAssignmentArrayNode.children[(1 + assigmentI * 2)    + 1];

                    // for it to be calculated
                    recursiveInterpret(interpretationContext, assignmentNode);

                    // try to assign the variable
                    Ensure.ensure(!interpretationContext.existsVariableByName(assignmentVariableName));
                    interpretationContext.assignVariable(assignmentVariableName, assignmentNode.interpretationResult);
                }

                // execute the execution node
                recursiveInterpret(interpretationContext, executionNode);
            }
            else {
                // force all arguments to be calculated
                for (int argIdx = 0; argIdx < node.children.Count - 1; argIdx++) {
                    ImmutableNodeReferer argumentNode = node.children[argIdx + 1];
                    recursiveInterpret(interpretationContext, argumentNode);
                }
            }

            if ( isSequence ) {
                tracer.sequenceExit(sequenceId);
            }
            else if( !isSpecial ) {
                if ( isArithmetic(callName) ) {
                    // process arguments
                    for (int argIdx = 0; argIdx < node.children.Count - 1; argIdx++) {
                        ImmutableNodeReferer argumentValueNode = node.children[argIdx + 1].interpretationResult;

                        primitiveCalcResultForAdditionalArgument(argumentValueNode.value);

                        tracer.callArgument(callId, node.children[argIdx + 1], argumentValueNode, argIdx);
                    }
                }
                else if( callName == "shl" || callName == "shr" || callName == "bAnd" || callName == "bOr" ) { // shift
                    Ensure.ensure(node.children.Count == 1+2/* two arguments*/);
                    ImmutableNodeReferer leftNode = node.children[1 + 0].interpretationResult;
                    ImmutableNodeReferer rightNode = node.children[1 + 1].interpretationResult;
                    long leftValue = leftNode.value.valueInt;
                    long rightValue = rightNode.value.valueInt;

                    long result = 0;
                    if ( callName == "shl" ) {
                        result = leftValue << (int)rightValue;
                    }
                    else if( callName == "shr" ) {
                        result = leftValue >> (int)rightValue;
                    }
                    else if (callName == "bAnd") {
                        result = leftValue & rightValue;
                    }
                    else if (callName == "bOr") {
                        result = leftValue | rightValue;
                    }
                    // else should never happen

                    interpretationResult = Variant.makeInt(result);

                    tracer.callArgument(callId, node.children[1 + 0], leftNode, 0);
                    tracer.callArgument(callId, node.children[1 + 1], rightNode, 1);
                }
                else {
                    throw new Exception("INTERNALERRROR"); // hard internal error because the case should be handled, because we already made sure that the callName is valid
                }

                node.interpretationResult = ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(interpretationResult.Value));

                tracer.callResult(callId, node.interpretationResult);
                tracer.callExit(callId);
            }
        }

        

        // TODO< move to helper >
        static void ensureNodeIsDatatype(ImmutableNodeReferer node, string datatype) {
            Ensure.ensure(node.type == ValueNode.EnumType.STRING_DATATYPE && node.valueString == datatype);
        }

        // TODO< move to helper >
        static string getNativeString(ImmutableNodeReferer node) {
            ensureNodeIsDatatype(node.children[0], "string");

            char[] charArray = new char[node.children.Count - 1];
            for(int i = 0; i < node.children.Count-1; i++) {
                charArray[i] = (char)node.children[i + 1].valueInt;
            }

            return new string(charArray);
        }
    }

    // interpreter for primitive instructions
    class PrimitiveInstructionInterpreter : Interpreter<PrimitiveInterpretationContext> {
        
        

        InterpretationInstruction
            // comparision instructions
            instructionCmpNeqInteger,
            instructionCmpEqInteger,
            instructionCmpGInteger,
            instructionCmpGeInteger

            ;

        InterpretationInstruction instructionGoto;

        InterpretationInstruction instructionAddInteger;
        InterpretationInstruction instructionSubInteger;
        InterpretationInstruction instructionMulInteger;

        InterpretationInstruction instructionDerefInteger;

        public PrimitiveInstructionInterpreter() {
            instructionCmpNeqInteger = new InterpreterInstructionCmpInteger(InterpreterInstructionCmpInteger.EnumComparisionType.NEQ_INT);
            instructionCmpEqInteger = new InterpreterInstructionCmpInteger(InterpreterInstructionCmpInteger.EnumComparisionType.EQ_INT);
            instructionCmpGInteger = new InterpreterInstructionCmpInteger(InterpreterInstructionCmpInteger.EnumComparisionType.G_INT);
            instructionCmpGeInteger = new InterpreterInstructionCmpInteger(InterpreterInstructionCmpInteger.EnumComparisionType.GE_INT);

            instructionGoto = new InterpreterInstructionGoto();

            instructionAddInteger = new InterpreterInstructionAddInteger();
            instructionSubInteger = new InterpreterInstructionSubInteger();
            instructionMulInteger = new InterpreterInstructionMulInteger();

            instructionDerefInteger = new InterpreterInstructionDerefInteger();
        }

        protected override void interpret2(PrimitiveInterpretationContext interpretationContext, ImmutableNodeReferer node, IList<ImmutableNodeReferer> arguments, IList<string> argumentNames) {
            switch(node.children[0].type) {
                case ValueNode.EnumType.INSTR_CMP_NEQ_INT:
                instructionCmpNeqInteger.execute(interpretationContext, node);
                return;

                case ValueNode.EnumType.INSTR_CMP_EQ_INT:
                instructionCmpEqInteger.execute(interpretationContext, node);
                return;

                case ValueNode.EnumType.INSTR_CMP_G_INT:
                instructionCmpGInteger.execute(interpretationContext, node);
                return;

                case ValueNode.EnumType.INSTR_CMP_GE_INT:
                instructionCmpGeInteger.execute(interpretationContext, node);
                return;



                case ValueNode.EnumType.INSTR_GOTO:
                instructionGoto.execute(interpretationContext, node);
                return;



                case ValueNode.EnumType.INSTR_ADD_INT:
                instructionAddInteger.execute(interpretationContext, node);
                return;

                case ValueNode.EnumType.INSTR_SUB_INT:
                instructionSubInteger.execute(interpretationContext, node);
                return;

                case ValueNode.EnumType.INSTR_MUL_INT:
                instructionMulInteger.execute(interpretationContext, node);
                return;


                case ValueNode.EnumType.INSTR_DEREF_INT:
                instructionDerefInteger.execute(interpretationContext, node);
                return;
            }
        }
    }

    interface InterpretationInstruction {
        void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node);
    }

    // comparision instruction working on integer described by type
    class InterpreterInstructionCmpInteger : InterpretationInstruction {
        public enum EnumComparisionType {
            NEQ_INT,
            EQ_INT,
            G_INT,
            GE_INT,
        }

        EnumComparisionType comparisionType;

        public InterpreterInstructionCmpInteger(EnumComparisionType comparisionType) {
            this.comparisionType = comparisionType;
        }

        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            long registerIdxA = node.children[1].valueInt;
            long registerIdxB = node.children[2].valueInt;

            // fetch registers
            long valueA = context.registers[registerIdxA].valueInt;
            long valueB = context.registers[registerIdxB].valueInt;

            switch(comparisionType) {
                case EnumComparisionType.NEQ_INT:
                context.conditionFlag = valueA != valueB;
                break;

                case EnumComparisionType.EQ_INT:
                context.conditionFlag = valueA == valueB;
                break;

                case EnumComparisionType.G_INT:
                context.conditionFlag = valueA > valueB;
                break;

                case EnumComparisionType.GE_INT:
                context.conditionFlag = valueA >= valueB;
                break;
            }
        }
    }

    class InterpreterInstructionGoto : InterpretationInstruction {
        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            Ensure.ensure(node.children[1].valueInt >= 0);// TODO< ensure >
            context.ip = (int)node.children[1].valueInt;

        }
    }

    class InterpreterInstructionAddInteger : InterpretationInstruction {
        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            Ensure.ensure(node.children.Count >= 1 + 1); // arguments have to be at least   op + target
            int numberOfArgs = node.children.Count - 1 - 1;
            
            long accumulator = 0;

            for(int argIdx = 0; argIdx < numberOfArgs; argIdx++) {
                long registerIdxIteration = node.children[1 + argIdx].valueInt;
                accumulator += context.registers[registerIdxIteration].valueInt;
            }

            long registerIdxDest = node.children[node.children.Count - 1].valueInt;
            context.registers[registerIdxDest].valueInt = accumulator;

            context.ip++;
        }
    }

    class InterpreterInstructionSubInteger : InterpretationInstruction {
        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            Ensure.ensure(node.children.Count >= 1 + 1); // arguments have to be at least   op + target
            int numberOfArgs = node.children.Count - 1 - 1;

            long accumulator = 0;

            {
                long registerIdxIteration = node.children[1 + 0].valueInt;
                accumulator += context.registers[registerIdxIteration].valueInt;
            }

            for (int argIdx = 1; argIdx < numberOfArgs; argIdx++) {
                long registerIdxIteration = node.children[1 + argIdx].valueInt;
                accumulator -= context.registers[registerIdxIteration].valueInt;
            }

            long registerIdxDest = node.children[node.children.Count - 1].valueInt;
            context.registers[registerIdxDest].valueInt = accumulator;

            context.ip++;
        }
    }

    class InterpreterInstructionMulInteger : InterpretationInstruction {
        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            Ensure.ensure(node.children.Count >= 1 + 1); // arguments have to be at least   op + target
            int numberOfArgs = node.children.Count - 1 - 1;

            long accumulator = 1;

            for (int argIdx = 0; argIdx < numberOfArgs; argIdx++) {
                long registerIdxIteration = node.children[1 + argIdx].valueInt;
                accumulator *= context.registers[registerIdxIteration].valueInt;
            }

            long registerIdxDest = node.children[node.children.Count - 1].valueInt;
            context.registers[registerIdxDest].valueInt = accumulator;

            context.ip++;
        }
    }


    class InterpreterInstructionDerefInteger : InterpretationInstruction {
        public void execute(PrimitiveInterpretationContext context, ImmutableNodeReferer node) {
            ImmutableNodeReferer pathNode = node.children[1];
            // translate path to integer array
            int[] path = new int[pathNode.children.Count];
            for(int i = 0;i < pathNode.children.Count; i++) {
                path[i] = (int)pathNode.children[i].valueInt;
            }

            ImmutableNodeReferer valueNode = NodeWalker.walk(node.parent, path);
            long value = valueNode.valueInt;

            // store into register
            long registerIdx = node.children[2].valueInt;
            context.registers[registerIdx].valueInt = value;

            context.ip++;
        }
    }



    class NodeWalker {
        public static ImmutableNodeReferer walk(ImmutableNodeReferer entry, int[] path, int pathStartIdx = 0) {
            Ensure.ensure(pathStartIdx <= path.Length);

            if (path.Length - pathStartIdx == 0) {
                return entry;
            }

            int currentPathElementIdx = path[pathStartIdx];
            Ensure.ensure(currentPathElementIdx >= -1); // -1 is go to parent and all other values is go to path

            if( currentPathElementIdx == -1 ) {
                return walk(entry.parent, path, pathStartIdx+1);
            }

            return walk(entry.children[currentPathElementIdx], path, pathStartIdx + 1);
        }

        public static void walkAndAct(ImmutableNodeReferer entry, int[] path, INodeAction action, int pathStartIdx = 0) {
            ImmutableNodeReferer node = walk(entry, path, pathStartIdx);
            action.act(node);
        }
    }

    interface INodeAction {
        void act(ImmutableNodeReferer current);
    }
}
