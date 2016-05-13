package ptrman.agix0.UsageCases;

import ptrman.agix0.Common.Evironment.Environment;

/**
 * A Usage Case describes the stimulation of the NN and the actions of the agent/component/etc on the environment
 */
public interface IUsageCase {
    int getNumberOfNeuralSimulationSteps();

    boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(Environment environment, final int stepCounter);

    // ...
    // neuroid simulation step
    // ...

    void afterNeuroidSimulationStep(Environment environment, final boolean[] hiddenNeuronActivation);


}
