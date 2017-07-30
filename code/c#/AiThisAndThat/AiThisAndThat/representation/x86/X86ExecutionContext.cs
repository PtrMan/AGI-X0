using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiThisAndThat.representation.x86 {
    public class X86ExecutionContext {
        public X86ExecutionContext(int numberOfIntegerRegisters, int numberOfVectorRegisters) {
            integerRegisters = new int[numberOfIntegerRegisters];
            vectorRegisters = new float[numberOfVectorRegisters][];
            for( int i = 0; i < numberOfVectorRegisters; i++ )  vectorRegisters[i] = new float[4];
        }

        public X86ExecutionContext deepClone() {
            X86ExecutionContext cloned = new X86ExecutionContext(integerRegisters.Length, vectorRegisters.Length);
            Array.Copy(integerRegisters, cloned.integerRegisters, integerRegisters.Length);
            for( int i = 0; i < vectorRegisters.Length; i++ )  Array.Copy(vectorRegisters[i], cloned.vectorRegisters[i], 4);
            return cloned;
        }


        public void interpret(X86Instruction instr) {
            int temp;
            
            switch(instr.type) {
                case X86Instruction.EnumInstructionType.MOV_INTINT:
                integerRegisters[instr.dest] = integerRegisters[instr.a];
                break;
                
                case X86Instruction.EnumInstructionType.ADD_INTCONST:
                integerRegisters[instr.dest] += instr.immediate;
                break;
                
                case X86Instruction.EnumInstructionType.SUB_INTCONST:
                integerRegisters[instr.dest] -= instr.immediate;
                break;
                
                case X86Instruction.EnumInstructionType.EXCHANGEINT_INT:
                temp = integerRegisters[instr.dest];
                integerRegisters[instr.dest] = integerRegisters[instr.a];
                integerRegisters[instr.a] = temp;
                break;
                
                case X86Instruction.EnumInstructionType.ADD_INT :
                integerRegisters[instr.dest] += integerRegisters[instr.a];
                break;
                
                case X86Instruction.EnumInstructionType.SUB_INT:
                integerRegisters[instr.dest] -= integerRegisters[instr.a];
                break;
                
                case X86Instruction.EnumInstructionType.MUL_INT:
                integerRegisters[instr.dest] *= integerRegisters[instr.a];
                break;

                case X86Instruction.EnumInstructionType.HORIZONTALADD_FLOATVECTOR4:
                instructionHorizontalAdd_floatVector4(instr);
                break;

                case X86Instruction.EnumInstructionType.HORIZONTALSUB_FLOATVECTOR4:
                instructionHorizontalSub_floatVector4(instr);
                break;

                case X86Instruction.EnumInstructionType.ADD_FLOATVECTOR4:
                instructionAdd_floatVector4(instr);
                break;
                
                case X86Instruction.EnumInstructionType.SUB_FLOATVECTOR4:
                instructionSub_floatVector4(instr);
                break;

                case X86Instruction.EnumInstructionType.MUL_FLOATVECTOR4:
                instructionMul_floatVector4(instr);
                break;
                
                case X86Instruction.EnumInstructionType.DIV_FLOATVECTOR4:
                instructionDiv_floatVector4(instr);
                break;
                
                case X86Instruction.EnumInstructionType.SHUFFLE_FLOATVECTOR4:
                instructionShuffle_floatVector4(instr);
                break;
            }
        }

        void instructionShuffle_floatVector4(X86Instruction instr) {
            int mask = instr.immediate2;
            int source0 = (mask >> 0) & 0x3;
            int source1 = (mask >> 2) & 0x3;
            int source2 = (mask >> 4) & 0x3;
            int source3 = (mask >> 6) & 0x3;

            float result0 = vectorRegisters[instr.a][source0];
            float result1 = vectorRegisters[instr.a][source1];
            float result2 = vectorRegisters[instr.a][source2];
            float result3 = vectorRegisters[instr.a][source3];
            
            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }

        void instructionAdd_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] + vectorRegisters[instr.dest][0];
            float result1 = vectorRegisters[instr.a][1] + vectorRegisters[instr.dest][1];
            float result2 = vectorRegisters[instr.a][2] + vectorRegisters[instr.dest][2];
            float result3 = vectorRegisters[instr.a][3] + vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }
        void instructionSub_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] - vectorRegisters[instr.dest][0];
            float result1 = vectorRegisters[instr.a][1] - vectorRegisters[instr.dest][1];
            float result2 = vectorRegisters[instr.a][2] - vectorRegisters[instr.dest][2];
            float result3 = vectorRegisters[instr.a][3] - vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }
        void instructionMul_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] * vectorRegisters[instr.dest][0];
            float result1 = vectorRegisters[instr.a][1] * vectorRegisters[instr.dest][1];
            float result2 = vectorRegisters[instr.a][2] * vectorRegisters[instr.dest][2];
            float result3 = vectorRegisters[instr.a][3] * vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }
        void instructionDiv_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] / vectorRegisters[instr.dest][0];
            float result1 = vectorRegisters[instr.a][1] / vectorRegisters[instr.dest][1];
            float result2 = vectorRegisters[instr.a][2] / vectorRegisters[instr.dest][2];
            float result3 = vectorRegisters[instr.a][3] / vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }

        void instructionHorizontalSub_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] - vectorRegisters[instr.a][1];
            float result1 = vectorRegisters[instr.a][2] - vectorRegisters[instr.a][3];
            float result2 = vectorRegisters[instr.dest][0] - vectorRegisters[instr.dest][1];
            float result3 = vectorRegisters[instr.dest][2] - vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }
        void instructionHorizontalAdd_floatVector4(X86Instruction instr) {
            float result0 = vectorRegisters[instr.a][0] + vectorRegisters[instr.a][1];
            float result1 = vectorRegisters[instr.a][2] + vectorRegisters[instr.a][3];
            float result2 = vectorRegisters[instr.dest][0] + vectorRegisters[instr.dest][1];
            float result3 = vectorRegisters[instr.dest][2] + vectorRegisters[instr.dest][3];

            vectorRegisters[instr.dest][0] = result0;
            vectorRegisters[instr.dest][1] = result1;
            vectorRegisters[instr.dest][2] = result2;
            vectorRegisters[instr.dest][3] = result3;
        }

        public int[] integerRegisters;
        public float[][] vectorRegisters;
    }
}
