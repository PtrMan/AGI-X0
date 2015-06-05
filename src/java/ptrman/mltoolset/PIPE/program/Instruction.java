package mltoolset.PIPE.program;

import java.util.Random;

/**
 *
 */
public interface Instruction
{
    public int getNumberOfParameters();
    
    public int getIndex();
    
    public Instruction getInstance();
}
