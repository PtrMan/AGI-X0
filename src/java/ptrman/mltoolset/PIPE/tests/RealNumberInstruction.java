package mltoolset.PIPE.tests;

import java.util.Random;
import mltoolset.PIPE.program.Instruction;


public class RealNumberInstruction implements mltoolset.PIPE.program.Instruction
{
    
    public RealNumberInstruction(int index)
    {
        this.index = index;
    }
    
    @Override
    public int getNumberOfParameters()
    {
        return 0;
    }

    @Override
    public int getIndex()
    {
        return index;
    }

    @Override
    public Instruction getInstance()
    {
        return new RealNumberInstruction(index);
    }
    
    private final int index;
}
