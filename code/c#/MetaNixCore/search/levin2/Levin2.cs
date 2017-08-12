using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MetaNix.scheduler;
using MetaNix.framework.logging;
using MetaNix.framework.datastructures;

namespace MetaNix.search.levin2 {
    // wrapper for two parameter call with success
    static class ArrayOperationsTwoArgumentWrapper {
        public static void arrayMove(InterpreterState state, int delta, int dummy, out bool success) {
            ArrayOperations.arrayMove(state, delta);
            success = true;
        }

        public static void arrayRemove(InterpreterState state, int dummy0, int dummy1, out bool success) {
            ArrayOperations.arrayRemove(state, out success);
        }

        public static void arrayCompareWithRegister(InterpreterState state, int register, int dummy0, out bool success) {
            ArrayOperations.arrayCompareWithRegister(state, register, out success);
        }
    }

    // wrappr for two parameters call with cuccess
    static class OperationsTwoArgumentWrapper {
        public static void jumpIfNotFlag(InterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jumpIfNotFlag(state, delta);
            success = true;
        }

        public static void jumpIfFlag(InterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jumpIfFlag(state, delta);
            success = true;
        }

        public static void jump(InterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jump(state, delta);
            success = true;
        }

    }

    public static class Operations {
        public static void @return(InterpreterState state, out bool success) {
            state.instructionPointer = state.topCallstack.pop(out success);
        }


        public static void jumpIfNotFlag(InterpreterState state, int delta) {
            if (!state.comparisionFlag) {
                state.instructionPointer += delta;
            }
            state.instructionPointer++;
        }

        public static void jumpIfFlag(InterpreterState state, int delta) {
            if (state.comparisionFlag) {
                state.instructionPointer += delta;
            }
            state.instructionPointer++;
        }
        
        public static void jump(InterpreterState state, int delta) {
            state.instructionPointer += delta;
            state.instructionPointer++;
        }
        
        public static void movImmediate(InterpreterState state, int register, int value, out bool success) {
            state.registers[register] = value;

            state.instructionPointer++;
            success = true;
        }



        public static void call(InterpreterState state, int delta) {
            state.topCallstack.push(state.instructionPointer + 1);
            state.instructionPointer += delta;
            state.instructionPointer++;
        }

        public static void compare(InterpreterState state, int register, int value) {
            state.comparisionFlag = state.registers[register] == value;
            state.instructionPointer++;
        }

        public static void add(InterpreterState state, int register, int value) {
            state.registers[register] += value;
            state.instructionPointer++;
        }


        public static void mulRegisterImmediate(InterpreterState state, int register, int value) {
            state.registers[register] *= value;
            state.instructionPointer++;
        }

        public static void mulRegisterRegister(InterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] *= state.registers[registerSource];
            state.instructionPointer++;
        }

