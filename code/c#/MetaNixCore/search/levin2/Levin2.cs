using System;
using System.Collections.Generic;
using System.Diagnostics;

using MetaNix.datastructures.compact;
using MetaNix.scheduler;
using MetaNix.framework.logging;

namespace MetaNix.search.levin2 {
    // implementations sample the programspace acording to an distribution which is stored and updated
    interface IProgramDistribution {
        // \param propabilityVector is an array of the propability [0; 1) for each selection of an instruction
        // \param lengthOfProgram how log is the sampled program, must be <= resultInstructions.Count, can be -1 if the maximal length is allowed
        void sample(double[] propabilityVector, ref uint[] resultInstructions, uint instructionsetCount, int lengthOfProgram, Random random);

        void addProgram(uint[] instructions);

        void multiplyPropabiliyForProgram(uint[] instructions, double multiplicator);
    }

    // stores the instructions and their corresponding propabilities based on an array
    /*
     * in each level in the stored tree the propabilities and the corresponding instructions get stored for all programs which were found by the agent thus far.
     * for all other instructions which were not encountered a "shadowed" propability is stored
     * 
     */
    public class SparseArrayProgramDistribution : IProgramDistribution {
        // \param numberOfInstructions how many instructions exist for the instructionset?
        public void sample(double[] propabilityVector, ref uint[] resultInstructions, uint instructionsetCount, int lengthOfProgram, Random random) {
            // select instruction, look for a children for the node, iterate

            lengthOfProgram = lengthOfProgram == -1 ? propabilityVector.Length : lengthOfProgram;

            SparseArrayProgramDistributionTreeElement iterationElement = root;

            // if the iteration tree element is null we choose an random instruction,
            // because the tree has no preference or knowledge of the instruction distribution

            for (int i = 0; i < lengthOfProgram; i++) {
                // * calculate absolute propability sum
                double propabilityMassForCurrentSparseArrayProgramDistributionTreeElement = iterationElement != null ? iterationElement.getPropabilityMass(instructionsetCount) : instructionsetCount;
                double propabilityForInstructionByIndex = propabilityVector[i];
                double absolutePropabilitySum = propabilityMassForCurrentSparseArrayProgramDistributionTreeElement * propabilityForInstructionByIndex;

                uint selectedInstruction;
                // * select instructions
                if (iterationElement != null) {
                    selectedInstruction = iterationElement.sampleInstructionBasedOnAbsolutePropability(absolutePropabilitySum, instructionsetCount, random);
                }
                else {
                    // fast way to choose a random instruction
                    selectedInstruction = (uint)absolutePropabilitySum;
                }

                // * store instruction
                resultInstructions[i] = selectedInstruction;

                // * choose next tree element
                if (iterationElement != null) {
                    iterationElement = iterationElement.getChildrenElementByInstruction(selectedInstruction);
                }
            }
        }

        public void addProgram(uint[] instructions) {
            double defaultPropabilityOfShadowedInstruction = 1.0; // just a name for an otherwise magical value

            SparseArrayProgramDistributionTreeElement treeElementForCurrentInstruction = root;

            if (root == null) {
                root = new SparseArrayProgramDistributionTreeElement(defaultPropabilityOfShadowedInstruction);
                treeElementForCurrentInstruction = root;
            }

            foreach (uint iterationInstruction in instructions) {
                if (!treeElementForCurrentInstruction.isInstructionKnown(iterationInstruction)) {
                    treeElementForCurrentInstruction.appendInstruction(iterationInstruction, defaultPropabilityOfShadowedInstruction, new SparseArrayProgramDistributionTreeElement(defaultPropabilityOfShadowedInstruction));
                }

                treeElementForCurrentInstruction = treeElementForCurrentInstruction.getChildrenElementByInstruction(iterationInstruction);
            }
        }

        public void multiplyPropabiliyForProgram(uint[] instructions, double multiplicator) {
            var iterationNode = root;
            foreach (uint iterationInstruction in instructions) {
                // Ensure.ensureHard(iterationNode != null);
                iterationNode.multiplyPropabilityForInstruction(iterationInstruction, multiplicator);
                iterationNode = iterationNode.getChildrenElementByInstruction(iterationInstruction);
            }
        }

        SparseArrayProgramDistributionTreeElement root;
    }

    class SparseArrayProgramDistributionElementsWithPropability {
        class RelativePropabilityAndSum {
            public double relativePropability;
            public double propabilitySum;

            public RelativePropabilityAndSum(double relativePropability, double propabilitySum) {
                this.relativePropability = relativePropability;
                this.propabilitySum = propabilitySum;
            }
        }

