package mltoolset.FlexibleNeuralTree.PipeInstructions;

import mltoolset.PIPE.program.Instruction;

/**
 *
 * returns a (at construction time determined) value and returns it
 */
public class SelectInputInstruction implements mltoolset.PIPE.program.Instruction
{
    
    public SelectInputInstruction(int index, int variableIndex)
    {
        this.index = index;
        this.variableIndex = variableIndex;
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
        return new SelectInputInstruction(index, variableIndex);
    }
    
    private final int index;
    private final int variableIndex;
}
