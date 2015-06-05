package ptrman.mltoolset.PIPE;

import ptrman.mltoolset.PIPE.program.Program;
import ptrman.mltoolset.misc.Assert;

import java.util.ArrayList;
import java.util.Random;

/**
 * PIPE algorithm
 * after the paper
 * "Probabilistic Incremental Program Evolution"
 */
public class PipeInstance {
    public void work(Parameters parameters, ProblemspecificDescriptor problemspecificDescriptor) {
        this.parameters = parameters;
        this.problemspecificDescriptor = problemspecificDescriptor;
        
        rootnode = problemspecificDescriptor.createPptNode();
        
        for(;;)
        {
            generationBasedLearningIteration();
            
            int x = 0;
        }
    }
    
    
    private void createProgramNodeFromPptNode(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode)
    {
        int selectedInstruction;

        selectedInstruction = selectInstructionIndexFromPptNode(pptNode);

        if( selectedInstruction != parameters.grcIndex )
        {
            programNode.setInstruction(problemspecificDescriptor.getInstructionByIndex(selectedInstruction));
        }

        // special handling for grc
        if( selectedInstruction == parameters.grcIndex )
        {
            if (pptNode.randomConstant > parameters.randomThreshold)
            {
                programNode.setInstruction(problemspecificDescriptor.createTerminalNode(pptNode.randomConstant));
            }
            else
            {
                programNode.setInstruction(problemspecificDescriptor.createTerminalNode(problemspecificDescriptor.createTerminalNodeFromProblemdependSet()));
            }
        }
        else
        {
            int i;

            for( i = 0; i < problemspecificDescriptor.getNumberOfArgumentsOfInstruction(problemspecificDescriptor.getInstructionByIndex(selectedInstruction)); i++ )
            {
                ptrman.mltoolset.PIPE.program.Node createdProgramNode;

                // create node if it is not present
                if (pptNode.childrens.size() < i + 1)
                {
                    pptNode.childrens.add(createPptNode());
                }
                
                if (pptNode.childrens.get(i) == null)
                {
                    pptNode.childrens.set(i, createPptNode());
                }
                
                createdProgramNode = new ptrman.mltoolset.PIPE.program.Node();
                
                Assert.Assert(programNode.getChildrens().size() >= i, "");
                if( programNode.getChildrens().size() == i )
                {
                    programNode.getChildrens().add(null);
                }
                
                programNode.getChildrens().set(i, createdProgramNode);
                
                // call recursivly for the childnodes
                createProgramNodeFromPptNode(pptNode.childrens.get(i), programNode.getChildrens().get(i));
            }
        }
    }

    private int selectInstructionIndexFromPptNode(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode)
    {
        float randomValue;
        float accumulated;
        int i;
        
        Assert.Assert(pptNode.propabilityVector.length > 0, "");
        
        randomValue = random.nextFloat();
        accumulated = 0.0f;
        
        for (i = 0; i < pptNode.propabilityVector.length; i++)
        {
            accumulated += pptNode.propabilityVector[i];

            Assert.Assert(accumulated > 0.0f, "");
            
            if (accumulated > randomValue)
            {
                return i;
            }
        }

        return -1; // in case of grc
    }

    private ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node createPptNode()
    {
        return problemspecificDescriptor.createPptNode();
    }

    private static float calcProgramPropability(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode)
    {
        float propability;
        int i;
        
        propability = pptNode.propabilityVector[programNode.getInstruction().getIndex()];

        for( i = 0; i < programNode.getChildrens().size(); i++ )
        {
            propability *= calcProgramPropability(pptNode.childrens.get(i), programNode.getChildrens().get(i));
        }

        return propability;
    }

    private void increaseProgramPropability(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode)
    {
        int i;
        float propabilityDelta;

        propabilityDelta = (parameters.learningRate * parameters.learningRateConstant * (1.0f - pptNode.propabilityVector[programNode.getInstruction().getIndex()]));
        pptNode.propabilityVector[programNode.getInstruction().getIndex()] += propabilityDelta;

        for (i = 0; i < problemspecificDescriptor.getInstructionByIndex(programNode.getInstruction().getIndex()).getNumberOfParameters(); i++)
        {
            increaseProgramPropability(pptNode.childrens.get(i), programNode.getChildrens().get(i));
        }
    }

    private void increaseNodePropabilitiesUntilTargetPropabilityIsReached(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode, float targetPropability)
    {
        float currentPropability;
        
        currentPropability = calcProgramPropability(pptNode, programNode);

        while (currentPropability < targetPropability)
        {
            increaseProgramPropability(pptNode, programNode);
            currentPropability = calcProgramPropability(pptNode, programNode);
        }
    }

    private void normalizePropabilities(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode)
    {
        float propabilitySum;
        float oldPropability;
        float gamma;
        int i;

        propabilitySum = pptNode.getSumOfPropabilities();
        oldPropability = pptNode.propabilityVector[programNode.getInstruction().getIndex()] - (propabilitySum - 1.0f);
        gamma = calcGamma(pptNode, programNode, oldPropability);

        for (i = 0; i < pptNode.propabilityVector.length; i++)
        {
            if (i != programNode.getInstruction().getIndex())
            {
                pptNode.propabilityVector[i] *= (1.0f - gamma);
                ptrman.mltoolset.misc.Assert.Assert(pptNode.propabilityVector[i] <= 1.0f && pptNode.propabilityVector[i] >= 0.0f, "");
            }
        }

        for (i = 0; i < problemspecificDescriptor.getInstructionByIndex(programNode.getInstruction().getIndex()).getNumberOfParameters(); i++)
        {
            normalizePropabilities(pptNode.childrens.get(i), programNode.getChildrens().get(i));
        }
    }

