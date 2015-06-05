package ptrman.mltoolset.FlexibleNeuralTree;

import ptrman.mltoolset.misc.Assert;

import java.util.ArrayList;

/**
 *
 * algorithm after the one described in the book
 * "Tree-Structure Based Hybrid Computational Intelligence - Theoretical Foundations and Applications"
 */
public class FlexibleNeuralTree {
    private static float calcTotalExcitation(ArrayList<Float> weights, ArrayList<Float> values) {
        int i;
        float excitation;
        
        Assert.Assert(weights.size() == values.size(), "");
        
        excitation = 0.0f;
        
        for( i = 0; i < weights.size(); i++ ) {
            excitation += (weights.get(i) * values.get(i));
        }
        
        return excitation;
    }
    
    private static float calcOutputOfNode(float an, float bn, float netn) {
        float temp;
        
        temp = (netn - an)/bn;
        
        return (float)Math.exp(-temp*temp);
    }
}
