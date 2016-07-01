using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LevinProgram {
    public List<uint> instructions = new List<uint>();

    public LevinProgram copy() {
        LevinProgram result = new LevinProgram();

        foreach(uint iterationInstruction in instructions) {
            result.instructions.Add(iterationInstruction);
        }

        return result;
    }
}