        CompressedTable compressedTable = new CompressedTable();

        // we store propability and sum for fast sampling
        // sum of propabilities is just relative the propabilities only in this object
        IList<RelativePropabilityAndSum> propabilities = new List<RelativePropabilityAndSum>();

        public void append(uint instruction, double relativePropability) {
            // TODO< in debug mode check for other instructions >
            compressedTable.append(instruction);
            propabilities.Add(new RelativePropabilityAndSum(relativePropability, getPropabilitySum() + relativePropability));
        }

        public bool existInstruction(uint instruction) {
            return compressedTable.hasValue(instruction);
        }

        public uint getInstructionWhichFallsIntoAbsolutePropabilitySum(double absolutePropabilitySum) {
            // TODO< binary search >
            for (int i = propabilities.Count - 1; i >= 0; i--) {
                if (propabilities[i].propabilitySum < absolutePropabilitySum) {
                    return compressedTable.getValueByGlobalIndex((uint)i);
                }
            }

            return compressedTable.getValueByGlobalIndex(0);
        }

        public double getPropabilitySum() {
            // propabilitySum is sum until now or zero
            return propabilities.Count > 0 ? propabilities[propabilities.Count - 1].propabilitySum : 0.0;
        }

        public void multiplyPropabilityForInstruction(uint instruction, double multiplicator) {
            for (uint i = 0; i < count; i++) {
                if (compressedTable.getValueByGlobalIndex(i) == instruction) {
                    propabilities[(int)i].relativePropability *= multiplicator;
                }
                else {
                    propabilities[(int)i].relativePropability /= multiplicator;
                }
            }

            recalcPropabilitySum();
        }

        void recalcPropabilitySum() {
            double sum = 0.0;

            for (int i = 0; i < count; i++) {
                sum += propabilities[i].relativePropability;
                propabilities[i].propabilitySum = sum;
            }
        }


        public uint count {
            get {
                //Ensure.ensureHard(propabilities.Length == compressedTable.usedValues);
                return (uint)propabilities.Count;
            }
        }

    }

    // tree in the sparse program distibution
    // contains the propabilities and instructionnumbers of known instructions and a propability for all unknown remaining instructions
    class SparseArrayProgramDistributionTreeElement {
        public SparseArrayProgramDistributionTreeElement(double propabilityOfShadowedInstruction) {
            this.propabilityOfShadowedInstruction = propabilityOfShadowedInstruction;
        }

        // adds instruction with following tree
        public void appendInstruction(uint instruction, double relativePropability, SparseArrayProgramDistributionTreeElement childrenTreeElement) {
            // ensure hard
            Debug.Assert(!childrenByInstruction.ContainsKey(instruction));
            childrenByInstruction[instruction] = childrenTreeElement;
            tableWithPropability.append(instruction, relativePropability);
        }

        // returns null if there is no children by the selected instruction
        public SparseArrayProgramDistributionTreeElement getChildrenElementByInstruction(uint instruction) {
            if (childrenByInstruction.ContainsKey(instruction)) {
                return childrenByInstruction[instruction];
            }
            return null;
        }

        // if it is not known a random instruction is returned
        // \param instructionsetCount is as parameter that the object doesn't have to carry around the number of instructions
        public uint sampleInstructionBasedOnAbsolutePropability(double absolutePropabilitySum, uint instructionsetCount, Random random) {
            if (absolutePropabilitySum > tableWithPropability.getPropabilitySum()) {
                // propability mass of table is too low, we have to search an instruction which is not mentioned in the table

                for (;;) {
                    uint candidateInstruction = (uint)random.Next((int)instructionsetCount);
                    if (!tableWithPropability.existInstruction(candidateInstruction)) {
                        return candidateInstruction;
                    }
                }
            }
            else {
                return tableWithPropability.getInstructionWhichFallsIntoAbsolutePropabilitySum(absolutePropabilitySum);
            }
        }

        public double getPropabilityMass(uint instructionsetCount) {
            double propabilityMassInTable = tableWithPropability.getPropabilitySum();
            double propabilityMassInShadowedInstructions = propabilityOfShadowedInstruction * (double)(instructionsetCount - tableWithPropability.count);

            return propabilityMassInTable + propabilityMassInShadowedInstructions;
        }

        SparseArrayProgramDistributionElementsWithPropability tableWithPropability = new SparseArrayProgramDistributionElementsWithPropability();
        double propabilityOfShadowedInstruction; // of one instruction and not the whole propabilitymass of all shadowed instructions

