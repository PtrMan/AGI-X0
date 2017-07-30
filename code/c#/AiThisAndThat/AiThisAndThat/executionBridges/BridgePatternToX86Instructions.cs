using System.Collections.Generic;

using AiThisAndThat.representation.x86;

using PatternWithDecoration = AiThisAndThat.patternMatching.Pattern<AiThisAndThat.patternMatching.Decoration>;
using PatternInterpretationContext = AiThisAndThat.patternMatching.InterpretationContext;
using PatternInterpreter = AiThisAndThat.patternMatching.Interpreter;

namespace AiThisAndThat.executionBridges {
    // call a X86 program by a pattern
    public class BridgePatternToX86InstructionsForFloat {
        public void call(PatternInterpretationContext interpretationCtx, IList<PatternWithDecoration> arguments) {
            PatternWithDecoration resultPattern = arguments[arguments.Count-1];

            PatternInterpreter.vmAssert(
                resultPattern.type == PatternWithDecoration.EnumType.VARIABLE,
                false,
                "Last argument must be result which is a variable");

            X86ExecutionContext ctx = new X86ExecutionContext(8, 8);

            // translate arguments to float values in context
            // the first value of the vector is used
            for( int argI = 0; argI < arguments.Count-1; argI++ ) {
                PatternWithDecoration argument = arguments[argI];
                PatternWithDecoration argumentValue;
                if( argument.type == PatternWithDecoration.EnumType.VARIABLE ) {
                    PatternInterpreter.vmAssert(
                        interpretationCtx.valueByVariable[argument.variableId].decoration.type == patternMatching.Decoration.EnumType.VALUE,
                        false,
                        "Variable must be value");

                    argumentValue = interpretationCtx.valueByVariable[argument.variableId];
                }
                else {
                    argumentValue = argument;
                }

                PatternInterpreter.vmAssert(
                    argumentValue.decoration.type == patternMatching.Decoration.EnumType.VALUE,
                    false,
                    "Argument must be value");

                PatternInterpreter.vmAssert(
                    argumentValue.decoration.value is float,
                    false,
                    "Argument must be float value");

                ctx.vectorRegisters[argI][0] = (float)argumentValue.decoration.value;
            }

            // interpret program
            for( int i = 0; i < instructions.Length; i++ ) {
                ctx.interpret(instructions[i]);
            }

            
            float floatResult = ctx.vectorRegisters[0][0];

            // translate result
            if( interpretationCtx.valueByVariable.ContainsKey(resultPattern.variableId) ) {
                interpretationCtx.valueByVariable[resultPattern.variableId].decoration.value = floatResult;
            }
            else {
                interpretationCtx.valueByVariable[resultPattern.variableId] = new PatternWithDecoration();
                interpretationCtx.valueByVariable[resultPattern.variableId].decoration = new patternMatching.Decoration();
                interpretationCtx.valueByVariable[resultPattern.variableId].decoration.value = floatResult;
            }

        }
        
        public X86Instruction[] instructions;
    }
}
