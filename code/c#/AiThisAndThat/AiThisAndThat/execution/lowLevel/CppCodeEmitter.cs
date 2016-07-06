using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace execution.lowLevel {
    class CppCodeEmitter : ICodeEmitter {
        public void emitCodeForInstruction(VliwInstruction instruction, StringBuilder codeDrain) {
            codeDrain.Append("\t// performanceMetric: instructionCounter\n");
            codeDrain.Append("\tperformanceMetric.instructionCounter++;\n");
            
            codeDrain.Append("\t// instruction0\n");
            emitCodeForPrimaryInstruction(instruction.instruction0, codeDrain);
            codeDrain.Append("\t// instruction1\n");
            emitCodeForPrimaryInstruction(instruction.instruction1, codeDrain);

            // TODO< special instructions >

            codeDrain.Append("\t\n");
            codeDrain.Append("\t\n");
        }

        private void emitCodeForPrimaryInstruction(VliwInstruction.PrimaryInstruction instruction, StringBuilder codeDrain) {
            if (instruction.type == VliwInstruction.PrimaryInstruction.EnumInstructionType.NOTUSED) {
                return;
            }
            else if (instruction.type == VliwInstruction.PrimaryInstruction.EnumInstructionType.ADD) {
                codeDrain.AppendFormat("\t{0} = {1} + {2};\n", instruction.variableDestination, instruction.variableSources[0], instruction.variableSources[1]);
            }
            else if (instruction.type == VliwInstruction.PrimaryInstruction.EnumInstructionType.SUB) {
                codeDrain.AppendFormat("\t{0} = {1} - {2};\n", instruction.variableDestination, instruction.variableSources[0], instruction.variableSources[1]);
            }
            else if (instruction.type == VliwInstruction.PrimaryInstruction.EnumInstructionType.MUL) {
                codeDrain.AppendFormat("\t{0} = {1} * {2};\n", instruction.variableDestination, instruction.variableSources[0], instruction.variableSources[1]);
            }
            else if (instruction.type == VliwInstruction.PrimaryInstruction.EnumInstructionType.DIV) {
                codeDrain.AppendFormat("\t{0} = {1} / {2};\n", instruction.variableDestination, instruction.variableSources[0], instruction.variableSources[1]);
            }

        }
    }
}
