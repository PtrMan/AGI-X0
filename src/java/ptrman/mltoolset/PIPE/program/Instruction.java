package ptrman.mltoolset.PIPE.program;

/**
 *
 */
public interface Instruction
{
    int getNumberOfParameters();
    int getIndex();
    Instruction getInstance();
}
