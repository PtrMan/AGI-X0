using AiThisAndThat.representation.x86;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// just a small experiment of a superoptimizer for a simple X86 like target machine
// this is because we can easily emit "real" X86 programs for these programs

namespace AiThisAndThat.super.optimization {
    
    public class EnumerationContext {
        X86Instruction[] targetProgram; // program which has to be optimized

        static void interpret(X86ExecutionContext ctx, X86Instruction[] program ) {
            for( int instrI = 0; instrI < program.Length; instrI++ ) {
                X86Instruction instr = program[instrI];
                ctx.interpret(instr);
            }
        }

        int[] instructionEnumeration;
        int[] maximalValue;


        public void test() {
            targetProgram = new X86Instruction[3];
            /*
            targetProgram[0] = new Instruction();
            targetProgram[0].type = Instruction.EnumInstructionType.ADD_INTCONST;
            targetProgram[0].dest = 0;
            targetProgram[0].a = 4;

            targetProgram[1] = new Instruction();
            targetProgram[1].type = Instruction.EnumInstructionType.ADD_INTCONST;
            targetProgram[1].dest = 0;
            targetProgram[1].a = 4;

            targetProgram[2] = new Instruction();
            targetProgram[2].type = Instruction.EnumInstructionType.MUL_INT;
            targetProgram[2].dest = 0;
            targetProgram[2].a = 1;

            targetProgram[3] = new Instruction();
            targetProgram[3].type = Instruction.EnumInstructionType.MOV_INTINT;
            targetProgram[3].dest = 1;
            targetProgram[3].a = 0;
            */
            targetProgram[0] = new X86Instruction();
            targetProgram[0].type = X86Instruction.EnumInstructionType.MUL_FLOATVECTOR4;
            targetProgram[0].dest = 0;
            targetProgram[0].a = 0;

            targetProgram[1] = new X86Instruction();
            targetProgram[1].type = X86Instruction.EnumInstructionType.HORIZONTALADD_FLOATVECTOR4;
            targetProgram[1].dest = 0;
            targetProgram[1].a = 0;

            targetProgram[2] = new X86Instruction();
            targetProgram[2].type = X86Instruction.EnumInstructionType.HORIZONTALADD_FLOATVECTOR4;
            targetProgram[2].dest = 0;
            targetProgram[2].a = 0;



            instructionEnumeration = new int[3 * widthOfInstructionEncoding];
            maximalValue = new int[3 * widthOfInstructionEncoding];
            maximalValue[0 * widthOfInstructionEncoding + 0] = 4; // immediate2
            maximalValue[0 * widthOfInstructionEncoding + 1] = numberOfInstructions; // type
            maximalValue[0 * widthOfInstructionEncoding + 2] = 8; // dest
            maximalValue[0 * widthOfInstructionEncoding + 3] = 64; // source/immediate

            maximalValue[1 * widthOfInstructionEncoding + 0] = 4; // immediate2
            maximalValue[1 * widthOfInstructionEncoding + 1] = numberOfInstructions; // type
            maximalValue[1 * widthOfInstructionEncoding + 2] = 8; // dest
            maximalValue[1 * widthOfInstructionEncoding + 3] = 64; // source/immediate

            maximalValue[2 * widthOfInstructionEncoding + 0] = 4; // immediate2
            maximalValue[2 * widthOfInstructionEncoding + 1] = numberOfInstructions; // type
            maximalValue[2 * widthOfInstructionEncoding + 2] = 8; // dest
            maximalValue[2 * widthOfInstructionEncoding + 3] = 64; // source/immediate


            for (;;) {
                enumerateStep();
            }
        }

