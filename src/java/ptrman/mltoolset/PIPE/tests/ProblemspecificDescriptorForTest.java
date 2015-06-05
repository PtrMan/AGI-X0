package mltoolset.PIPE.tests;

import java.util.ArrayList;
import java.util.Random;
import mltoolset.PIPE.PropabilisticPrototypeTree.Node;
import mltoolset.PIPE.program.Instruction;
import mltoolset.PIPE.program.Program;
import mltoolset.misc.Assert;

public class ProblemspecificDescriptorForTest implements mltoolset.PIPE.ProblemspecificDescriptor
{
    public ProblemspecificDescriptorForTest()
    {
        instructionPrototypes.add(new BinaryInstruction(BinaryInstruction.EnumOperation.ADD, 0));
        instructionPrototypes.add(new BinaryInstruction(BinaryInstruction.EnumOperation.SUB, 1));
        instructionPrototypes.add(new BinaryInstruction(BinaryInstruction.EnumOperation.MUL, 2));
        instructionPrototypes.add(new BinaryInstruction(BinaryInstruction.EnumOperation.DIV, 3));
        instructionPrototypes.add(new TrigonometricInstruction(TrigonometricInstruction.EnumType.SIN, 4));
        instructionPrototypes.add(new TerminalNumberConstantInstruction(0.0f, 5, new Random()));
        instructionPrototypes.add(new RealNumberInstruction(6));
    }
    
    @Override
    public int getNumberOfArgumentsOfInstruction(Instruction selectedInstruction) 
    {
        return selectedInstruction.getNumberOfParameters();
    }

    @Override
    public Instruction createTerminalNode(float randomConstant)
    {
        return new TerminalNumberConstantInstruction(randomConstant, 5, new Random());
    }

    @Override
    public Instruction getInstructionByIndex(int selectedInstruction)
    {
        Assert.Assert(selectedInstruction >= 0, "");
        Assert.Assert(selectedInstruction < instructionPrototypes.size(), "");
        
        return instructionPrototypes.get(selectedInstruction);
    }

    @Override
    public float createTerminalNodeFromProblemdependSet()
    {
        switch( random.nextInt(2) )
        {
            case 0:
            return 0.2f;
                
            case 1:
            return 0.3f;
            
            case 2:
            return 0.4f;
            
        }
        
        return 0.1f;
    }

    @Override
    public float getFitnessOfProgram(Program program)
    {
        float mse;
        float x;
        
        mse = 0.0f;
        
        for( x = -1.0f; x < 1.0f; x+=0.05f )
        {
            float diff;
            
            diff = evaluationTrainingFunctionAt(x) - evaluateProgramAt(program, x);
            mse += (diff*diff);
        }
        
        return mse;
    }
    
    private float evaluateProgramAt(Program program, float x)
    {
        return evaluateNodeAt(program.entry, x);
    }
    
    private static float evaluateNodeAt(mltoolset.PIPE.program.Node node, float x)
    {
        int instructionIndex;
        
        instructionIndex = node.getInstruction().getIndex();
        
        switch( instructionIndex )
        {
            case 0:
            return evaluateNodeAt(node.getChildrens().get(0), x) + evaluateNodeAt(node.getChildrens().get(1), x);
            
            case 1:
            return evaluateNodeAt(node.getChildrens().get(0), x) - evaluateNodeAt(node.getChildrens().get(1), x);
                
            case 2:
            return evaluateNodeAt(node.getChildrens().get(0), x) * evaluateNodeAt(node.getChildrens().get(1), x);
            
            case 3:
            return evaluateNodeAt(node.getChildrens().get(0), x) / evaluateNodeAt(node.getChildrens().get(1), x);
            
            case 4:
            return (float)Math.sin(evaluateNodeAt(node.getChildrens().get(0), x));
            
            case 5:
            return ((TerminalNumberConstantInstruction)node.getInstruction()).getValue();
            
            case 6:
            return x;
            
            default:
            throw new RuntimeException();
        }
    }
    
    
    @Override
    public String getDescriptionOfProgramAsString(Program program)
    {
        return getDescriptionOfNodeAsString(program.entry);
    }
    
    private String getDescriptionOfNodeAsString(mltoolset.PIPE.program.Node node)
    {
        int instructionIndex;
        
        instructionIndex = node.getInstruction().getIndex();
        
        switch( instructionIndex )
        {
            case 0:
            return "(" + getDescriptionOfNodeAsString(node.getChildrens().get(0)) + " + " + getDescriptionOfNodeAsString(node.getChildrens().get(1)) + ")";
            
            case 1:
            return "(" + getDescriptionOfNodeAsString(node.getChildrens().get(0)) + " - " + getDescriptionOfNodeAsString(node.getChildrens().get(1)) + ")";
                
            case 2:
            return "(" + getDescriptionOfNodeAsString(node.getChildrens().get(0)) + " * " + getDescriptionOfNodeAsString(node.getChildrens().get(1)) + ")";
            
            case 3:
            return "(" + getDescriptionOfNodeAsString(node.getChildrens().get(0)) + " / " + getDescriptionOfNodeAsString(node.getChildrens().get(1)) + ")";
            
            case 4:
            return "sin(" + getDescriptionOfNodeAsString(node.getChildrens().get(0)) + ")";
            
            case 5:
            return Float.toString(((TerminalNumberConstantInstruction)node.getInstruction()).getValue());
            
            case 6:
            return "x";
            
            default:
            throw new RuntimeException();
        }
    }
    
    private float evaluationTrainingFunctionAt(float x)
    {
        return (float)Math.sin(x) + (float)Math.sin(x*0.8f);
    }

    @Override
    public float getNumberOfInstructions()
    {
        return instructionPrototypes.size();
    }

    @Override
    public Node createPptNode()
    {
        Node result;
        
        result = new Node();
        result.propabilityVector = new float[7];
        result.propabilityVector[0] = 0.25f*0.3f;
        result.propabilityVector[1] = 0.25f*0.3f;
        result.propabilityVector[2] = 0.25f*0.3f;
        result.propabilityVector[3] = 0.25f*0.3f;
        
        result.propabilityVector[4] = 0.2f;
        
        result.propabilityVector[5] = 0.4f;
        
        result.propabilityVector[6] = 0.3f;
        
        result.randomConstant = 0.8f;
        return result;
    }
    
    private Random random = new Random();
    
    private ArrayList<Instruction> instructionPrototypes = new ArrayList<>();

    

}
