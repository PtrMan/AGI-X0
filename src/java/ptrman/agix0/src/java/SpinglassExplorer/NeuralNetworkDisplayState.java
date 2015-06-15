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

    public void resetIntegratedActivation() {
        for( int i = 0; i < integratedActiviationOfInputNeurons.length; i++ ) {
            integratedActiviationOfInputNeurons[i] = 0.0f;
        }

        for( int i = 0; i < integratedActiviationOfHiddenNeurons.length; i++ ) {
            integratedActiviationOfHiddenNeurons[i] = 0.0f;
        }
    }

    public void integrate(int steps, boolean[] activationOfInputNeurons, boolean[] activationOfHiddenNeurons) {
        float stepsDiv1 = 1.0f / (float)steps;

        for( int i = 0; i < integratedActiviationOfInputNeurons.length; i++ ) {
            if( activationOfInputNeurons[i] ) {
                integratedActiviationOfInputNeurons[i] += stepsDiv1;
            }
        }

        for( int i = 0; i < integratedActiviationOfHiddenNeurons.length; i++ ) {
            if( activationOfHiddenNeurons[i] ) {
                integratedActiviationOfHiddenNeurons[i] += stepsDiv1;
            }
        }
    }

    public float[] integratedActiviationOfInputNeurons;
    public float[] integratedActiviationOfHiddenNeurons;
}