        public void enumerateStep() {
            // decode
            bool programIsValid;
            X86Instruction[] trailProgram = decodeProgram(instructionEnumeration, out programIsValid);
            
            if( programIsValid ) {
                // run the target program and the trailProgram and compare results

                IList<X86ExecutionContext> executionContextsToCheck = new List<X86ExecutionContext>();
                executionContextsToCheck.Add(new X86ExecutionContext(8, 8));
                /*
                executionContextsToCheck.Add(new ExecutionContext(8, 8));
                executionContextsToCheck[0].integerRegisters[0] = 3;
                executionContextsToCheck[0].integerRegisters[1] = 5;
                executionContextsToCheck[1].integerRegisters[0] = 4;
                executionContextsToCheck[1].integerRegisters[1] = 5;
                */
                executionContextsToCheck[0].vectorRegisters[0] = new float[4]{1.0f, 0.1f, 0.9f, 3.76f};

                bool equalResultForAllContext = true;

                foreach( var executionCtxI in executionContextsToCheck ) {
                    X86ExecutionContext execCtxTarget = executionCtxI.deepClone();
                    interpret(execCtxTarget, targetProgram);

                    X86ExecutionContext execCtxTrail = executionCtxI.deepClone();
                    interpret(execCtxTrail, trailProgram);

                    /*if( execCtxTarget.integerRegisters[0] != execCtxTrail.integerRegisters[0] || execCtxTarget.integerRegisters[1] != execCtxTrail.integerRegisters[1] ) {
                        equalResultForAllContext = false;
                        break;
                    }*/

                    if( execCtxTarget.vectorRegisters[0][0] != execCtxTrail.vectorRegisters[0][0] ||
                        execCtxTarget.vectorRegisters[0][1] != execCtxTrail.vectorRegisters[0][1] ||
                        execCtxTarget.vectorRegisters[0][2] != execCtxTrail.vectorRegisters[0][2] ||
                        execCtxTarget.vectorRegisters[0][3] != execCtxTrail.vectorRegisters[0][3]
                    ) {

                        equalResultForAllContext = false;
                        break;
                    }
                }

                if( equalResultForAllContext ) {
                    int here = 5;
                    throw new Exception("FOUND");
                }
                
            }

            // increment and carry increment

            if( false )  Console.WriteLine("enumBefore  {0} {1} {2} {3}     {4} {5} {6} {7}  ", instructionEnumeration[0], instructionEnumeration[1], instructionEnumeration[2], instructionEnumeration[3], instructionEnumeration[4], instructionEnumeration[5], instructionEnumeration[6], instructionEnumeration[7] );


            /* commented because buggy and broken
            var typeOfFistInstruction = decodeInstructionType(instructionEnumeration[1]);
            
            // special case to speed up search
            // we do this to not enumerate the immediat2 value of the first instruction
            if( Instruction.doesInstructionNeedImmediate2(typeOfFistInstruction) ) {
                instructionEnumeration[0]++;
            }
            else {
                instructionEnumeration[0] = maximalValue[0];
            }
            
            
            for (int enumerationIdx = 0; enumerationIdx < instructionEnumeration.Length; enumerationIdx++) {
                if (instructionEnumeration[enumerationIdx] >= maximalValue[enumerationIdx]) {
                    instructionEnumeration[enumerationIdx] = 0;
                    instructionEnumeration[enumerationIdx + 1]++;

                    int idxOfNextEncoding = (enumerationIdx + 1);
                    bool nextEncodingIsImmediate2 = (idxOfNextEncoding % widthOfInstructionEncoding) == 0; // is the enumeration encoding a immediate2
                    var typeOfInstructionOfNextEncoding = decodeInstructionType(instructionEnumeration[idxOfNextEncoding + 1]);

                    if( nextEncodingIsImmediate2 ) {
                        // we do this because we have to speed up the search over the immediate2 values for encodings which don't use it (which are the mayority of instructions)

                        if( Instruction.doesInstructionNeedImmediate2(typeOfInstructionOfNextEncoding) ) {
                            instructionEnumeration[idxOfNextEncoding]++;
                        }
                        else {
                            instructionEnumeration[idxOfNextEncoding] = maximalValue[idxOfNextEncoding];
                        }
                    }
                }
                else {
                    break;
                }
            }*/

            for( int instructionIdx = 0; instructionIdx < 3; instructionIdx++ ) {
                int enumerationIdx = instructionIdx * widthOfInstructionEncoding;

                int idxOfImmediate2Encoding = enumerationIdx+0;
                int idxOfTypeEncoding = enumerationIdx+1;
                int idxOfDestEncoding = enumerationIdx+2;
                int idxOfAEncoding = enumerationIdx+3;

                var typeOfInstruction = decodeInstructionType(instructionEnumeration[idxOfTypeEncoding]);

                if( X86Instruction.doesInstructionNeedImmediate2(typeOfInstruction) ) {
                    instructionEnumeration[idxOfImmediate2Encoding]++;

                    // check if we don't need to carry
                    if( instructionEnumeration[idxOfImmediate2Encoding] < maximalValue[idxOfImmediate2Encoding] ) {
                        break;
                    }

                    // we need to carry
                    instructionEnumeration[idxOfImmediate2Encoding] = 0;
                    instructionEnumeration[idxOfTypeEncoding]++;
                }
                else {
                    instructionEnumeration[idxOfTypeEncoding]++;
                }

                // check if we don't need to carry for the type
                if( instructionEnumeration[idxOfTypeEncoding] < maximalValue[idxOfTypeEncoding] ) {
                    break;
                }


                // we need to carry
                instructionEnumeration[idxOfTypeEncoding] = 0;
                instructionEnumeration[idxOfDestEncoding]++;

                if( instructionEnumeration[idxOfDestEncoding] < maximalValue[idxOfDestEncoding] ) {
                    break;
                }

                // we need to carry
                instructionEnumeration[idxOfDestEncoding] = 0;
                instructionEnumeration[idxOfAEncoding]++;

                
                
                if( instructionEnumeration[idxOfAEncoding] < maximalValue[idxOfAEncoding] ) {
                    break;
                }

                // we need to carry
                instructionEnumeration[idxOfAEncoding] = 0;

                // if we are here we carry into the next instruction
            }

            if( false )  Console.WriteLine("enumAfter  {0} {1} {2} {3}     {4} {5} {6} {7}  ", instructionEnumeration[0], instructionEnumeration[1], instructionEnumeration[2], instructionEnumeration[3], instructionEnumeration[4], instructionEnumeration[5], instructionEnumeration[6], instructionEnumeration[7] );


            // check for wrap around
            bool allZero = true;
            for (int enumerationIdx = 0; enumerationIdx < instructionEnumeration.Length; enumerationIdx++) { 
                if( instructionEnumeration[enumerationIdx] != 0 ) {
                    allZero = false;
                    break;
                }
            }

            if( allZero ) { // if this is true then we are finished with the enumeration of the program with the length
                int here5605 = 5;
                throw new Exception("WRAP AROUND");
            }

        }

