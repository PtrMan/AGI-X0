namespace MetaNix.search.levin2 {
    // wrapper for two parameter call with success
    static class ArrayOperationsTwoArgumentWrapper {
        public static void arrayMove(GlobalInterpreterState globalState, LocalInterpreterState localState, int delta, int dummy, out bool success) {
            ArrayOperations.arrayMove(globalState, localState, delta);
            success = true;
        }

        public static void arrayRemove(GlobalInterpreterState globalState, LocalInterpreterState localState, int dummy0, int dummy1, out bool success) {
            ArrayOperations.arrayRemove(globalState, localState, out success);
        }

        public static void arrayCompareWithRegister(GlobalInterpreterState globalState, LocalInterpreterState localState, int register, int dummy0, out bool success) {
            ArrayOperations.arrayCompareWithRegister(globalState, localState, register, out success);
        }
    }

    // wrappr for two parameters call with cuccess
    static class OperationsTwoArgumentWrapper {
        public static void jumpIfNotFlag(LocalInterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jumpIfNotFlag(state, delta);
            success = true;
        }

        public static void jumpIfFlag(LocalInterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jumpIfFlag(state, delta);
            success = true;
        }

        public static void jump(LocalInterpreterState state, int delta, int dummy0, out bool success) {
            Operations.jump(state, delta);
            success = true;
        }

    }

    public static class Operations {
        public static void @return(LocalInterpreterState state, out bool success) {
            state.instructionPointer = state.callstack.pop(out success);
        }


        public static void jumpIfNotFlag(LocalInterpreterState state, int delta) {
            if (!state.comparisionFlag) {
                state.instructionPointer += delta;
            }
            state.instructionPointer++;
        }

        public static void jumpIfFlag(LocalInterpreterState state, int delta) {
            if (state.comparisionFlag) {
                state.instructionPointer += delta;
            }
            state.instructionPointer++;
        }

        public static void jump(LocalInterpreterState state, int delta) {
            state.instructionPointer += delta;
            state.instructionPointer++;
        }

        public static void movImmediate(LocalInterpreterState state, int register, int value, out bool success) {
            state.registers[register] = value;

            state.instructionPointer++;
            success = true;
        }



        public static void call(LocalInterpreterState state, int delta) {
            state.callstack.push(state.instructionPointer + 1);
            state.instructionPointer += delta;
            state.instructionPointer++;
        }

        // /param type 0 : equality, -1 less than, 1 greater than
        public static void compareImmediate(LocalInterpreterState state, int register, int value, int type) {
            if( type == 0 )        state.comparisionFlag = state.registers[register] == value;
            else if (type == -1)   state.comparisionFlag = state.registers[register] < value;
            else                   state.comparisionFlag = state.registers[register] > value;

            state.instructionPointer++;
        }

        // /param type 0 : equality, -1 less than, 1 greater than
        public static void compareRegister(LocalInterpreterState state, int registerRight, int registerLeft, int type) {
            if (type == 0) state.comparisionFlag = state.registers[registerRight] == state.registers[registerLeft];
            else if (type == -1) state.comparisionFlag = state.registers[registerRight] < state.registers[registerLeft];
            else state.comparisionFlag = state.registers[registerRight] > state.registers[registerLeft];

            state.instructionPointer++;
        }


        public static void add(LocalInterpreterState state, int register, int value) {
            state.registers[register] += value;
            state.instructionPointer++;
        }


        public static void mulRegisterImmediate(LocalInterpreterState state, int register, int value) {
            state.registers[register] *= value;
            state.instructionPointer++;
        }

        public static void mulRegisterRegister(LocalInterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] *= state.registers[registerSource];
            state.instructionPointer++;
        }

        public static void addRegisterRegister(LocalInterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] += state.registers[registerSource];
            state.instructionPointer++;
        }

        public static void subRegisterRegister(LocalInterpreterState state, int registerDestination, int registerSource) {
            state.registers[registerDestination] -= state.registers[registerSource];
            state.instructionPointer++;
        }

        // add by checking flag
        public static void addFlag(LocalInterpreterState state, int register, int value) {
            if (state.comparisionFlag) {
                state.registers[register] += value;
            }
            state.instructionPointer++;
        }

        // interprets the value as binary (zero is false and all else is true) and negates it
        public static void binaryNegate(GlobalInterpreterState globalState, LocalInterpreterState localState, int register) {
            localState.registers[register] = (localState.registers[register] == 0) ? 1 : 0;
            localState.instructionPointer++;
        }

        // random number up to the value of the register
        public static void random(GlobalInterpreterState globalState, LocalInterpreterState localState, int destRegister, int register, out bool success) {
            if (localState.registers[register] <= 0) {
                success = false;
                return;
            }

            localState.registers[destRegister] = globalState.rng.Next(localState.registers[register]);
            success = true;
        }
    }

    public static class ArrayOperations {
        public static void arrayMove(GlobalInterpreterState globalState, LocalInterpreterState localState, int delta) {
            globalState.arrayState.index += delta;
            localState.instructionPointer++;
        }

        public static void arrayRemove(GlobalInterpreterState globalState, LocalInterpreterState localState, out bool success) {
            if (globalState.arrayState == null || !globalState.arrayState.isIndexValid) {
                success = false;
                return;
            }

            globalState.arrayState.array.removeAt(globalState.arrayState.index);

            localState.instructionPointer++;

            success = true;
        }

        public static void arrayCompareWithRegister(GlobalInterpreterState globalState, LocalInterpreterState localState, int register, out bool success) {
            if (globalState.arrayState == null || !globalState.arrayState.isIndexValid) {
                success = false;
                return;
            }
            localState.comparisionFlag = localState.registers[register] == globalState.arrayState.array[globalState.arrayState.index];
            localState.instructionPointer++;
            success = true;
        }


        public static void insert(GlobalInterpreterState globalState, LocalInterpreterState localState, int register, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            if (globalState.arrayState.index < 0 || globalState.arrayState.index > globalState.arrayState.array.count) {
                success = false;
                return;
            }

            int valueToInsert = localState.registers[register];
            globalState.arrayState.array.insert(globalState.arrayState.index, valueToInsert);

            localState.instructionPointer++;
            success = true;
        }

        // array is ignored
        public static void setIdxRelative(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, int index, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            if (index == -1) { // end of array, so insertion appends an element
                globalState.arrayState.index = globalState.arrayState.array.count;
            }
            else {
                globalState.arrayState.index = index;
            }

            localState.instructionPointer++;
            success = true;
        }

        // moves the array index by delta and stores in the flag if the index is still in bound after moving
        public static void idxFlag(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, int delta, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            if (array == 0) {
                globalState.arrayState.index += delta;
                localState.comparisionFlag = globalState.arrayState.isIndexValid; // store the validity of the arrayIndex in flag
            }
            else {
                success = false;
                return;
            }


            localState.instructionPointer++;
            success = true;
        }

        // /param array is the index of the array (currently ignored)
        public static void valid(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            localState.comparisionFlag = globalState.arrayState.isIndexValid;

            localState.instructionPointer++;
            success = true;
        }

        public static void read(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, int register, out bool success) {
            if (globalState.arrayState == null || !globalState.arrayState.isIndexValid) {
                success = false;
                return;
            }

            localState.registers[register] = globalState.arrayState.array[globalState.arrayState.index];

            localState.instructionPointer++;
            success = true;
        }

        public static void idx2Reg(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, int register, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            localState.registers[register] = globalState.arrayState.index;

            localState.instructionPointer++;
            success = true;
        }

        public static void reg2idx(GlobalInterpreterState globalState, LocalInterpreterState localState, int register, int array, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            globalState.arrayState.index = localState.registers[register];

            localState.instructionPointer++;
            success = true;
        }


        public static void arrayMovToArray(GlobalInterpreterState globalState, LocalInterpreterState localState, int array, int register, out bool success) {
            if (globalState.arrayState == null || !globalState.arrayState.isIndexValid) {
                success = false;
                return;
            }
            globalState.arrayState.array[globalState.arrayState.index] = localState.registers[register];
            localState.instructionPointer++;
            success = true;
        }

        // macro-arrAdvanceOrExit
        // advance array index and reset and return/terminate if its over
        // else jump relative
        public static void macroArrayAdvanceOrExit(GlobalInterpreterState globalState, LocalInterpreterState localState, int ipDelta, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            globalState.arrayState.index++;
            bool isIndexValid = globalState.arrayState.isIndexValid;
            if (!isIndexValid) {
                globalState.arrayState.index = 0;
                Operations.@return(localState, out success);
                return;
            }

            localState.instructionPointer++;
            localState.instructionPointer += ipDelta;

            success = true;
        }

        // macro-arrNotEndOrExit
        // advance array index and reset and return/terminate if its over
        // else jump relative
        public static void macroArrayNotEndOrExit(GlobalInterpreterState globalState, LocalInterpreterState localState, int ipDelta, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            bool isIndexValid = globalState.arrayState.isIndexValid;
            if (!isIndexValid) {
                globalState.arrayState.index = 0;
                Operations.@return(localState, out success);
                return;
            }

            localState.instructionPointer++;
            localState.instructionPointer += ipDelta;

            success = true;
        }

        public static void length(GlobalInterpreterState globalState, LocalInterpreterState localState, int destRegister, out bool success) {
            if (globalState.arrayState == null) {
                success = false;
                return;
            }

            localState.registers[destRegister] = globalState.arrayState.array.count;

            success = true;
        }
    }

}
