module search.levin.LevinSearch;

import std.array : insertInPlace;

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

// translated from the C# version

// see paper "Shifting inductive bias with success-story algorithm, adaptive levin search and incremental self improvement"
// for the description of levin search implemented here

// TODO< make it more efficient maybe by reversing the order of the instructions and putting them into a tree >
class LevinSearch {
    final this() {
        reset();
    }

    final void reset() {
        phase = 1;
        currentIteration = 0;
        previousLevinPrograms = [];
    }

    final static class Solution {
        final this(LevinProgram program) {
            this.program = program;
        }

        LevinProgram program;
    }

    final Solution iterate(LevinProblem problem, uint numberOfIterations, out bool done) {
        done = false;

        for ( uint i = 0; i < numberOfIterations; i++ ) {
            Solution solution = iteration(problem, /*out*/ done);
            if( done ) {
                return solution;
            }
        }

        return null;
    }

    protected final Solution iteration(LevinProblem problem, out bool done) {
        done = false;

        if( currentIteration > maxIterations ) {
            done = true;
            return null;
        }

        LevinProgram[] currentLevinPrograms;

        if( currentIteration == 0 ) {
            // we have to seed the current levin programs
            for (uint instructionI = 0; instructionI < numberOfInstructions; instructionI++) {
                LevinProgram program = new LevinProgram();
                program.instructions = [instructionI];

                currentLevinPrograms ~= program;
            }
        }
        else {
            // form new programs we haven't jet executed
            for (uint instructionI = 0; instructionI < numberOfInstructions; instructionI++) {
                currentLevinPrograms ~= concatInstructionBeforePreviousLevinPrograms(instructionI);
            }
        }

        

        // TODO< order programs by propability ? >


        // execute programs
        foreach(LevinProgram iterationProgram; currentLevinPrograms) {
            uint maxNumberOfStepsToExecute = cast(uint)((calcSolomonoffLevinMeasure(iterationProgram) * cast(double)phase) / c);

            bool hasHalted;
            executeProgram(iterationProgram, problem, maxNumberOfStepsToExecute, /*out*/ hasHalted);
            if( hasHalted ) {
                done = true;
                return new Solution(iterationProgram);
            }
        }


        previousLevinPrograms = currentLevinPrograms;
        phase *= 2;
        currentIteration++;

        return null;
    }
    
    private final void executeProgram(LevinProgram program, LevinProblem problem, uint maxNumberOfStepsToExecute, out bool hasHalted) {
        problem.executeProgram(program, maxNumberOfStepsToExecute, /*out*/ hasHalted);
    }

    protected final LevinProgram[] concatInstructionBeforePreviousLevinPrograms(uint instruction) {
        LevinProgram[] result;

        foreach(LevinProgram iterationPreviousLevinProgram; previousLevinPrograms) {
            LevinProgram modified = iterationPreviousLevinProgram.copy();
            modified.instructions.insertInPlace(0, instruction);
            result ~= modified;
        }

        return result;
    }


    // [1] https://en.wikipedia.org/wiki/Algorithmic_probability
    // [2] http://wiki.opencog.org/w/OpenCogPrime:OccamsRazor

    // approximation for not computable "solomonoff levin measure"
    // see mainly [2] and secondary [1]
    protected final double calcSolomonoffLevinMeasure(LevinProgram program) {
        double prod = 1.0;

        for(int instructionI = 0; instructionI < program.instructions.length; instructionI++) {
            prod *= instructionPropabilityMatrix[instructionI][program.instructions[instructionI]];
        }
        //sum += Math.Pow(2.0f, -(double)program.instructions.length);
        
        return prod;
    }


    uint phase;
    uint currentIteration;

    public uint maxIterations = 0;
    public uint numberOfInstructions = 0;
    public double c = 0.0;

    protected LevinProgram[] previousLevinPrograms;

    // [position in program, op]
    public double[][] instructionPropabilityMatrix;
}
