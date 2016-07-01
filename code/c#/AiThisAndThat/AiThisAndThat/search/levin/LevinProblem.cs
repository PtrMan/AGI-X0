using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


abstract class LevinProblem {
    public abstract void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted);
}

