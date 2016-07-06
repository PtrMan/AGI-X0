using System;

namespace execution.lowLevel {
    class VliwInstruction {
        public struct PrimaryInstruction {
            public enum EnumInstructionType {
                NOTUSED = 0,

                ADD,
                SUB,
                MUL,
                DIV
            }

            public PrimaryInstruction(EnumInstructionType instructionType, string datatype) {
                this.type = instructionType;
                this.datatype = datatype;
                this.variableSources = new string[2];
                this.variableDestination = null;
            }

            public string datatype;
            
            public EnumInstructionType type;
            public string variableDestination;
            public string []variableSources;
        }

        public PrimaryInstruction instruction0, instruction1;

        // TODO< instructions for comparision and jumps and invocations >
    }
}
