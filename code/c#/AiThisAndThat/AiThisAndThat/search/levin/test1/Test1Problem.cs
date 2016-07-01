using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Test1Problem : LevinProblem {
    public override void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
        hasHalted = false;

        Console.WriteLine("program to execute is {0}", program.instructions);
        Console.WriteLine("number of steps to execute= {0}", maxNumberOfStepsToExecute);

        if ( program.instructions.Count == 2 && program.instructions[0] == 1 && program.instructions[1] == 2) {
            Console.WriteLine("found solution");

            hasHalted = true;
        }
    }    
}
