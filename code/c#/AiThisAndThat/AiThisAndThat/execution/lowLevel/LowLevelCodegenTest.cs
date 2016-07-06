using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace execution.lowLevel {
    class LowLevelCodegenTest {
        static public void test() {
            VliwInstruction[] instructions = new VliwInstruction[2];
            instructions[0] = new VliwInstruction();
            instructions[0].instruction0 = new VliwInstruction.PrimaryInstruction(VliwInstruction.PrimaryInstruction.EnumInstructionType.ADD, "int");
            instructions[0].instruction0.variableDestination = "temp0";
            instructions[0].instruction0.variableSources[0] = "a";
            instructions[0].instruction0.variableSources[1] = "b";

            instructions[0].instruction1 = new VliwInstruction.PrimaryInstruction(VliwInstruction.PrimaryInstruction.EnumInstructionType.ADD, "int");
            instructions[0].instruction1.variableDestination = "temp1";
            instructions[0].instruction1.variableSources[0] = "c";
            instructions[0].instruction1.variableSources[1] = "d";


            instructions[1] = new VliwInstruction();
            instructions[1].instruction0 = new VliwInstruction.PrimaryInstruction(VliwInstruction.PrimaryInstruction.EnumInstructionType.ADD, "int");
            instructions[1].instruction0.variableDestination = "temp2";
            instructions[1].instruction0.variableSources[0] = "temp0";
            instructions[1].instruction0.variableSources[1] = "temp1";

            instructions[1].instruction1 = new VliwInstruction.PrimaryInstruction(VliwInstruction.PrimaryInstruction.EnumInstructionType.NOTUSED, "int");

            StringBuilder stringbuilder = new StringBuilder();

            ICodeEmitter codeEmitter = new CppCodeEmitter();

            codeEmitter.emitCodeForInstruction(instructions[0], stringbuilder);
            codeEmitter.emitCodeForInstruction(instructions[1], stringbuilder);

            Console.Write(stringbuilder.ToString());

            int debugMeHere = 1;
        }
    }
}
