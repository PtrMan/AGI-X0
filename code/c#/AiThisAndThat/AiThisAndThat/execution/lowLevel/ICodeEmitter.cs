using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace execution.lowLevel {
    interface ICodeEmitter {
        void emitCodeForInstruction(VliwInstruction instruction, StringBuilder codeDrain);
    }
}
