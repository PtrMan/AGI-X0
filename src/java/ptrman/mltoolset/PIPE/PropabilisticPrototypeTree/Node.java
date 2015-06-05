package ptrman.mltoolset.PIPE.PropabilisticPrototypeTree;

import java.util.ArrayList;

/**
 *
 *
 */
public class Node {
    public float randomConstant;
    
    // must sum up to 1.0
    public float propabilityVector[];
    
    public ArrayList<Node> childrens = new ArrayList<>();
    
    public float getSumOfPropabilities() {
        int i;
        float sum;
        
        sum = 0.0f;
        
        for( i = 0; i < propabilityVector.length; i++ ) {
            sum += propabilityVector[i];
        }
        
        return sum;
    }

    public void normalizePropabilities() {
        float sum, invSum;
        int i;
        
        sum = getSumOfPropabilities();
        invSum = 1.0f/sum;
        
        for( i = 0; i < propabilityVector.length; i++ ) {
            propabilityVector[i] *= invSum;
        }
    }
    
    public Node cloneThis() {
        Node result;
        int i;
        
        result = new Node();
        result.randomConstant = randomConstant;
        result.propabilityVector = new float[result.propabilityVector.length];
        for( i = 0; i < result.propabilityVector.length; i++ ) {
            result.propabilityVector[i] = propabilityVector[i];
        }
        
        return result;
    }
}
