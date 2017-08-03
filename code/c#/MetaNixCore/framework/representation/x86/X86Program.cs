using AiThisAndThat.prototyping;

namespace MetaNix.framework.representation.x86 {
    public class X86Program : AbstractProgram<X86Instruction, X86ExecutionContext> {
        public override void interpret(X86ExecutionContext ctx) {
            for( int instrI = 0; instrI < instructions.length; instrI++ ) {
                X86Instruction instr = instructions[instrI];
                ctx.interpret(instr);
            }
        }
    }
}
