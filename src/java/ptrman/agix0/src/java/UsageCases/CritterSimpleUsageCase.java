package ptrman.agix0.src.java.UsageCases;


import ptrman.agix0.src.java.Common.Evironment.Environment;

/**
 *
 */
public class CritterSimpleUsageCase implements IUsageCase {
    @Override
    public int getNumberOfNeuralSimulationSteps() {
        return 60;
    }

    @Override
    public boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(final int stepCounter) {
        final boolean stimulation;

        if( (stepCounter % 5) == 0 ) {
            stimulation = true;
        }
        else {
            stimulation = false;
        }

        return new boolean[]{stimulation};
    }

    @Override
    public void afterNeuroidSimulationStep(Environment environment, boolean[] hiddenNeuronActivation) {
        final boolean moveForwardNeuralSignal = hiddenNeuronActivation[hiddenNeuronActivation.length-1];

        if( moveForwardNeuralSignal ) {
            // TODO
            //environment.entities.get(0).speed = 1.0f;
        }
        else {
            // TODO
            //environment.entities.get(0).speed = 0.0f;
        }
    }
}
