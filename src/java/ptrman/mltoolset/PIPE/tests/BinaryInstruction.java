package mltoolset.PIPE.tests;

import java.util.Random;
import mltoolset.PIPE.program.Instruction;

public class BinaryInstruction implements mltoolset.PIPE.program.Instruction
{
    public enum EnumOperation
    {
        ADD,
        SUB,
        MUL,
        DIV
    }
    
    public BinaryInstruction(EnumOperation operation, int index)
    {
        this.operation = operation;
        this.index = index;
    }
    
    @Override
    public int getNumberOfParameters()
    {
        return 2;
    }
    
    @Override
    public int getIndex()
    {
        return index;
    }
    
    @Override
    public Instruction getInstance()
    {
        return new BinaryInstruction(operation, index);
    }
    
    public EnumOperation getOperation()
    {
        return operation;
    }
    
    
    
    private EnumOperation operation;
    private final int index;

}
