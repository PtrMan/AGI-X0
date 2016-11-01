module search.levin.LevinSearch;

import std.stdint;
import std.array : insertInPlace;

import linopterixed.types.Bigint;
import misced.BinaryHelpers;

abstract class LevinProblem {
    public abstract void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted);
}

class LevinProgram {
    uint[] instructions;

    final LevinProgram copy() {
        LevinProgram result = new LevinProgram();

        foreach(uint iterationInstruction; instructions) {
            result.instructions ~= iterationInstruction;
        }

        return result;
    }
}


final class Solution {
    final this(LevinProgram program) {
        this.program = program;
    }

    LevinProgram program;
}

// translated from the C# version

// see paper "Shifting inductive bias with success-story algorithm, adaptive levin search and incremental self improvement"
// for the description of levin search implemented here

// TODO< make it more efficient maybe by reversing the order of the instructions and putting them into a tree >
class LevinSearch {
    final this() {
        reset();

        // set bigIntOne to 1
        bigIntOne.accessor32(0, 1);
        bigIntOne.accessor32(1, 0);
    }

    final void reset() {
        phase = 1;
        currentIteration = 0;
    }


    final Solution iterate(LevinProblem problem, uint numberOfIterations, out bool done) {
        done = false;

        currentProgramEnumerator.accessor32(0, 0); // set it to zero
        currentProgramEnumerator.accessor32(1, 0);

        for ( uint i = 0; i < numberOfIterations; i++ ) {
            Solution solution = iteration(problem, /*out*/ done);
            if( done ) {
                return solution;
            }
        }

        return null;
    }

    // iteration number start at zero!
    protected final Solution iteration(LevinProblem problem, out bool done) {
        uint currentNumberOfInstructions = currentIteration + 1;

        uint innerFnGetInstruction(size_t instructionIndex) {
            Bigint!1 masked;
            Bigint!1.booleanAnd(currentProgramEnumerator.shiftRight(cachedBitsOfInstruction * instructionIndex), programEnumeratorInstructionMask, /*out*/masked);
            uint instruction = masked.accessor32(0); // ASSUMTION< we assume that an instruction can't be bigger than 32 bits >
            
            //import std.stdio;
            //writeln("innerFnGetInstruction() [", instructionIndex, "] of ", currentProgramEnumerator.shiftRight(cachedBitsOfInstruction * instructionIndex).accessor32(0), " = ", instruction);

            assert(instruction < numberOfInstructions);
            return instruction;
        }

        bool innerFnIsIterationFinished() {
            // do it by looking at the highest bit of the program
            // this is fasten than looking for the instruction to not be 0

            return currentProgramEnumerator.getBit(cachedBitsOfInstruction * currentNumberOfInstructions);
        }

        void innerFnResetFirstBitOfInstruction(size_t insutrctionIndex) {
            currentProgramEnumerator.setBit(cachedBitsOfInstruction * insutrctionIndex, false);
        }

        uint[] innerFnTranslateCurrentProgramEnumeratorToInstructions() {
            uint[] result;
            result.length = currentNumberOfInstructions;

            for( uint i = 0; i < currentNumberOfInstructions; i++ ) {
                result[i] = innerFnGetInstruction(i);
            }
            return result;
        }

        done = false;

        if( currentIteration > maxIterations ) {
            done = true;
            return null;
        }

        LevinProgram levinProgram = new LevinProgram();

        for(;;) {
            if( innerFnIsIterationFinished() ) {
                break;
            }


            // execute program

            // TODO< order programs by propability ? >
            // by looking up the real instruction with an reording table

            // TODO< we can do multiple iterations and execute multiple programs in parallel >

            levinProgram.instructions = innerFnTranslateCurrentProgramEnumeratorToInstructions();

            uint maxNumberOfStepsToExecute = cast(uint)((calcSolomonoffLevinMeasure(levinProgram.instructions) * cast(double)phase) / c);

            bool hasHalted;

            if( reportProgramExecution !is null ) {
                reportProgramExecution(levinProgram);
            }

            executeProgram(levinProgram, problem, maxNumberOfStepsToExecute, /*out*/ hasHalted);
            if( hasHalted ) {
                done = true;
                return new Solution(levinProgram);
            }

            // we need to increment the counter
            incrementCurrentProgramEnumerator();
        }

        // we have to set the highest instruction to zero, because else we skip some of the searchspace, example:
        // [0]
        // [1]
        // [0, 0] <- last instruction has to be zero instead of one
        // ...
        innerFnResetFirstBitOfInstruction(currentIteration+1);

        phase *= 2;
        currentIteration++;

        return null;
    }
    