        // children which describe the nodes after this instruction chosen by the instruction which was chosen
        IDictionary<uint, SparseArrayProgramDistributionTreeElement> childrenByInstruction = new Dictionary<uint, SparseArrayProgramDistributionTreeElement>();

        public bool isInstructionKnown(uint instruction) {
            return tableWithPropability.existInstruction(instruction);
        }

        // multiplies the propability of the instruction and all other instructions by the inverse
        public void multiplyPropabilityForInstruction(uint instruction, double multiplicator) {
            tableWithPropability.multiplyPropabilityForInstruction(instruction, multiplicator);
            propabilityOfShadowedInstruction /= multiplicator; // all other instructions get less propability mass
        }
    }







    // returns the program based on an distribution
    class ProgramSampler {
        public ProgramSampler(IProgramDistribution programDistribution, uint numberOfInstructions, uint instructionsetCount) {
            this.programDistribution = programDistribution;
            this.temporaryChosenInstructions = new uint[numberOfInstructions];
            this.temporaryPropabilityVector = new double[numberOfInstructions];
            this.instructionsetCount = instructionsetCount;
        }

        public void setInstructionsetCount(uint instructionsetCount) {
            this.instructionsetCount = instructionsetCount;
        }

        public uint[] sampleProgram(int programLength = -1) {
            int usedProgramLength = programLength == -1 ? (int)this.programLength : programLength;

            // fill random vector with values
            for (int instructionIndex = 0; instructionIndex < usedProgramLength; instructionIndex++) {
                temporaryPropabilityVector[instructionIndex] = random.NextDouble();
            }

            programDistribution.sample(temporaryPropabilityVector, ref temporaryChosenInstructions, instructionsetCount, usedProgramLength, random);
            return temporaryChosenInstructions;
        }

        private uint programLength {
            get {
                return (uint)temporaryPropabilityVector.Length;
            }
        }

        IProgramDistribution programDistribution;

        uint[] temporaryChosenInstructions;
        double[] temporaryPropabilityVector; // temporary vector for the chosen absolute values in range [0..1) on which the instructions get chosen

        uint instructionsetCount;
        public Random random = new Random();
    }










    sealed class InductionOperationsString {
        public static void arrayMove(InterpreterState state, int delta) {
            state.arrayState.index += delta;
            state.instructionPointer++;
        }

        public static void arrayRemove(InterpreterState state, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }

            state.arrayState.array.RemoveAt(state.arrayState.index);

            state.instructionPointer++;

            success = true;
        }

        public static void arrayCompareWithRegister(InterpreterState state, uint register, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }
            state.comparisionFlag = state.registers[register] == state.arrayState.array[state.arrayState.index];
            state.instructionPointer++;
            success = true;
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

        public static void return_(InterpreterState state, out bool success) {
            state.instructionPointer = state.topCallstack.pop(out success);
        }


        public static void call(InterpreterState state, int delta) {
            state.topCallstack.push(state.instructionPointer + 1);
            state.instructionPointer += delta;
            state.instructionPointer++;
        }

        public static void compare(InterpreterState state, uint register, int value) {
            state.comparisionFlag = state.registers[register] == value;
            state.instructionPointer++;
        }

        public static void add(InterpreterState state, uint register, int value) {
            state.registers[register] += value;
            state.instructionPointer++;
        }


        internal static void mul(InterpreterState state, int register, int value) {
            state.registers[register] *= value;
            state.instructionPointer++;
        }

        // add by checking flag
        public static void addFlag(InterpreterState state, uint register, int value) {
            if( state.comparisionFlag ) {
                state.registers[register] += value;
            }
            state.instructionPointer++;
        }

        internal static void arrayInsert(InterpreterState state, uint register, out bool success) {
            if (state.arrayState == null ) {
                success = false;
                return;
            }

            if( state.arrayState.index < 0 || state.arrayState.index > state.arrayState.array.Count ) {
                success = false;
                return;
            }

            int valueToInsert = state.registers[register];
            state.arrayState.array.Insert(state.arrayState.index, valueToInsert);

            state.instructionPointer++;
            success = true;
        }

        internal static void arraySetIdx(InterpreterState state, int index, out bool success) {
            if (state.arrayState == null ) {
                success = false;
                return;
            }

            if( index == -1 ) { // end of array, so insertion appends an element
                state.arrayState.index = state.arrayState.array.Count;
            }
            else {
                state.arrayState.index = index;
            }

            state.instructionPointer++;
            success = true;
        }

