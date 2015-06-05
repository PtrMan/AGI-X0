package ptrman.mltoolset.PIPE.tests;


import ptrman.mltoolset.PIPE.program.Instruction;

/**
 *
 *
 */
public class TrigonometricInstruction implements ptrman.mltoolset.PIPE.program.Instruction {

    @Override
    public Instruction getInstance() {
        return new TrigonometricInstruction(type, index);
    }
    
    public enum EnumType {
        SIN
    }
    
    public TrigonometricInstruction(EnumType type, int index) {
        this.type = type;
        this.index = index;
    }
    
    
    @Override
    public int getNumberOfParameters() {
        return 1;
    }

    @Override
    public int getIndex() {
        return index;
    }

    private final EnumType type;
    private final int index;
}
