package ptrman.agix0.Common;

import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

/**
 * A component of the "brain"
 * can be a neural network or a more symbolic mechanism.
 *
 */
public class Component {
    public Neuroid<Float, Integer> neuroidNetwork;

    private NeuroidNetworkDescriptor networkDescriptor;

    public NeuroidNetworkDescriptor getNeuralNetworkDescriptor() {
        return networkDescriptor;
    }

    public void setupNeuroidNetwork(NeuroidNetworkDescriptor networkDescriptor) {
        // we store the NeuroidNetworkDescriptor for Spinglass
        this.networkDescriptor = networkDescriptor;

        neuroidNetwork = NeuroidCommon.createNeuroidNetworkFromDescriptor(networkDescriptor);
    }

    public void setStimulus(boolean[] values) {
        neuroidNetwork.input = values;
    }

    public boolean[] getActiviationOfNeurons() {
        return neuroidNetwork.getActiviationOfNeurons();
    }

    public void timestep() {
        cellularAutomataTimestep();

        neuroidNetwork.timestep();
    }

    // simulate all cellular automata of all CA neurons
    private void cellularAutomataTimestep() {
        // TODO
    }
}
