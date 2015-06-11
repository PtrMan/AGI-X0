package ptrman.agix0.src.java.UsageCases;

import ptrman.agix0.src.java.Evolvator.Evironment.Playground;

/**
 * A Usage Case describes the stimulation of the NN and the actions of the agent/component/etc on the environment
 */
public interface IUsageCase {
    int getNumberOfNeuralSimulationSteps();

    boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(final int stepCounter);

    // ...
    // neuroid simulation step
    // ...

    void afterNeuroidSimulationStep(Playground playground, final boolean[] hiddenNeuronActivation);
}
