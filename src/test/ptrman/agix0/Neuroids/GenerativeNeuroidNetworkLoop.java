package ptrman.agix0.Neuroids;

import org.junit.Assert;
import org.junit.Test;
import ptrman.agix0.Common.NeuroidCommon;
import ptrman.agix0.Evolvator.NetworkGlobalState;
import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuronDescriptor;
import ptrman.agix0.Neuroids.NeuroidNetworkManipulation.GenerativeNeuroidNetworkTransformator;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class GenerativeNeuroidNetworkLoop {
    /**
     * checks if a indirectly described loop gets wired correctly and works in the Neuroid network
     *
     */
    @Test
    public void testLoop() {
        // initialize template neuron
        NetworkGlobalState.templateNeuronDescriptor = new NeuronDescriptor();
        NetworkGlobalState.templateNeuronDescriptor.firingLatency = 0;
        NetworkGlobalState.templateNeuronDescriptor.firingThreshold = 0.4f;

        // initialize template neuroid network
        NeuroidNetworkDescriptor templateNeuroidNetwork = new NeuroidNetworkDescriptor();
        templateNeuroidNetwork.numberOfInputNeurons = 1; // we exite the hidden neurons directly
        templateNeuroidNetwork.numberOfOutputNeurons = 1; // we read out the impulse directly

        templateNeuroidNetwork.neuronLatencyMin = 0;
        templateNeuroidNetwork.neuronLatencyMax = 0;

        templateNeuroidNetwork.neuronThresholdMin = 0.4f;
        templateNeuroidNetwork.neuronThresholdMax = 0.4f;

        templateNeuroidNetwork.connectionDefaultWeight = 0.5f;



        List<Integer> neuronFamily = new ArrayList<>();
        neuronFamily.add(5);
        GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor = GenerativeNeuroidNetworkDescriptor.createAfterFamily(neuronFamily, NetworkGlobalState.templateNeuronDescriptor);

        generativeNeuroidNetworkDescriptor.neuronClusters.get(0).addLoop(GenerativeNeuroidNetworkDescriptor.NeuronCluster.EnumDirection.ANTICLOCKWISE);

        generativeNeuroidNetworkDescriptor.inputConnections = new GenerativeNeuroidNetworkDescriptor.OutsideConnection[1];
        generativeNeuroidNetworkDescriptor.inputConnections[0] = new GenerativeNeuroidNetworkDescriptor.OutsideConnection(0, 0);

        generativeNeuroidNetworkDescriptor.outputConnections = new GenerativeNeuroidNetworkDescriptor.OutsideConnection[0];

        // generate network
        NeuroidNetworkDescriptor neuroidNetworkDescriptor = GenerativeNeuroidNetworkTransformator.generateNetwork(templateNeuroidNetwork, generativeNeuroidNetworkDescriptor);

        Neuroid<Float, Integer> neuroidNetwork = NeuroidCommon.createNeuroidNetworkFromDescriptor(neuroidNetworkDescriptor);

        // simulate and test

        // we stimulate the first neuron and wait till it looped around to the last neuron, then the first neuron again
        neuroidNetwork.input[0] = true;

        // propagate the input to the first neuron
        neuroidNetwork.timestep();

        neuroidNetwork.input[0] = false;

        neuroidNetwork.timestep();
        neuroidNetwork.timestep();
        neuroidNetwork.timestep();
        neuroidNetwork.timestep();
        neuroidNetwork.timestep();

        // index 1 because its anticlockwise
        Assert.assertTrue(neuroidNetwork.getActiviationOfNeurons()[1]);

        neuroidNetwork.timestep();

        Assert.assertTrue(neuroidNetwork.getActiviationOfNeurons()[0]);
        Assert.assertFalse(neuroidNetwork.getActiviationOfNeurons()[1]);
    }
}
