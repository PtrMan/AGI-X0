package ptrman.agix0.UsageCases;

import ptrman.agix0.Common.Evironment.Environment;

public class NullUsageCase implements IUsageCase {
    @Override
    public int getNumberOfNeuralSimulationSteps() {
        return 0;
    }

    @Override
    public boolean[] beforeNeuroidSimationStepGetNeuroidInputForNextStep(Environment environment, int stepCounter) {
        return new boolean[0];
    }

    @Override
    public void afterNeuroidSimulationStep(Environment environment, boolean[] hiddenNeuronActivation) {
    }
}