        static X86Instruction[] decodeProgram(int[] encoding, out bool programIsValid) {
            programIsValid = false;

            int numberOfInstructions = encoding.Length / widthOfInstructionEncoding;

            X86Instruction[] instructions = new X86Instruction[numberOfInstructions];
            for( int instructionIdx = 0; instructionIdx < numberOfInstructions; instructionIdx++ ) {
                instructions[instructionIdx] = new X86Instruction();
            }

            for( int instructionIdx = 0; instructionIdx < numberOfInstructions; instructionIdx++ ) {
                instructions[instructionIdx].immediate2 = encoding[instructionIdx * widthOfInstructionEncoding + 0];
                instructions[instructionIdx].type = decodeInstructionType(encoding[instructionIdx * widthOfInstructionEncoding + 1]);
                
                // special case to speed up search, should never happen
                Debug.Assert( !(!X86Instruction.doesInstructionNeedImmediate2(instructions[instructionIdx].type) && encoding[instructionIdx * widthOfInstructionEncoding + 0] != 0 ) );
                
                instructions[instructionIdx].dest = encoding[instructionIdx * widthOfInstructionEncoding + 2];

                // special case to speed up search
                if( !X86Instruction.doesInstructionNeedExtendedImmediate(instructions[instructionIdx].type) && encoding[instructionIdx * widthOfInstructionEncoding + 3] >= 8 ) {
                    return null;
                }

                instructions[instructionIdx].a = encoding[instructionIdx * widthOfInstructionEncoding + 3];
            }

            if( false )  Console.WriteLine("{0}.{1}  {2}.{3}  {4}.{5}", instructions[0].type, instructions[0].immediate2,   instructions[1].type, instructions[1].immediate2,   instructions[2].type, instructions[2].immediate2);

            programIsValid = true;

            return instructions;
        }

        const int widthOfInstructionEncoding = 4;

        const int numberOfInstructions = 15;

        static X86Instruction.EnumInstructionType decodeInstructionType(int encoding) {
            switch(encoding) {
                case 0: return X86Instruction.EnumInstructionType.NOP;
                case 1: return X86Instruction.EnumInstructionType.ADD_INTCONST;
                case 2: return X86Instruction.EnumInstructionType.SUB_INTCONST;
                case 3: return X86Instruction.EnumInstructionType.MOV_INTINT;
                case 4: return X86Instruction.EnumInstructionType.EXCHANGEINT_INT;
                case 5: return X86Instruction.EnumInstructionType.ADD_INT;
                case 6: return X86Instruction.EnumInstructionType.SUB_INT;
                case 7: return X86Instruction.EnumInstructionType.MUL_INT;
                case 8: return X86Instruction.EnumInstructionType.HORIZONTALADD_FLOATVECTOR4;
                case 9: return X86Instruction.EnumInstructionType.HORIZONTALSUB_FLOATVECTOR4;
                case 10: return X86Instruction.EnumInstructionType.ADD_FLOATVECTOR4;
                case 11: return X86Instruction.EnumInstructionType.SUB_FLOATVECTOR4;
                case 12: return X86Instruction.EnumInstructionType.MUL_FLOATVECTOR4;
                case 13: return X86Instruction.EnumInstructionType.DIV_FLOATVECTOR4;
                case 14: return X86Instruction.EnumInstructionType.SHUFFLE_FLOATVECTOR4;
            }
            throw new Exception();
        }
    }
}