    private static float calcGamma(ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node pptNode, ptrman.mltoolset.PIPE.program.Node programNode, float oldPropability)
    {
        if (oldPropability == 1.0f)
        {
            return 0.0f;
        }
        else
        {
            return (pptNode.propabilityVector[programNode.getInstruction().getIndex()] - oldPropability) / (1.0f - oldPropability);
        }
    }

    private void mutationOfPrototypeTree(ptrman.mltoolset.PIPE.program.Program bestProgram)
    {
        float mutationPropability;
        float bestRating;
        
        // ASK< redudant ? >
        bestRating = problemspecificDescriptor.getFitnessOfProgram(bestProgram);

        mutationPropability = parameters.mutationPropability / (problemspecificDescriptor.getNumberOfInstructions() * (float) Math.sqrt(bestRating));

        mutateProgram(bestProgram, mutationPropability);
    }

    private void mutateProgram(ptrman.mltoolset.PIPE.program.Program bestProgram, float mutationPropability)
    {
        mutateNode(bestProgram.entry, mutationPropability);
    }
    
    private void mutateNode(ptrman.mltoolset.PIPE.program.Node bestProgram, float mutationPropability)
    {
        boolean changed;
        int i;

        changed = false;

        for (i = 0; i < problemspecificDescriptor.getNumberOfInstructions(); i++)
        {
            boolean mutate;

            mutate = mutationPropability < random.nextFloat();
            
            if( mutate )
            {
                rootnode.propabilityVector[i] += (parameters.mutationRate * (1.0f - rootnode.propabilityVector[i]));
            }
            
            changed |= mutate;
        }
        
        // normalize
        if( changed )
        {
            rootnode.normalizePropabilities();
        }
        
        for( ptrman.mltoolset.PIPE.program.Node iterationNode : bestProgram.getChildrens() )
        {
            if( iterationNode != null )
            {
                mutateNode(iterationNode, mutationPropability);
            }
        }
    }
    
    // 4.3.3 Generation-Based Learning
    private void generationBasedLearningIteration()
    {
        int i;
        ArrayList<ptrman.mltoolset.PIPE.program.Program> population;
        Program bestProgram;
        
        population = new ArrayList<>();
        
        // (1) creation of program population

        ptrman.mltoolset.misc.Assert.Assert(parameters.populationSize > 0, "populationsize must be >= 1");
        for( i = 0; i < parameters.populationSize; i++ )
        {
            ptrman.mltoolset.PIPE.program.Program createdProgram;
            
            createdProgram = new Program();
            createdProgram.entry = new ptrman.mltoolset.PIPE.program.Node();
            createProgramNodeFromPptNode(rootnode, createdProgram.entry);
            population.add(createdProgram);
        }
        
        
        
        // (2) population evaluation
        
        for( Program iterationProgram : population )
        {
            iterationProgram.fitness = problemspecificDescriptor.getFitnessOfProgram(iterationProgram);
        }
        
        System.out.println("");
        System.out.println("candidates:");
        
        for( i = 0; i < parameters.populationSize; i++ )
        {
            System.out.println(population.get(i).fitness);
            System.out.println(problemspecificDescriptor.getDescriptionOfProgramAsString(population.get(i)));
        }
        
        System.out.println("---");
        
        bestProgram = population.get(0);
        for( Program iterationProgram : population )
        {
            if( iterationProgram.fitness < bestProgram.fitness )
            {
                bestProgram = iterationProgram;
            }
        }
        
        if( elitist == null )
        {
            elitist = bestProgram;
        }
        else
        {
            if( bestProgram.fitness < elitist.fitness )
            {
                elitist = bestProgram;
            }
        }
        
        System.out.println(elitist.fitness);
        System.out.println(problemspecificDescriptor.getDescriptionOfProgramAsString(elitist));
        
        // (3) learning from population

        // TODO< belongs into own method >
        float targetPropability;
        
        float currentPropability;

        currentPropability = calcProgramPropability(rootnode, bestProgram.entry);
        
        targetPropability = currentPropability + (1.0f - currentPropability) * parameters.learningRate *  ((parameters.epsilon+elitist.fitness)/(parameters.epsilon+bestProgram.fitness));
        
        increaseNodePropabilitiesUntilTargetPropabilityIsReached(rootnode, bestProgram.entry, targetPropability);
        normalizePropabilities(rootnode, bestProgram.entry);
        // TODO< learn constants >
        
        // (4) mutation of prototype tree
        mutationOfPrototypeTree(bestProgram);
        
        // (5) prototype tree pruning
        // TODO
        
    }
    
    // is at the beginning null
    private ptrman.mltoolset.PIPE.program.Program elitist;

    private ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node rootnode;

    private ProblemspecificDescriptor problemspecificDescriptor;
    private Parameters parameters;

    private Random random = new Random();

}
