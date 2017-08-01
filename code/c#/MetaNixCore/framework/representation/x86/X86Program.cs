using MetaNix.datastructures;

namespace MetaNix.framework.representation.x86 {
    public class X86Program {
        public MutableArray<X86Instruction> instructions = new MutableArray<X86Instruction>();

        public void interpret(X86ExecutionContext ctx) {
            for( int instrI = 0; instrI < instructions.length; instrI++ ) {
                X86Instruction instr = instructions[instrI];
                ctx.interpret(instr);
            }
        }
    }
}
