package ptrman.mltoolset.FlexibleNeuralTree.PipeInstructions;

import java.util.Random;


/**
 *
 * 
 */
public class TreeNodeInstruction implements ptrman.mltoolset.PIPE.program.Instruction {
    private final Random random;
    
    public TreeNodeInstruction(int numberOfParameters, int index, Random random) {
        weights = new float[numberOfParameters];
        this.index = index;
        this.random = random;
    }
    
    public float weights[];
    public float a, b;
    
    private final int index;
    
    @Override
    public int getNumberOfParameters()
    {
        return weights.length;
    }

    @Override
    public int getIndex()
    {
        return index;
    }

    @Override
    public ptrman.mltoolset.PIPE.program.Instruction getInstance() {
        TreeNodeInstruction result;
        int i;
        
        result = new TreeNodeInstruction(weights.length, index, random);
        
        // initialize weights by random
        for( i = 0; i < weights.length; i++ ) {
            result.weights[i] = 0.05f + random.nextFloat()*(1.0f - 0.05f);
            result.a = 0.05f + random.nextFloat()*(1.0f - 0.05f);
            result.b = 0.05f + random.nextFloat()*(1.0f - 0.05f);
        }
        
        return result;
    }
}
