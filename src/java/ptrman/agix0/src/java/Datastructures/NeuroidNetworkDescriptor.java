package ptrman.agix0.src.java.Datastructures;

import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

/**
 * Describes the (static) aspects of an Neuroid network.
 *
 * For the dynamic alspect the network need to be translated to "real" neurons and "real" connections.
 * The dynamics of the neurons and connections can then be simulated with the CPU/GPU.
 */
public class NeuroidNetworkDescriptor {
    public NeuronDescriptor[] hiddenNeurons;

    public List<Neuroid.Helper.EdgeWeightTuple<Float>> connections = new ArrayList<>();

    public int getNumberOfHiddenNeurons() {
        return hiddenNeurons.length;
    }

    public int numberOfInputNeurons;

    public float randomFiringPropability;

    public int neuronLatencyMin;
    public int neuronLatencyMax;

    public float neuronThresholdMin;
    public float neuronThresholdMax;
}
