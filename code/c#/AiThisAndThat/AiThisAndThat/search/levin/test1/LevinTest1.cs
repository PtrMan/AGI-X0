using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LevinTest1 {
    public static void entry() {
        LevinSearch levinSearch = new LevinSearch();

        levinSearch.numberOfInstructions = 3;
        levinSearch.c = 0.3;
        levinSearch.maxIterations = 50000;

        levinSearch.instructionPropabilityMatrix = new double[2, levinSearch.numberOfInstructions];
        levinSearch.instructionPropabilityMatrix[0, 0] = 1.0f;
        levinSearch.instructionPropabilityMatrix[0, 1] = 1.0f;
        levinSearch.instructionPropabilityMatrix[0, 2] = 1.0f;
        levinSearch.instructionPropabilityMatrix[1, 0] = 1.0f;
        levinSearch.instructionPropabilityMatrix[1, 1] = 1.0f;
        levinSearch.instructionPropabilityMatrix[1, 2] = 1.0f;

        Test1Problem test1Problem = new Test1Problem();

        uint numberOfIterations = 2;
        bool done;
        levinSearch.iterate(test1Problem, numberOfIterations, out done);
        if( done ) {
            // TODO
        }
    }
}

