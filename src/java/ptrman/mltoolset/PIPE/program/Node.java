package ptrman.mltoolset.PIPE.program;

import ptrman.mltoolset.misc.Assert;

import java.util.ArrayList;

/**
 *
 * 
 */
public class Node {
    public void setInstruction(Instruction instruction) {
        Assert.Assert(instruction != null, "");
        
        this.instruction = instruction;
    }
    
    public Instruction getInstruction()
    {
        return instruction;
    }
    
    public ArrayList<Node> getChildrens()
    {
        return childrens;
    }
    
    
    public int getNumberOfNodesRecursive() {
        int numberOfNodes;
        
        numberOfNodes = 1;
        
        for( Node iterationNode : getChildrens() )
        {
            numberOfNodes += iterationNode.getNumberOfNodesRecursive();
        }
        
        return numberOfNodes;
    }
    
    private ArrayList<Node> childrens = new ArrayList<>();
    private Instruction instruction = null;
}
