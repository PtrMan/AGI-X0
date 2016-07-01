using System;
using System.Collections.Generic;

// see paper "Shifting inductive bias with success-story algorithm, adaptive levin search and incremental self improvement"
// for the description of levin search implemented here

// TODO< make it more efficient maybe by reversing the order of the instructions and putting them into a tree >
class LevinSearch {
    public LevinSearch() {
        reset();
    }

    public void reset() {
        phase = 1;
        currentIteration = 0;
        previousLevinPrograms = new List<LevinProgram>();
    }

    public class Solution {
        public Solution(LevinProgram program) {
            this.program = program;
        }

        public LevinProgram program;
    }

    public Solution iterate(LevinProblem problem, uint numberOfIterations, out bool done) {
        done = false;

        for ( uint i = 0; i < numberOfIterations; i++ ) {
            Solution solution = iteration(problem, out done);
            if( done ) {
                return solution;
            }
        }

        return null;
    }

    protected Solution iteration(LevinProblem problem, out bool done) {
        done = false;

        if( currentIteration > maxIterations ) {
            done = true;
            return null;
        }

        List<LevinProgram> currentLevinPrograms = new List<LevinProgram>();

        if( currentIteration == 0 ) {
            // we have to seed the current levin programs
            for (uint instructionI = 0; instructionI < numberOfInstructions; instructionI++) {
                LevinProgram program = new LevinProgram();
                program.instructions = new List<uint> { instructionI };

                currentLevinPrograms.Add(program);
            }
        }
        else {
            // form new programs we haven't jet executed
            for (uint instructionI = 0; instructionI < numberOfInstructions; instructionI++) {
                currentLevinPrograms.AddRange(concatInstructionBeforePreviousLevinPrograms(instructionI));
            }
        }

        

        // TODO< order programs by propability ? >


        // execute programs
        foreach(LevinProgram iterationProgram in currentLevinPrograms) {
            uint maxNumberOfStepsToExecute = (uint)((calcSolomonoffLevinMeasure(iterationProgram) * (double)phase) / c);

            bool hasHalted;
            executeProgram(iterationProgram, problem, maxNumberOfStepsToExecute, out hasHalted);
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
    
    private void executeProgram(LevinProgram program, LevinProblem problem, uint maxNumberOfStepsToExecute, out bool hasHalted) {
        problem.executeProgram(program, maxNumberOfStepsToExecute, out hasHalted);
    }

    protected List<LevinProgram> concatInstructionBeforePreviousLevinPrograms(uint instruction) {
        List<LevinProgram> result = new List<LevinProgram>();

        foreach(LevinProgram iterationPreviousLevinProgram in previousLevinPrograms) {
            LevinProgram modified = iterationPreviousLevinProgram.copy();
            modified.instructions.Insert(0, instruction);
            result.Add(modified);
        }

        return result;
    }


    // [1] https://en.wikipedia.org/wiki/Algorithmic_probability
    // [2] http://wiki.opencog.org/w/OpenCogPrime:OccamsRazor

    // approximation for not computable "solomonoff levin measure"
    // see mainly [2] and secondary [1]
    protected double calcSolomonoffLevinMeasure(LevinProgram program) {
        double prod = 1.0;

        for(int instructionI = 0; instructionI < program.instructions.Count; instructionI++) {
            prod *= instructionPropabilityMatrix[instructionI, program.instructions[instructionI]];
        }
        //sum += Math.Pow(2.0f, -(double)program.instructions.Count);
        
        return prod;
    }


    uint phase;
    uint currentIteration;

    public uint maxIterations = 0;
    public uint numberOfInstructions = 0;
    public double c = 0.0;

    protected List<LevinProgram> previousLevinPrograms;

    // [position in program, op]
    public double[,] instructionPropabilityMatrix;
}