        // moves the array index by delta and stores in the flag if the index is still in bound after moving
        internal static void arrayIdxFlag(InterpreterState state, uint array, int delta, out bool success) {
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
        internal static void arrayValid(InterpreterState state, int array, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.comparisionFlag = state.arrayState.isIndexValid;

            state.instructionPointer++;
            success = true;
        }
        
        internal static void arrayRead(InterpreterState state, uint array, uint register, out bool success) {
            if (state.arrayState == null || !state.arrayState.isIndexValid) {
                success = false;
                return;
            }
            
            state.registers[register] = state.arrayState.array[state.arrayState.index];
            
            state.instructionPointer++;
            success = true;
        }

        internal static void arrayIdx2Reg(InterpreterState state, uint array, uint register, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.registers[register] = state.arrayState.index;

            state.instructionPointer++;
            success = true;
        }

        internal static void mov(InterpreterState state, int register, int value, out bool success) {
            state.registers[register] = value;

            state.instructionPointer++;
            success = true;
        }

        internal static void arrayMovToArray(InterpreterState state, uint array, uint register, out bool success) {
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
        internal static void macroArrayAdvanceOrExit(InterpreterState state, int ipDelta, out bool success) {
            if (state.arrayState == null) {
                success = false;
                return;
            }

            state.arrayState.index++;
            bool isIndexValid = state.arrayState.isIndexValid;
            if( !isIndexValid ) {
                state.arrayState.index = 0;
                return_(state, out success);
                return;
            }

            state.instructionPointer++;
            state.instructionPointer += ipDelta;

            success = true;
        }

    }

    public class CallStack {
        int privateCount;
        int[] stackValues = new int[128];

        public void setTo(int[] arr) {
            arr.CopyTo(stackValues, 0);
            privateCount = arr.Length;
        }

        public void push(int value) {
            stackValues[privateCount] = value;
            privateCount++;
        }

        public int pop(out bool success) {
            if (privateCount <= 0) {
                success = false;
                return 0;
            }

            success = true;
            int result = stackValues[privateCount - 1];
            privateCount--;
            return result;
        }

        public int top {
            get {
                return stackValues[privateCount - 1];
            }
        }

        public int count {
            get {
                return privateCount;
            }
        }
    }

    public class InterpreterState {
        public ArrayState arrayState;

        // each callstack is valid just for one function
        // but we need multiple callstacks because functions can invoke other functions
        public IList<CallStack> callstacks = new List<CallStack>();

        public CallStack topCallstack {
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

            callstacks = new List<CallStack> { new CallStack() };
            topCallstack.setTo(new int[] { 0x0000ffff });
        }

        public int[] registers;
        public bool comparisionFlag;
        public int instructionPointer;
    }

    public class ArrayState {
        public int index;
        public IList<int> array = new List<int>();

        public bool isIndexValid {
            get {
                return index >= 0 && index < array.Count;
            }
        }
    }


    sealed public class InstructionInfo {
        static string[] hardcodedSingleInstructions = {
            "arrayIdx -1", 
            "arrayIdx +1",
            "arrayRemove",
            "arrayCompare reg0",
            "arrayCompare reg1",
            "ret",
            "arrayInsert reg0",
            "arrayInsert reg1",
            "arrayInsert reg2",
            "arraySetIdx 0",
            "arraySetIdx -1",
            "arrayIdxFlag arr0 +1",
            "arrayValid arr0",
            "arrayRead arr0 reg0", // 13
            "arrayIdx2Reg arr0 reg0",
            "mov reg0, 0",
            "mov reg0, 1",
            "mov reg0, 3",
            "arrayMov arr0 reg0", // 18

            "mul reg0, -1", // 19

            "macro-arrAdvanceOrExit -4", // 20
        };

        public static uint getNumberOfHardcodedSingleInstructions() {
            return (uint)hardcodedSingleInstructions.Length;
        }

        public static uint getNumberOfInstructions() {
            return (uint)hardcodedSingleInstructions.Length + 1 + 1 + 1 + 3 * 16 + 1;
        }

        public static string getMemonic(uint instruction) {
            if( instruction <= hardcodedSingleInstructions.Length ) {
                return hardcodedSingleInstructions[(int)instruction];
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



            // jump
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                return String.Format("jmp {0}", jumpDelta);
            }
            currentBaseInstruction += 16;

            // jump if flag is not set
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                return String.Format("jmpIfNotFlag {0}", jumpDelta);
            }
            currentBaseInstruction += 16;
            
            // call
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                return String.Format("call {0}", jumpDelta);
            }
            currentBaseInstruction += 16;

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
        // checks if the program was terminated successfully by returning to the global caller
        public static bool isTerminating(InterpreterState interpreterState, uint instruction) {
            // return
            if( interpreterState.topCallstack.top == 0x0000ffff && instruction == 5 )   return true;


            if( instruction == 20 ) {
                bool atLastIndex = interpreterState.arrayState.index == interpreterState.arrayState.array.Count - 1;
                
                return atLastIndex;
            }

            return false;
        }

        // \param indirectCall is not -1 if the instruction is an indirect call to another function
        public static void dispatch(InterpreterState interpreterState, Instruction instr, out bool success, out int indirectCall) {
            indirectCall = -1;

            uint instruction = instr.code;

            switch (instruction) {
                case 0: InductionOperationsString.arrayMove(interpreterState, -1); success = true; return;
                case 1: InductionOperationsString.arrayMove(interpreterState, 1); success = true; return;
                case 2: InductionOperationsString.arrayRemove(interpreterState, out success); return;
                case 3: InductionOperationsString.arrayCompareWithRegister(interpreterState, 0, out success); return;
                case 4: InductionOperationsString.arrayCompareWithRegister(interpreterState, 1, out success); return;
                case 5: InductionOperationsString.return_(interpreterState, out success); return;
                case 6: InductionOperationsString.arrayInsert(interpreterState, /*reg*/0, out success); return;
                case 7: InductionOperationsString.arrayInsert(interpreterState, /*reg*/1, out success); return;
                case 8: InductionOperationsString.arrayInsert(interpreterState, /*reg*/2, out success); return;
                case 9: InductionOperationsString.arraySetIdx(interpreterState, 0, out success); return;
                case 10: InductionOperationsString.arraySetIdx(interpreterState, -1, out success); return; // -1 is end of array
                case 11: InductionOperationsString.arrayIdxFlag(interpreterState, 0, 1, out success); return; // TODO< should be an intrinsic command which gets added by default >
                case 12: InductionOperationsString.arrayValid(interpreterState, /*array*/0, out success); return;
                case 13: InductionOperationsString.arrayRead(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 14: InductionOperationsString.arrayIdx2Reg(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 15: InductionOperationsString.mov(interpreterState, /*register*/0, 0, out success); return;
                case 16: InductionOperationsString.mov(interpreterState, /*register*/0, 1, out success); return;
                case 17: InductionOperationsString.mov(interpreterState, /*register*/0, 3, out success); return;
                case 18: InductionOperationsString.arrayMovToArray(interpreterState, /*array*/0, /*register*/0, out success); return;
                case 19: InductionOperationsString.mul(interpreterState, /*register*/0, -1); success = true; return;
                case 20: InductionOperationsString.macroArrayAdvanceOrExit(interpreterState, -4, out success); return;
            }

            // if we are here we have instrution with hardcoded parameters

            uint baseInstruction = InstructionInfo.getNumberOfHardcodedSingleInstructions();
            Debug.Assert(instruction >= baseInstruction);
            int currentBaseInstruction = (int)baseInstruction;

            // compare constant
            if (instruction <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                InductionOperationsString.compare(interpreterState, /*register*/0, 0);
                success = true;
                return;
            }

            currentBaseInstruction += 1;





            // add register constant
            if (instruction <= currentBaseInstruction + 2) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the pool

                if (subInstruction == 0) {
                    InductionOperationsString.add(interpreterState, /*register*/0, -1);
                }
                else if (subInstruction == 1) {
                    InductionOperationsString.add(interpreterState, /*register*/0, 2);
                }

            }

            currentBaseInstruction += 2;


            // addFlag reg0 constant
            if (instruction <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                InductionOperationsString.addFlag(interpreterState, /*register*/0, 1);
                success = true;
                return;
            }

            currentBaseInstruction += 1;





            // jump
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                InductionOperationsString.jump(interpreterState, jumpDelta);
                success = true;
                return;
            }
            currentBaseInstruction += 16;

            // jump if flag is not set
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                InductionOperationsString.jumpIfNotFlag(interpreterState, jumpDelta);
                success = true;
                return;
            }
            currentBaseInstruction += 16;
            
            // call
            if (instruction <= currentBaseInstruction + 16) {
                int subInstruction = (int)instruction - currentBaseInstruction; // which instruction do we choose from the jump instructions?
                int jumpDelta = subInstruction - 8;
                InductionOperationsString.call(interpreterState, jumpDelta);
                success = true;
                return;
            }
            currentBaseInstruction += 16;

            // indirect table call
            if (instruction <= currentBaseInstruction + 1) {
                // currently just compare reg0 with zero
                indirectCall = 0;
                success = true;
                return;
            }

            currentBaseInstruction += 1;


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

            /*

            arguments.program = new uint[]
            {
                13,
                19,
                18,
                20,
            };
            arguments.lengthOfProgram = 4;
            arguments.maxNumberOfRetiredInstructions = 500;


            */

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
                    Program2.debug(null, arguments.program);

                    Console.WriteLine("ip={0}", arguments.interpreterState.instructionPointer);
                    Console.Write("arr=");

                    throw new NotImplementedException(); // TODO< logger >
                    Program2.debug(null, arguments.interpreterState.arrayState.array);
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

            uint[] sampledProgram = programSampler.sampleProgram((int)numberOfInstructionsToEnumerate);
            // copy
            sampledProgram.CopyTo(program, 0);
            program[privateNumberOfInstructions - 1] = 5; // overwrite last instruction with ret so it terminates always

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

                interpreterArguments.interpreterState.arrayState.array.Clear();
                for (int i = 0; i < currentTrainingSample.questionArray.Count; i++) {
                    interpreterArguments.interpreterState.arrayState.array.Add(currentTrainingSample.questionArray[i]);
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

            privateNumberOfInstructions = 1+/* with RET*/1; // with RET  (int)numberOfInstructions;

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

        public uint instructionsetCount;

        public double exhausiveSearchFactor = 2.0; // hown many times do we try a program at maximum till we give up based on the # of combinations
                                                   // can be less than 1 which favors non-exhausive search
        
        int privateNumberOfInstructions;
        uint numberOfInstructionsToEnumerate;
        uint enumerationMaxProgramLength; // maximal program length (with RET if RET is included) of enumeration

        public Interpreter.InterpretArguments interpreterArguments = new Interpreter.InterpretArguments(); // preallocated
        uint[] program; // preallocated temporary program which is composed out of the parent program and the current try

        Interpreter interpreter = new Interpreter();
        ProgramSampler programSampler;
        SparseArrayProgramDistribution programDistribution;
        public long remainingIterationsForCurrentProgramLength;

        public IList<TrainingSample> trainingSamples = new List<TrainingSample>();
        public uint[] parentProgram; // can be null

        internal void biasSearchTowardProgram() {
            uint[] effectiveProgram = new uint[numberOfInstructions - 1];
            for (int i = 0; i < numberOfInstructions - 1; i++) {
                effectiveProgram[i] = program[i];
            }

            programDistribution.addProgram(effectiveProgram);
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

        public static void debug(ILogger log,  uint[] arr) {
            string message = "";

            foreach (uint iValue in arr) {
                message += string.Format("{0} ", iValue);
            }

            Logged logged = new Logged();
            logged.notifyConsole = Logged.EnumNotifyConsole.YES;
            logged.message = message;
            logged.origin = new string[]{ "search", "ALS" };
            logged.serverity = Logged.EnumServerity.INFO;
            log.write(logged);
        }

        public static void debug<Type>(ILogger log, IList<Type> list) {
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

        

        public static void interactiveTestEnumeration(ILogger logger) {
            uint programMaxSize = 512;

            SparseArrayProgramDistribution programDistribution = new SparseArrayProgramDistribution();

            uint numberOfInstructions = 5;
            uint enumeratedProgramLength = numberOfInstructions - 1;
            uint instructionsetCount = 54 + 16 + 1 + 1 - 16/*because no call*/;

            uint[] program = new uint[programMaxSize];

            uint numberOfInstructionsToEnumerate = 6; // we enumerate at maximum with just 6 instructions
            // ASK< do we need this even if we base our programs on existing programs? >
            ProgramSampler programSampler = new ProgramSampler(programDistribution, numberOfInstructionsToEnumerate, instructionsetCount);

            programSampler.setInstructionsetCount(instructionsetCount);


            double exhausiveSearchFactor = 2.0; // hown many times do we try a program at maximum till we give up based on the # of combinations
                                                // can be less than 1 which favors non-exhausive search

            Interpreter.InterpretArguments interpreterArguments = new Interpreter.InterpretArguments();
            interpreterArguments.maxNumberOfRetiredInstructions = 50;
            interpreterArguments.program = program;
            interpreterArguments.lengthOfProgram = numberOfInstructions;
            interpreterArguments.interpreterState = new InterpreterState();
            interpreterArguments.interpreterState.registers = new int[3];
            interpreterArguments.interpreterState.arrayState = new ArrayState();
            interpreterArguments.interpreterState.arrayState.array = new List<int>();
            interpreterArguments.debugExecution = false;

            Interpreter interpreter = new Interpreter();

            uint[] programDirectParent;
            uint[] programResult = new uint[0]; // the program of the current search process


            IList<TrainingSample> trainingSamples = new List<TrainingSample>();
            trainingSamples.Add(new TrainingSample());
            trainingSamples.Add(new TrainingSample());
            trainingSamples[0].questionArray = new List<int> { 5, 8, 3, 7 };
            trainingSamples[0].questionRegisters = new int?[] { 7, null, null }; // search for 7
            trainingSamples[0].answerArray = new List<int> { 5, 8, 3, 7 }; // don't change array
            trainingSamples[0].answerArrayIndex = 3; // result index must be 3

            trainingSamples[1].questionArray = new List<int> { 7, 8, 3, 2 };
            trainingSamples[1].questionRegisters = new int?[] { 7, null, null }; // search for 7
            trainingSamples[1].answerArray = new List<int> { 7, 8, 3, 2 }; // don't change array
            trainingSamples[1].answerArrayIndex = 0; // result index must be 3



            for (int iteration = 0; iteration < (double)(exhausiveSearchFactor * Math.Pow(instructionsetCount, enumeratedProgramLength)); iteration++) {
                uint[] sampledProgram = programSampler.sampleProgram((int)numberOfInstructionsToEnumerate);
                // copy
                sampledProgram.CopyTo(program, 0);
                program[numberOfInstructions - 1] = 5; // overwrite last instruction with ret so it terminates always


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

                    interpreterArguments.interpreterState.arrayState.array.Clear();
                    for (int i = 0; i < currentTrainingSample.questionArray.Count; i++) {
                        interpreterArguments.interpreterState.arrayState.array.Add(currentTrainingSample.questionArray[i]);
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
                        currentTrainingSample.answerArray != null &
                        !ListHelpers.isSame(interpreterArguments.interpreterState.arrayState.array, currentTrainingSample.answerArray)
                    ) {
                        trainingSamplesTestedSuccessful = false;
                        break;
                    }

                    if (currentTrainingSample.answerArrayIndex.HasValue && interpreterArguments.interpreterState.arrayState.index != currentTrainingSample.answerArrayIndex) {
                        trainingSamplesTestedSuccessful = false;
                        break;
                    }

                }

                if (!trainingSamplesTestedSuccessful) {
                    continue; // try next program
                }

                // ** search was successful


                // bias further search
                {
                    uint[] effectiveProgram = new uint[numberOfInstructions - 1];
                    for (int i = 0; i < numberOfInstructions - 1; i++) {
                        effectiveProgram[i] = program[i];
                    }

                    programDistribution.addProgram(effectiveProgram);
                }


                { // log
                    Logged logged = new Logged();
                    logged.notifyConsole = Logged.EnumNotifyConsole.YES;
                    logged.message = string.Format("< Task:find(no meta) > finished, required< #iterations={0}, cputime=?, realtime=?>", iteration);
                    logged.origin = new string[] {"search", "ALS"};
                    logged.serverity = Logged.EnumServerity.INFO;
                    logger.write(logged);
                }
                
                programResult = new uint[numberOfInstructions];
                for (int i = 0; i < programResult.Length; i++) {
                    programResult[i] = program[i];
                }
                debug(logger, programResult);


                break;




            }

            int here5 = 5;


            // store found program
            programDirectParent = new uint[numberOfInstructions];
            programResult.CopyTo(programDirectParent, 0);



            programSampler.setInstructionsetCount(54 + 1 + 1 + 16);

            numberOfInstructions = 7;
            enumeratedProgramLength = numberOfInstructions - 1;

            program = new uint[programDirectParent.Length + numberOfInstructions];
            interpreterArguments.program = program;

            numberOfInstructionsToEnumerate = enumeratedProgramLength; // we enumerate one less instruction because we add a ret at the end by default

            interpreterArguments.lengthOfProgram = (uint)programDirectParent.Length + numberOfInstructions;

            for (int iteration = 0; true /*iteration < (double)(exhausiveSearchFactor*Math.Pow(instructionsetCount, enumeratedProgramLength))*/; iteration++) {
                uint[] sampledProgram = programSampler.sampleProgram((int)numberOfInstructionsToEnumerate);
                // concat sampled program with parent program
                sampledProgram.CopyTo(program, 0);
                program[numberOfInstructions - 1] = 5; // overwrite last instruction with ret so it terminates always

                programDirectParent.CopyTo(program, numberOfInstructions);

                { // reset interpreter state
                    interpreterArguments.interpreterState.reset();

                    interpreterArguments.interpreterState.registers[0] = 2; // remove two times
                    interpreterArguments.interpreterState.registers[1] = 2; // search for 2
                    interpreterArguments.interpreterState.arrayState.array.Clear();
                    interpreterArguments.interpreterState.arrayState.array.Add(4);
                    interpreterArguments.interpreterState.arrayState.array.Add(2);
                    interpreterArguments.interpreterState.arrayState.array.Add(7);
                    interpreterArguments.interpreterState.arrayState.array.Add(9);
                }


                // * interpret
                bool
                    programExecutedSuccessful,
                    hardExecutionError;

                interpreter.interpret(interpreterArguments, out programExecutedSuccessful, out hardExecutionError);

                if (!programExecutedSuccessful) {
                    continue; // try next program
                }

                // we are here if the program executed successfully and if it returned something


                // compare result

                if (
                    !ListHelpers.isSame(interpreterArguments.interpreterState.arrayState.array, new List<int> { 4, 9 })
                ) {
                    continue;
                }













                { // reset interpreter state
                    interpreterArguments.interpreterState.reset();

                    interpreterArguments.interpreterState.registers[0] = 1; // remove 1 time
                    interpreterArguments.interpreterState.registers[1] = 7; // search for 7
                    interpreterArguments.interpreterState.arrayState.array.Clear();
                    interpreterArguments.interpreterState.arrayState.array.Add(4);
                    interpreterArguments.interpreterState.arrayState.array.Add(2);
                    interpreterArguments.interpreterState.arrayState.array.Add(7);
                    interpreterArguments.interpreterState.arrayState.array.Add(9);
                }


                // * interpret
                interpreter.interpret(interpreterArguments, out programExecutedSuccessful, out hardExecutionError);

                if (!programExecutedSuccessful) {
                    continue; // try next program
                }

                // we are here if the program executed successfully and if it returned something


                // compare result

                if (
                    !ListHelpers.isSame(interpreterArguments.interpreterState.arrayState.array, new List<int> { 4, 2, 9 })
                ) {
                    continue;
                }






                { // reset interpreter state
                    interpreterArguments.interpreterState.reset();

                    interpreterArguments.interpreterState.registers[0] = 3; // remove 3 times
                    interpreterArguments.interpreterState.registers[1] = 4; // search for 4
                    interpreterArguments.interpreterState.arrayState.array.Clear();
                    interpreterArguments.interpreterState.arrayState.array.Add(4);
                    interpreterArguments.interpreterState.arrayState.array.Add(2);
                    interpreterArguments.interpreterState.arrayState.array.Add(7);
                    interpreterArguments.interpreterState.arrayState.array.Add(9);
                }


                // * interpret
                interpreter.interpret(interpreterArguments, out programExecutedSuccessful, out hardExecutionError);

                if (!programExecutedSuccessful) {
                    continue; // try next program
                }

                // we are here if the program executed successfully and if it returned something


                // compare result
                if (
                    !ListHelpers.isSame(interpreterArguments.interpreterState.arrayState.array, new List<int> { 9 })
                ) {
                    continue;
                }



                {
                    Logged logged = new Logged();
                    logged.notifyConsole = Logged.EnumNotifyConsole.YES;
                    logged.message = string.Format("<Task:findRemovalProgramExtended1 (no meta)> finished, required< #iterations={0}, cputime=?, realtime=?>", iteration);
                    logged.origin = new string[] { "search", "ALS" };
                    logged.serverity = Logged.EnumServerity.INFO;
                    logger.write(logged);
                }
                
                
                // debug memonics
                for (int i = 0; i < program.Length; i++) {
                    Logged logged = new Logged();
                    logged.notifyConsole = Logged.EnumNotifyConsole.YES;
                    logged.message = string.Format("{0}: {1}", i, InstructionInfo.getMemonic(program[i]));
                    logged.origin = new string[] { "search", "ALS" };
                    logged.serverity = Logged.EnumServerity.INFO;
                    logger.write(logged);

                    //Console.WriteLine("{0}: {1}", i, InstructionInfo.getMemonic(program[i]));
                }


                int here9 = 9;

                programResult = new uint[program.Length];
                for (int i = 0; i < programResult.Length; i++) {
                    programResult[i] = program[i];
                }




            }

        }
        
    }

    class ListHelpers {
        public static bool isSame(IList<int> a, IList<int> b) {
            if (a.Count != b.Count) {
                return false;
            }
            for (int i = 0; i < a.Count; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
