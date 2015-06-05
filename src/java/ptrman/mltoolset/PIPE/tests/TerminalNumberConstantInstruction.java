
package mltoolset.PIPE.tests;

import java.util.Random;
import mltoolset.PIPE.program.Instruction;

/**
 *
 * 
 */
public class TerminalNumberConstantInstruction implements mltoolset.PIPE.program.Instruction
{
    public TerminalNumberConstantInstruction(float value, int index, Random random)
    {
        this.value = random.nextFloat();
        this.index = index;
        this.random = random;
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
    
    public float getValue()
    {
        return value;
    }
    
    private float value;
    private int index;
    private Random random;

    @Override
    public Instruction getInstance()
    {
        return new TerminalNumberConstantInstruction(random.nextFloat(), index, random);
    }
}