    private final void executeProgram(LevinProgram program, LevinProblem problem, uint maxNumberOfStepsToExecute, out bool hasHalted) {
        problem.executeProgram(program, maxNumberOfStepsToExecute, /*out*/ hasHalted);
    }

    // [1] https://en.wikipedia.org/wiki/Algorithmic_probability
    // [2] http://wiki.opencog.org/w/OpenCogPrime:OccamsRazor

    // approximation for not computable "solomonoff levin measure"
    // see mainly [2] and secondary [1]
    protected final double calcSolomonoffLevinMeasure(uint[] instructions) {
        double measure = 1.0;

        foreach( instructionI, instruction; instructions ) {
            measure *= instructionPropabilityMatrix[instructionI][instruction];
        }
        //sum += Math.Pow(2.0f, -(double)instructions.length);
        
        return measure;
    }

    protected final void incrementCurrentProgramEnumerator() {
        // increment
        {
            Bigint!1 temp;
            Bigint!1.add(currentProgramEnumerator, bigIntOne, temp);
            currentProgramEnumerator = temp;
        }

        // check for overflow
        
        // check if we need to carry over the incrementation 
        // we don't have to do this for power of two # of instruction because it does it automatically when we increment it 
        if( !cachedNumberOfInstructionIsPowerOfTwo ) {
            // if it is not a power of two we have to check the lowest place for an overflow, if the overflow happend we increment the next instruction, and so on for all instructions

            foreach( instructionIndex; 0..sizeOfProgram ) {
                Bigint!1 masked;
                Bigint!1.booleanAnd(currentProgramEnumerator.shiftRight(cachedBitsOfInstruction * instructionIndex), programEnumeratorInstructionMask, /*out*/masked);
                
                assert( masked.accessor32(0) <= numberOfInstructions );
                if( masked.accessor32(0) == numberOfInstructions ) { // we assume here that the # of instructions is <= 32 bits, which shouldn't be able to be broken with a normal computer
                    // increment the next up instruction
                    {
                        Bigint!1 temp;
                        Bigint!1 addValue = bigIntOne.shiftLeft(cachedBitsOfInstruction * (instructionIndex+1));
                        Bigint!1.add(currentProgramEnumerator, addValue, temp);
                        currentProgramEnumerator = temp;
                    }

                    // ... and set the instruction to zero
                    {
                        Bigint!1 temp;
                        Bigint!1.booleanAnd(programEnumeratorInstructionMask.shiftLeft(cachedBitsOfInstruction * instructionIndex).booleanNegation(), currentProgramEnumerator, temp);
                        currentProgramEnumerator = temp;
                    }
                }
            }
        }
    }


    uint phase;
    uint currentIteration;

    public uint maxIterations = 0;
    public double c = 0.0;

    //protected LevinProgram[] previousLevinPrograms;
    protected Bigint!1 currentProgramEnumerator; // Bigint with the size of 64 bit
    protected Bigint!1 bigIntOne;

    protected Bigint!1 programEnumeratorInstructionMask;

    // [position in program, op]
    public double[][] instructionPropabilityMatrix;

    final @property uint sizeOfProgram() pure const {
        assert(instructionPropabilityMatrix.length != 0);
        return instructionPropabilityMatrix.length;
    }

    final @property uint numberOfInstructions() pure {
        return protectedNumberOfInstructions;
    }

    final @property uint numberOfInstructions(uint newValue) {
        protectedNumberOfInstructions = newValue;

        cachedBitsOfInstruction = numberOfBits(protectedNumberOfInstructions);
        cachedNumberOfInstructionIsPowerOfTwo = cachedBitsOfInstruction == 1;

        programEnumeratorInstructionMask.accessor32(0, maskForBits!uint32_t(cachedBitsOfInstruction));
        programEnumeratorInstructionMask.accessor32(1, 0);

        return newValue;
    }

    protected uint protectedNumberOfInstructions = 0;
    protected uint cachedBitsOfInstruction = 0;
    protected bool cachedNumberOfInstructionIsPowerOfTwo = false;


    alias void delegate(LevinProgram currentProgram) ReportProgramExecutionDelegateType;
    ReportProgramExecutionDelegateType reportProgramExecution = null; // is a delegate which gets called before the evaluation of each program
}

