package ptrman.agix0.src.java.SpinglassExplorer;

import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;

/**
 *
 */
public class NeuralNetworkDisplayState {
    //public boolean[] activationInputNeurons;
    //public boolean[] activationHiddenNeurons;

    public void allocateAfterDescriptor(NeuroidNetworkDescriptor neuralNetworkDescriptor) {
        integratedActiviationOfInputNeurons = new float[neuralNetworkDescriptor.numberOfInputNeurons];
        integratedActiviationOfHiddenNeurons = new float[neuralNetworkDescriptor.getNumberOfHiddenNeurons()];
    }

    public float[] integratedActiviationOfInputNeurons;
    public float[] integratedActiviationOfHiddenNeurons;
}
