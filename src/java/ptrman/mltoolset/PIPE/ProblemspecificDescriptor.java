package ptrman.mltoolset.PIPE;

import ptrman.mltoolset.PIPE.program.Instruction;
import ptrman.mltoolset.PIPE.program.Program;

public interface ProblemspecificDescriptor
{
    int getNumberOfArgumentsOfInstruction(Instruction selectedInstruction);
    
    ptrman.mltoolset.PIPE.program.Instruction createTerminalNode(float randomConstant);

    ptrman.mltoolset.PIPE.program.Instruction getInstructionByIndex(int selectedInstruction);

    float createTerminalNodeFromProblemdependSet();
    
    // lower fitness is better
    float getFitnessOfProgram(ptrman.mltoolset.PIPE.program.Program program);
    

    float getNumberOfInstructions();


    ptrman.mltoolset.PIPE.PropabilisticPrototypeTree.Node createPptNode();

    String getDescriptionOfProgramAsString(Program elitist);
    
}