        public static void addRegisterRegister(InterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] += state.registers[registerSource];
            state.instructionPointer++;
        }

        public static void subRegisterRegister(InterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] -= state.registers[registerSource];
            state.instructionPointer++;
        }

        // add by checking flag
        public static void addFlag(InterpreterState state, int register, int value) {
            if (state.comparisionFlag) {
                state.registers[register] += value;
            }
            state.instructionPointer++;
        }
        
        // interprets the value as binary (zero is false and all else is true) and negates it
        public static void binaryNegate(InterpreterState state, int register) {
            state.registers[register] = (state.registers[register] == 0) ? 1 : 0;
            state.instructionPointer++;
        }

        // random number up to the value of the register
        public static void random(InterpreterState state, int destRegister, int register, out bool success) {
            if( state.registers[register] <= 0 ) {
                success = false;
                return;
            }

            state.registers[destRegister] = state.rng.Next(state.registers[register]);
            success = true;
        }
    }

    public static class ArrayOperations {
        public static void arrayMove(InterpreterState state, int delta) {
            state.arrayState.index += delta;
            state.instructionPointer++;
        }

        public static void arrayRemove(InterpreterState state, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }

            state.arrayState.array.removeAt(state.arrayState.index);

            state.instructionPointer++;

            success = true;
        }

        public static void arrayCompareWithRegister(InterpreterState state, int register, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }
            state.comparisionFlag = state.registers[register] == state.arrayState.array[state.arrayState.index];
            state.instructionPointer++;
            success = true;
        }


        public static void insert(InterpreterState state, int register, out bool success) {
            if (state.arrayState == null ) {
                success = false;
                return;
            }

            if( state.arrayState.index < 0 || state.arrayState.index > state.arrayState.array.count ) {
                success = false;
                return;
            }

            int valueToInsert = state.registers[register];
            state.arrayState.array.insert(state.arrayState.index, valueToInsert);

            state.instructionPointer++;
            success = true;
        }

        // array is ignored
        public static void setIdx(InterpreterState state, int array, int index, out bool success) {
            if (state.arrayState == null ) {
                success = false;
                return;
            }

            if( index == -1 ) { // end of array, so insertion appends an element
                state.arrayState.index = state.arrayState.array.count;
            }
            else {
                state.arrayState.index = index;
            }

            state.instructionPointer++;
            success = true;
        }

        // moves the array index by delta and stores in the flag if the index is still in bound after moving
        public static void idxFlag(InterpreterState state, int array, int delta, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            if( array == 0 ) {
                state.arrayState.index += delta;
                state.comparisionFlag = state.arrayState.isIndexValid; // store the validity of the arrayIndex in flag
            }
            else {
                success = false;
                return;
            }


            state.instructionPointer++;
            success = true;
        }

        // /param array is the index of the array (currently ignored)
        public static void valid(InterpreterState state, int array, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.comparisionFlag = state.arrayState.isIndexValid;

            state.instructionPointer++;
            success = true;
        }

        public static void read(InterpreterState state, int array, int register, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }
            
            state.registers[register] = state.arrayState.array[state.arrayState.index];
            
            state.instructionPointer++;
            success = true;
        }

        public static void idx2Reg(InterpreterState state, int array, int register, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.registers[register] = state.arrayState.index;

            state.instructionPointer++;
            success = true;
        }
        

        public static void arrayMovToArray(InterpreterState state, int array, int register, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }
            state.arrayState.array[state.arrayState.index] = state.registers[register];
            state.instructionPointer++;
            success = true;
        }

        // macro-arrAdvanceOrExit
        // advance array index and reset and return/terminate if its over
        // else jump relative
        public static void macroArrayAdvanceOrExit(InterpreterState state, int ipDelta, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.arrayState.index++;
            bool isIndexValid = state.arrayState.isIndexValid;
            if( !isIndexValid ) {
                state.arrayState.index = 0;
                Operations.@return(state, out success);
                return;
            }

            state.instructionPointer++;
            state.instructionPointer += ipDelta;

            success = true;
        }

        // macro-arrNotEndOrExit
        // advance array index and reset and return/terminate if its over
        // else jump relative
        public static void macroArrayNotEndOrExit(InterpreterState state, int ipDelta, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }
            
            bool isIndexValid = state.arrayState.isIndexValid;
            if (!isIndexValid) {
                state.arrayState.index = 0;
                Operations.@return(state, out success);
                return;
            }

            state.instructionPointer++;
            state.instructionPointer += ipDelta;

            success = true;
        }

        public static void length(InterpreterState state, int destRegister, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.registers[destRegister] = state.arrayState.array.count;

            success = true;
        }
    }


    // used to store arguments and call a static operation functions with two arguments
    public class TwoArgumentOperationCaller {
        public TwoArgumentOperationCaller(FunctionDelegateType function, int value0, int value1) {
            this.function = function;
            this.value0 = value0;
            this.value1 = value1;
        }

        public delegate void FunctionDelegateType(InterpreterState state, int value0, int value1, out bool success);
        
        public void call(InterpreterState state, out bool success) {
            function(state, value0, value1, out success);
        }

        FunctionDelegateType function;
        int value0;
        int value1;
    }


    

    public class InterpreterState {
        // used to patch additional instructions into the running program
        public delegate void AdditionalOperationsDelegateType(InterpreterState state, out bool success);
        public IDictionary<uint, AdditionalOperationsDelegateType> additionalOperations = new Dictionary<uint, AdditionalOperationsDelegateType>();

        public ArrayState arrayState;

        public Random rng = new Random();

        // each callstack is valid just for one function
        // but we need multiple callstacks because functions can invoke other functions
        public IList<IntStack> callstacks = new List<IntStack>();

        public IntStack topCallstack {
            get {
                return callstacks[callstacks.Count - 1];
            }
        }

        public void reset() {
            for (int iRegister = 0; iRegister < registers.Length; iRegister++) {
                registers[iRegister] = 0;
            }
            comparisionFlag = false;
            instructionPointer = 0;
            if (arrayState != null) {
                arrayState.index = 0;
            }

            callstacks = new List<IntStack> { new IntStack() };
            topCallstack.setTo(new int[] { 0x0000ffff }); // special value to indicate returning of program
        }

        public int[] registers;
        public bool comparisionFlag;
        public int instructionPointer;
    }

    // implementations can implement different kinds of array like behaviour
    public interface IAbstractArray<Type> {
        void removeAt(int idx);
        void insert(int idx, Type value);

        void clear();
        void append(Type value);

        Type this[int idx] {
            get;
            set;
        }

        int count {
            get;
        }
    }

    public class ListArray<Type> : IAbstractArray<Type> {
        public int count => arr.Count;

        public Type this[int idx] {
            get => arr[idx];
            set => arr[idx] = value;
        }

        public Type at(int idx) {
            return arr[idx];
        }

        public void insert(int idx, Type value) {
            arr.Insert(idx, value);
        }

        public void removeAt(int idx) {
            arr.RemoveAt(idx);
        }

        public void setAt(int idx, Type value) {
            arr[idx] = value;
        }

        public void clear() {
            arr.Clear();
        }

        public void append(Type value) {
            arr.Add(value);
        }

        IList<Type> arr = new List<Type>();
    }

    // preserves relative instruction of jump instructions by default
    public class InstructionOffsetPreservingArray : IAbstractArray<int> {
        public int this[int idx] {
            get => arr[idx];
            set => arr[idx] = value;
        }

        public int count => arr.Count;

        public void append(int value) {
            arr.Add(value);
        }

        public void clear() {
            arr.Clear();
        }

        public void insert(int idx, int value) {
            insertAtOffsetPreserving(idx, value);
        }

        public void removeAt(int idx) {
            removeAtOffsetPreserving(idx);
        }

        public void insertAtOffsetPreserving(int idx, int value) {
            adjustControlflowOffsetsForInstructionAtIndex(idx, +1);
            arr.Insert(idx, value);
        }
        

        public void removeAtOffsetPreserving(int idx) {
            adjustControlflowOffsetsForInstructionAtIndex(idx, -1);
            arr.RemoveAt(idx);
        }

        // /param delta the delta of the relative offset
        void adjustControlflowOffsetsForInstructionAtIndex(int idx, int delta) {
            
            for (int i = 0; i < arr.Count; i++) {
                if( i == idx )   continue;

                uint instructionWithoutRelative;
                int relative;
                InstructionInterpreter.decodeInstruction((uint)arr[i], out instructionWithoutRelative, out relative);

                bool isInstructionWithRelativeOffset = instructionWithoutRelative <= InstructionInterpreter.NUMBEROFCONTROLFLOWINSTRUCTIONS;
                if (!isInstructionWithRelativeOffset)    continue; // if it is not a control flow altering instruction we don't have to change it

                int relativeDestination = i + relative;

                bool overlaps = false;

                if( relative < 0 )   overlaps = idx > i && i < relativeDestination;
                else                 overlaps = i < idx && idx < relativeDestination;

                if (!overlaps)    continue; // if the range of the jump doesn't overlap with the index we don't hvae to modfy it

                int newRelative = relative + delta;

                arr[i] = (int)InstructionInterpreter.convInstructionAndRelativeToInstruction(instructionWithoutRelative, relative);
            }
        }



        IList<int> arr = new List<int>();
    }


    public class ArrayState {
        public int index;
        public IAbstractArray<int> array = new ListArray<int>();

        public bool isIndexValid {
            get {
                return index >= 0 && index < array.count;
            }
        }
    }


    sealed public class InstructionInfo {
        static string[] hardcodedSingleInstructions = {
            "ret",
            "macro-arrAdvanceOrExit $rel",
            "macro-arrNotEndOrExit $rel",
            "jmp $rel",
            "jmpIfNotFlag $rel",
            "call $rel",

            "arrayIdx arr0 -1", 
            "arrayIdx arr0 +1",
            "arrayRemove",
            "arrayCompare reg0",
            "arrayCompare reg1",
            
            "arrayInsert reg0",
            "arrayInsert reg1",
            "arrayInsert reg2",
            "arraySetIdx arr0 0",
            "arraySetIdx arr0 -1",
            "arrayIdxFlag arr0 +1",
            "arrayValid arr0",
            "arrayRead arr0 reg0",
            "arrayIdx2Reg arr0 reg0",
            "mov reg0, 0",
            "mov reg0, 1",
            "mov reg0, 3",
            "arrayMov arr0 reg0",

            "mul reg0, -1",
            "binaryNeg reg0",

            
            "mov reg1, 0",
            "mov reg1, 1",
            "mov reg1, 3",
            "arrayRead arr0 reg1",
            
            "mul reg0, reg1",
            "add reg0, reg1",
            "arrayMov arr0 reg1",
            "sub reg1, reg0",
           
            // TODO< remaining instructions >
        };

        public static uint getNumberOfHardcodedSingleInstructions() {
            return (uint)hardcodedSingleInstructions.Length;
        }

        public static uint getNumberOfInstructions() {
            return (uint)hardcodedSingleInstructions.Length + 6;
        }

        public static string getMemonic(uint instruction) {
            // instruction is a 16 bit number where the high 7 bits are the jump offset for jump instructions
            uint instructionWithoutRelative = instruction & 0x1ff;
            int relative = (int)(instruction >> 9) - ((1 << 7) - 1) / 2;

            if (instructionWithoutRelative <= hardcodedSingleInstructions.Length ) {
                return hardcodedSingleInstructions[(int)instruction].Replace("$rel", relative.ToString());
            }
            
            // if we are here we have instrution with hardcoded parameters

            int baseInstruction = hardcodedSingleInstructions.Length;
            Debug.Assert(instruction > baseInstruction);
            int currentBaseInstruction = baseInstruction;

            // compare constant
            if (instruction <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                return "cmp reg0 0";
            }

            currentBaseInstruction += 1;

            // add register constant
            if (instruction <= currentBaseInstruction + 2) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the pool

                if( subInstruction == 0 ) {
                    return "add reg0 -1";
                }
                else if (subInstruction == 1) {
                    return "add reg0 2";
                }

            }

            currentBaseInstruction += 2;

            // addFlag reg0 constant
            if (instruction <= currentBaseInstruction + 1) {
                // currently just add 1 to reg0
                return "addFlag reg0 1";
            }

            currentBaseInstruction += 1;
            
            // indirect table call
            if (instruction <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                return String.Format("call [indirect0]");
            }

            currentBaseInstruction += 1;




            // unknown instruction
            return "<unknown>";
        }
    }

    sealed public class Instruction {
        public Instruction(uint code) {
            this.code = code;
        }

        public uint code; // numeric code of the instruction
    }

    sealed public class InstructionInterpreter {
        // conversion helper
        public static uint convInstructionAndRelativeToInstruction(uint instruction, int relative) {
            uint relativeAsUint = (uint)(relative + (1 << (7 - 1)));
            uint resultInstruction = instruction | (relativeAsUint << 9);

            
            int relativeDecoded = (int)(resultInstruction >> 9) - ((1 << 7)) / 2;
            Debug.Assert(relativeDecoded == relative);

            return resultInstruction;
        }

        // conversion helper
        public static void decodeInstruction(uint instr, out uint instructionWithoutRelative, out int relative) {
            // instruction is a 16 bit number where the high 7 bits are the jump offset for jump instructions
            instructionWithoutRelative = instr & 0x1ff;
            relative = (int)(instr >> 9) - ((1 << 7)) / 2;
        }


        // checks if the program was terminated successfully by returning to the global caller
        public static bool isTerminating(InterpreterState interpreterState, uint instruction) {
            // return
            if( interpreterState.topCallstack.top == 0x0000ffff && instruction == INSTRUCTION_RET)   return true;


            if(isMacroArrayAdvanceOrExit(instruction) ) {
                bool atLastIndex = interpreterState.arrayState.index >= interpreterState.arrayState.array.count - 1;
                
                return atLastIndex;
            }

            return false;
        }

        static bool isMacroArrayAdvanceOrExit(uint instruction) {
            uint instructionWithoutRelative = instruction & 0x1ff;

            return instructionWithoutRelative == 1 || instructionWithoutRelative == 2;
        }



        public const uint INSTRUCTION_RET = 0; // instruction for return

        public const uint NUMBEROFCONTROLFLOWINSTRUCTIONS = 6;
        
        // \param indirectCall is not -1 if the instruction is an indirect call to another function
        public static void dispatch(InterpreterState interpreterState, Instruction instr, out bool success, out int indirectCall) {
            indirectCall = -1;
            
            uint instructionWithoutRelative;
            int relative;
            decodeInstruction(instr.code, out instructionWithoutRelative, out relative);
            
            switch (instructionWithoutRelative) {
                case INSTRUCTION_RET: Operations.@return(interpreterState, out success); return;
                case 1: ArrayOperations.macroArrayAdvanceOrExit(interpreterState, relative, out success); return;
                case 2: ArrayOperations.macroArrayNotEndOrExit(interpreterState, relative, out success); return;
                case 3: Operations.jump(interpreterState, relative); success = true; return;
                case 4: Operations.jumpIfNotFlag(interpreterState, relative); success = true; return;
                case 5: Operations.call(interpreterState, relative); success = true; return;

                case 6: ArrayOperationsTwoArgumentWrapper.arrayMove(interpreterState, -1, int.MaxValue, out success); return;
                case 7: ArrayOperationsTwoArgumentWrapper.arrayMove(interpreterState, 1, int.MaxValue, out success); return;
                case 8: ArrayOperationsTwoArgumentWrapper.arrayRemove(interpreterState, int.MaxValue, int.MaxValue, out success); return;
                case 9: ArrayOperationsTwoArgumentWrapper.arrayCompareWithRegister(interpreterState, 0, int.MaxValue, out success); return;
                case 10: ArrayOperationsTwoArgumentWrapper.arrayCompareWithRegister(interpreterState, 1, int.MaxValue, out success); return;
                
                case 11: ArrayOperations.insert(interpreterState, /*reg*/0, out success); return;
                case 12: ArrayOperations.insert(interpreterState, /*reg*/1, out success); return;
                case 13: ArrayOperations.insert(interpreterState, /*reg*/2, out success); return;
                case 14: ArrayOperations.setIdx(interpreterState, 0, 0, out success); return;
                case 15: ArrayOperations.setIdx(interpreterState, 0, -1, out success); return; // -1 is end of array
                case 16: ArrayOperations.idxFlag(interpreterState, 0, 1, out success); return; // TODO< should be an intrinsic command which gets added by default >
                case 17: ArrayOperations.valid(interpreterState, /*array*/0, out success); return;
                case 18: ArrayOperations.read(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 19: ArrayOperations.idx2Reg(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 20: Operations.movImmediate(interpreterState, /*register*/0, 0, out success); return;
                case 21: Operations.movImmediate(interpreterState, /*register*/0, 1, out success); return;
                case 22: Operations.movImmediate(interpreterState, /*register*/0, 3, out success); return;
                case 23: ArrayOperations.arrayMovToArray(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 24: Operations.mulRegisterImmediate(interpreterState, /*register*/0, -1); success = true; return;
                case 25: Operations.binaryNegate(interpreterState, /*register*/0); success = true; return;
                case 26: ArrayOperations.macroArrayAdvanceOrExit(interpreterState, -4, out success); return;
                case 27: Operations.movImmediate(interpreterState, /*register*/1, 0, out success); return;
                case 28: Operations.movImmediate(interpreterState, /*register*/1, 1, out success); return;
                case 29: Operations.movImmediate(interpreterState, /*register*/1, 3, out success); return;
                case 30: ArrayOperations.read(interpreterState, /*array*/0, /*register*/1, out success); return;
                
                case 31: Operations.mulRegisterRegister(interpreterState, 0, 1); success = true; return;
                case 32: Operations.addRegisterRegister(interpreterState, 0, 1); success = true; return;
                case 33: ArrayOperations.arrayMovToArray(interpreterState, /*array*/0, /*register*/1, out success); return;
                case 34: Operations.subRegisterRegister(interpreterState, 1, 0); success = true; return;
                
                case 35: Operations.random(interpreterState, 0, 0, out success); return;
                case 36: ArrayOperations.length(interpreterState, /*destRegister*/0, out success); return;
            }

            // if we are here we have instrution with hardcoded parameters

            uint baseInstruction = InstructionInfo.getNumberOfHardcodedSingleInstructions();
            Debug.Assert(instructionWithoutRelative >= baseInstruction);
            int currentBaseInstruction = (int)baseInstruction;

            // compare constant
            if (instructionWithoutRelative <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                Operations.compare(interpreterState, /*register*/0, 0);
                success = true;
                return;
            }

            currentBaseInstruction += 1;





            // add register constant
            if (instructionWithoutRelative <= currentBaseInstruction + 3) {
                int subInstruction = (int)instructionWithoutRelative - currentBaseInstruction; // which instruction do we choose from the pool

                if (subInstruction == 0) {
                    Operations.add(interpreterState, /*register*/0, -1);
                }
                else if (subInstruction == 1) {
                    Operations.add(interpreterState, /*register*/0, 2);
                }
                else if (subInstruction == 2) {
                    Operations.add(interpreterState, /*register*/1, -1);
                }

            }

            currentBaseInstruction += 3;


            // addFlag reg0 constant
            if (instructionWithoutRelative <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                Operations.addFlag(interpreterState, /*register*/0, 1);
                success = true;
                return;
            }

            currentBaseInstruction += 1;




            
            
            

            // indirect table call
            if (instructionWithoutRelative <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                indirectCall = 0;
                success = true;
                return;
            }

            currentBaseInstruction += 1;

            // additional instructions
            InterpreterState.AdditionalOperationsDelegateType additionalOperationDelegate;
            if( interpreterState.additionalOperations.TryGetValue(instructionWithoutRelative, out additionalOperationDelegate) ) {
                additionalOperationDelegate(interpreterState, out success);
                return;
            }


            // unknown instruction
            success = false;
        }


    }


    public class Interpreter {
        public class InterpretArguments {
            public uint maxNumberOfRetiredInstructions;
            public uint[] program;
            public uint lengthOfProgram; // length of the enabled program

            public InterpreterState interpreterState;
            public bool debugExecution;

        }

        public void interpret(InterpretArguments arguments, out bool programExecutedSuccessful, out bool hardExecutionError) {
            programExecutedSuccessful = false;
            hardExecutionError = false;

            if( arguments.lengthOfProgram >= 4 ) {
                if( arguments.program[0] == 3 && arguments.program[1] == 62 && arguments.program[2] == 2 && arguments.program[3] == 31 ) {
                    int debugHere = 5;
                }
            }

            for (int instructionsRetired = 0; instructionsRetired < arguments.maxNumberOfRetiredInstructions; instructionsRetired++) {
                bool instructionPointerValid = arguments.interpreterState.instructionPointer >= 0 && arguments.interpreterState.instructionPointer < arguments.lengthOfProgram;
                if (!instructionPointerValid) {
                    hardExecutionError = true;
                    break;
                }

                uint currentInstruction = arguments.program[arguments.interpreterState.instructionPointer];

                if (arguments.debugExecution) {
                    Console.WriteLine("program=");

                    throw new NotImplementedException(); // TODO< logger >
                    //Program2.debug(null, arguments.program);

                    Console.WriteLine("ip={0}", arguments.interpreterState.instructionPointer);
                    Console.Write("arr=");

                    throw new NotImplementedException(); // TODO< logger >
                    //Program2.debug(null, arguments.interpreterState.arrayState.array);
                    Console.WriteLine("array index={0}", arguments.interpreterState.arrayState.index);
                }

                if (InstructionInterpreter.isTerminating(arguments.interpreterState, currentInstruction)) {
                    programExecutedSuccessful = true; // the program executed successfully only if we return
                    break;
                }

                bool instructionExecutedSuccessfull;
                int indirectCallIndex;
                InstructionInterpreter.dispatch(arguments.interpreterState, new Instruction(currentInstruction), out instructionExecutedSuccessfull, out indirectCallIndex);
                if (!instructionExecutedSuccessfull) {
                    hardExecutionError = true;
                    break;
                }

                if (indirectCallIndex != -1) {
                    // an indirect call is a call to an (interpreted) function

                    // TODO< try to dispatch indirect call >

                    // for now we ignore it
                }
            }

            if (hardExecutionError) {
                programExecutedSuccessful = false;
            }
        }
    }


    public class TrainingSample {
        public IList<int> questionArray; // disabled if null
        public int?[] questionRegisters;
        public int? questionArrayIndex;

        public IList<int> answerArray; // disabled if null
        public int? answerArrayIndex; // which array index should the array have whe the function returns, disabled if null
        public int?[] answerRegisters;
        public bool? answerFlag;
    }

    // keeps track of how many iterations to try for the iteration
    // how long the tried program is
    // up to which length is searched, etc
    public class LevinSearchContext {
        public void searchIteration(out bool searchCompleted) {
            searchCompleted = false;
            searchIterations++;

            // are just the lookup values we still have to lookup
            instructionIndices = programSampler.sampleProgram((int)numberOfInstructionsToEnumerate);
            
            uint[] sampledProgram = instructionIndices.Select(v => instructionIndexToInstruction[v]).ToArray();

            // copy
            sampledProgram.CopyTo(program, 0);
            program[privateNumberOfInstructions - 1] = InstructionInterpreter.INSTRUCTION_RET; // overwrite last instruction with ret so it terminates always

            // append parent program if there is one
            if( parentProgram != null ) {
                parentProgram.CopyTo(program, privateNumberOfInstructions);
            }


            bool trainingSamplesTestedSuccessful = true;

            foreach (TrainingSample currentTrainingSample in trainingSamples) {
                // reset interpreter state
                interpreterArguments.interpreterState.reset();

                // set question states
                for (int i = 0; i < currentTrainingSample.questionRegisters.Length; i++) {
                    if (currentTrainingSample.questionRegisters[i].HasValue) {
                        interpreterArguments.interpreterState.registers[i] = currentTrainingSample.questionRegisters[i].Value;
                    }
                }

                interpreterArguments.interpreterState.arrayState.array.clear();
                for (int i = 0; i < currentTrainingSample.questionArray.Count; i++) {
                    interpreterArguments.interpreterState.arrayState.array.append(currentTrainingSample.questionArray[i]);
                }

                if(currentTrainingSample.questionArrayIndex.HasValue) {
                    interpreterArguments.interpreterState.arrayState.index = currentTrainingSample.questionArrayIndex.Value;
                }

                bool
                    programExecutedSuccessful,
                    hardExecutionError;

                // * interpret
                interpreter.interpret(interpreterArguments, out programExecutedSuccessful, out hardExecutionError);

                if (!programExecutedSuccessful) {
                    trainingSamplesTestedSuccessful = false;
                    break;
                }

                // we are here if the program executed successfully and if it returned something


                // compare result
                if (
                    currentTrainingSample.answerArray != null &&
                    !ListHelpers.isSame(interpreterArguments.interpreterState.arrayState.array, currentTrainingSample.answerArray)
                ) {
                    trainingSamplesTestedSuccessful = false;
                    break;
                }

                if (currentTrainingSample.answerArrayIndex.HasValue && interpreterArguments.interpreterState.arrayState.index != currentTrainingSample.answerArrayIndex) {
                    trainingSamplesTestedSuccessful = false;
                    break;
                }

                if( currentTrainingSample.answerRegisters != null ) {
                    for(int i = 0; i < currentTrainingSample.answerRegisters.Length; i++ ) {
                        if(currentTrainingSample.answerRegisters[i].HasValue && currentTrainingSample.answerRegisters[i] != interpreterArguments.interpreterState.registers[i] ) {
                            trainingSamplesTestedSuccessful = false;
                            break;
                        }
                    }
                }

                if(currentTrainingSample.answerFlag.HasValue && currentTrainingSample.answerFlag != interpreterArguments.interpreterState.comparisionFlag) {
                    trainingSamplesTestedSuccessful = false;
                    break;
                }

            }

            if (!trainingSamplesTestedSuccessful) {
                return; // try next program
            }

            // ** search was successful
            searchCompleted = true;
            return;
        }

        // 
        public void initiateSearch(SparseArrayProgramDistribution programDistribution, uint enumerationMaxProgramLength) {
            searchIterations = 0;

            this.programDistribution = programDistribution;
            this.enumerationMaxProgramLength = enumerationMaxProgramLength;

            privateNumberOfInstructions = 1+/* with RET*/1;

            numberOfInstructionsToEnumerate = 6; // we enumerate at maximum with just 6 instructions

            programSampler = new ProgramSampler(programDistribution, numberOfInstructionsToEnumerate, instructionsetCount);
            programSampler.setInstructionsetCount(instructionsetCount);
            
            uint programMaxSize = 512;
            program = new uint[programMaxSize];
            interpreterArguments.program = program;
            
            reinintateSearch();
        }

        public bool canIncreaseProgramsize() {
            return privateNumberOfInstructions + 1 <= enumerationMaxProgramLength;
        }

        // tries to increase the programsize by one and reinitates the search
        public void increaseProgramsizeAndReinitiate() {
            Ensure.ensureHard(privateNumberOfInstructions+1 <= enumerationMaxProgramLength);
            privateNumberOfInstructions++;

            reinintateSearch();
        }

        void reinintateSearch() {
            // TODO< -1 depends on if we add an RET by default or not >
            Ensure.ensureHard(privateNumberOfInstructions >= 1); // depends on if we add an ret or not, TODO< add condition >
            uint enumeratedProgramLength = (uint)privateNumberOfInstructions - 1;

            Ensure.ensureHard(privateNumberOfInstructions >= 1);
            interpreterArguments.lengthOfProgram = (uint)privateNumberOfInstructions;
            
            remainingIterationsForCurrentProgramLength = (long)(exhausiveSearchFactor * Math.Pow(instructionsetCount, enumeratedProgramLength));
        }

        public int numberOfInstructions {
            get {
                return privateNumberOfInstructions;
            }
        }

        public ulong searchIterations; // how many iterations were done for this search already?

        private uint instructionsetCount { get {
                return (uint)instructionIndexToInstruction.Count;
            }
        }

        // translates from instructionIndex to "real" instruction code
        public IDictionary<uint, uint> instructionIndexToInstruction = new Dictionary<uint, uint>();

        public double exhausiveSearchFactor = 2.0; // hown many times do we try a program at maximum till we give up based on the # of combinations
                                                   // can be less than 1 which favors non-exhausive search
        
        int privateNumberOfInstructions;
        uint numberOfInstructionsToEnumerate;
        uint enumerationMaxProgramLength; // maximal program length (with RET if RET is included) of enumeration

        public Interpreter.InterpretArguments interpreterArguments = new Interpreter.InterpretArguments(); // preallocated
        uint[] program; // preallocated temporary program which is composed out of the parent program and the current try
        uint[] instructionIndices; // indices of the program which were used to lookup the real instructions

        Interpreter interpreter = new Interpreter();
        ProgramSampler programSampler;
        SparseArrayProgramDistribution programDistribution;
        public long remainingIterationsForCurrentProgramLength;

        public IList<TrainingSample> trainingSamples = new List<TrainingSample>();
        public uint[] parentProgram; // can be null

        internal void biasSearchTowardProgram() {
            programDistribution.addProgram(instructionIndices);
        }
    }

    public class LevinSearchTask : MetaNix.scheduler.ITask {
        // /param doneObserable will be notified when the search is done or failed
        public LevinSearchTask(Observable obserable, string taskname) {
            this.observable = obserable;
            this.taskname = taskname;
        }

        public void processTask(Scheduler scheduler, double softTimelimitInSeconds, out EnumTaskStates taskState) {
            executiontimeSchedulingStopwatch.Restart();
            for (;;) {
                bool searchCompleted;
                timingIteration(out searchCompleted);
                if (searchCompleted) {
                    taskState = EnumTaskStates.FINISHED;
                    return;
                }

                if ((double)executiontimeSchedulingStopwatch.ElapsedMilliseconds / 1000.0 > softTimelimitInSeconds) {
                    taskState = EnumTaskStates.RUNNING;
                    return;
                }
            }

            taskState = EnumTaskStates.RUNNING;
        }

        void timingIteration(out bool searchCompleted) {
            searchCompleted = false;

            for (int timingIterationCounter = 0; timingIterationCounter < iterationGranularity; timingIterationCounter++) {
                levinSearchContext.remainingIterationsForCurrentProgramLength--;
                if (levinSearchContext.remainingIterationsForCurrentProgramLength < 0) {
                    // current length done

                    if(levinSearchContext.canIncreaseProgramsize()) {
                        observable.notify("increaseProgramsize", this, taskname);
                        levinSearchContext.increaseProgramsizeAndReinitiate();
                        return;
                    }
                    else {
                        // search failed
                        searchCompleted = true;
                        observable.notify("failed", this, taskname);
                        return;
                    }
                }

                levinSearchContext.searchIteration(out searchCompleted);
                if (searchCompleted) {
                    biasSearchTowardProgram();
                    observable.notify("success", this, taskname);
                    return;
                }
            }
        }

        private void biasSearchTowardProgram() {
            levinSearchContext.biasSearchTowardProgram();
        }

        public LevinSearchContext levinSearchContext;



        public uint iterationGranularity = 50000; // how many iterations are done after we check for the soft timelimit
        Stopwatch executiontimeSchedulingStopwatch = new Stopwatch();

        private Observable observable;
        private string taskname;
    }

    
    

    public class Program2 {
        // roll one bit to the left
        static ulong rolLeft1(ulong number) {
            ulong carryOver = number >> (64 - 1);
            ulong shiftedToLeft = number << 1;
            return carryOver | shiftedToLeft;
        }
        
        // for experimentation
        
        public static void debug<Type>(ILogger log, IEnumerable<Type> list) {
            string message = "";

            foreach (Type iValue in list) {
                message += string.Format("{0} ", iValue);
            }

            Logged logged = new Logged();
            logged.notifyConsole = Logged.EnumNotifyConsole.YES;
            logged.message = message;
            logged.origin = new string[] { "search",  "ALS", "debug" };
            logged.serverity = Logged.EnumServerity.INFO;
            log.write(logged);
        }
        
    }

    static class ListHelpers {
        public static bool isSame(IAbstractArray<int> a, IList<int> b) {
            if (a.count != b.Count)   return false;
            
            for (int i = 0; i < a.count; i++)
                if (a[i] != b[i])   return false;
            
            return true;
        }
    }
}