unittest {
    // should enumerate 0, 1, 2, 0-0, 1-0, 2-0, 0-1, 1-1, 2-1, 0-2, 1-2, 2-2

    struct RecordedProgram {
        uint[] instructions;
    }

    RecordedProgram[] programs;

    void recordProgram(LevinProgram currentProgram) {
        RecordedProgram newProgram;
        newProgram.instructions = currentProgram.instructions;
        programs ~= newProgram;
    }

    final class DummyProblem : LevinProblem {
        final override void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
            hasHalted = false;
        }
    }

    LevinSearch levinSearch = new LevinSearch();
    levinSearch.reportProgramExecution = &recordProgram;
    levinSearch.numberOfInstructions = 3;
    levinSearch.c = 0.1;
    levinSearch.maxIterations = 5000;

    uint programLength = 2;

    levinSearch.instructionPropabilityMatrix.length = programLength;
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
        iterationArray.length = levinSearch.numberOfInstructions;
    }

    // set initial propaility matrix
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
        foreach( i; 0..iterationArray.length ) {
            iterationArray[i] = 1.0f;
        }
    }

    bool done;
    uint numberOfIterations = 2;
    levinSearch.iterate(new DummyProblem, numberOfIterations, /*out*/ done);

    assert(programs.length == 9+3);

    assert(programs[0].instructions == [0]);
    assert(programs[1].instructions == [1]);
    assert(programs[2].instructions == [2]);

    assert(programs[3].instructions == [0, 0]);
    assert(programs[4].instructions == [1, 0]);
    assert(programs[5].instructions == [2, 0]);

    assert(programs[6].instructions == [0, 1]);
    assert(programs[7].instructions == [1, 1]);
    assert(programs[8].instructions == [2, 1]);

    assert(programs[9].instructions == [0, 2]);
    assert(programs[10].instructions == [1, 2]);
    assert(programs[11].instructions == [2, 2]);
}


unittest {
    struct RecordedProgram {
        uint[] instructions;
    }

    RecordedProgram[] programs;

    void recordProgram(LevinProgram currentProgram) {
        RecordedProgram newProgram;
        newProgram.instructions = currentProgram.instructions;
        programs ~= newProgram;
    }

    final class DummyProblem : LevinProblem {
        final override void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
            hasHalted = false;
        }
    }

    LevinSearch levinSearch = new LevinSearch();
    levinSearch.reportProgramExecution = &recordProgram;
    levinSearch.numberOfInstructions = 4;
    levinSearch.c = 0.1;
    levinSearch.maxIterations = 5000;

    uint programLength = 2;

    levinSearch.instructionPropabilityMatrix.length = programLength;
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
        iterationArray.length = levinSearch.numberOfInstructions;
    }

    // set initial propaility matrix
    foreach( ref iterationArray; levinSearch.instructionPropabilityMatrix ) {
        foreach( i; 0..iterationArray.length ) {
            iterationArray[i] = 1.0f;
        }
    }

    bool done;
    uint numberOfIterations = 2;
    levinSearch.iterate(new DummyProblem, numberOfIterations, /*out*/ done);

    assert(programs.length == 4+4*4);

    uint runningIndex = 0;

    assert(programs[runningIndex++].instructions == [0]);
    assert(programs[runningIndex++].instructions == [1]);
    assert(programs[runningIndex++].instructions == [2]);
    assert(programs[runningIndex++].instructions == [3]);

    assert(programs[runningIndex++].instructions == [0, 0]);
    assert(programs[runningIndex++].instructions == [1, 0]);
    assert(programs[runningIndex++].instructions == [2, 0]);
    assert(programs[runningIndex++].instructions == [3, 0]);

    assert(programs[runningIndex++].instructions == [0, 1]);
    assert(programs[runningIndex++].instructions == [1, 1]);
    assert(programs[runningIndex++].instructions == [2, 1]);
    assert(programs[runningIndex++].instructions == [3, 1]);

    assert(programs[runningIndex++].instructions == [0, 2]);
    assert(programs[runningIndex++].instructions == [1, 2]);
    assert(programs[runningIndex++].instructions == [2, 2]);
    assert(programs[runningIndex++].instructions == [3, 2]);

    assert(programs[runningIndex++].instructions == [0, 3]);
    assert(programs[runningIndex++].instructions == [1, 3]);
    assert(programs[runningIndex++].instructions == [2, 3]);
    assert(programs[runningIndex++].instructions == [3, 3]);
}
