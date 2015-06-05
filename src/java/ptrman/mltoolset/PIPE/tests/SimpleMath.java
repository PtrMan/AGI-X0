package ptrman.mltoolset.PIPE.tests;

import ptrman.mltoolset.PIPE.Parameters;
import ptrman.mltoolset.PIPE.PipeInstance;

public class SimpleMath {
    public static void main(String[] args) 
    {
        test();
    }
    
    public static void test() {
        Parameters parameters;
        ProblemspecificDescriptorForTest problemspecificDescriptor;
        
        parameters = new Parameters();
        parameters.populationSize = 1;
        parameters.learningRate = 0.3f;
        parameters.learningRateConstant = 0.1f; // like in paper
        parameters.mutationPropability = 0.08f;
        parameters.mutationRate = 0.1f;
        parameters.epsilon = 0.01f;
        parameters.randomThreshold = 5.0f; // impossible high
        
        parameters.grcIndex = 5;
        
        problemspecificDescriptor = new ProblemspecificDescriptorForTest();
        
        PipeInstance pipeInstance = new PipeInstance();
        pipeInstance.work(parameters, problemspecificDescriptor);
    }
}
