namespace MetaNix.framework.representation.x86 {
    public class X86Instruction {
        public enum EnumInstructionType {
            NOP,
            ADD_INTCONST,
            SUB_INTCONST,
            MOV_INTINT, // move between integer and integer
            EXCHANGEINT_INT,
            ADD_INT,
            SUB_INT,
            MUL_INT,

            HORIZONTALADD_FLOATVECTOR4, // like SSE   haddps xmm, xmm
            HORIZONTALSUB_FLOATVECTOR4, // like SSE   hsubps xmm, xmm
            ADD_FLOATVECTOR4,
            SUB_FLOATVECTOR4,
            MUL_FLOATVECTOR4,
            DIV_FLOATVECTOR4,

            SHUFFLE_FLOATVECTOR4,

            // TODO

            // SQRT_ps
            // RSQRT_ps

            // blend_ps
            //  _mm_dp_ps     SSE4.1  conditional dot product

            // _mm_round_ps   SSE4.1  rounding
        }

        public EnumInstructionType type;

        public int a, dest;

        // immediate value
        public int immediate {
            get {
                return a;
            }
        }

        public int immediate2; // always a immediate

        public static bool doesInstructionNeedExtendedImmediate(EnumInstructionType instructionType) {
            return
                instructionType == EnumInstructionType.ADD_INTCONST ||
                instructionType == EnumInstructionType.SUB_INTCONST;
        }

        public static bool doesInstructionNeedImmediate2(EnumInstructionType instructionType) {
            return instructionType == EnumInstructionType.SHUFFLE_FLOATVECTOR4;
        }
    }
}
