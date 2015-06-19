package ptrman.agix0.src.java.Evolvator;

import ptrman.agix0.src.java.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.src.java.Neuroids.Datastructures.NeuronDescriptor;

import java.util.ArrayList;
import java.util.List;

public class NetworkGeneticExpression {
    public NeuroidNetworkDescriptor networkDescriptor = new NeuroidNetworkDescriptor();

    public NetworkGlobalSettings populationGlobalSettings = new NetworkGlobalSettings();


    public NetworkGeneticExpression(int numberOfNeurons) {
        networkDescriptor.hiddenNeurons = new NeuronDescriptor[numberOfNeurons];

        for( int neuronI = 0; neuronI < numberOfNeurons; neuronI++ ) {
            networkDescriptor.hiddenNeurons[neuronI] = new NeuronDescriptor();
        }
    }

    public NetworkGeneticExpression getClone() {
        NetworkGeneticExpression cloned = new NetworkGeneticExpression(networkDescriptor.hiddenNeurons.length);
        cloned.networkDescriptor = networkDescriptor.getClone();
        return cloned;
    }

    // TODO< cache? >
    public int getEnabledNeurons() {
        return getIndicesOfEnabledNeurons(true).size();
    }

    public List<Integer> getIndicesOfEnabledNeurons(final boolean enabled) {
        List<Integer> resultIndices = new ArrayList<>();

        for( int neuronI = 0; neuronI < networkDescriptor.hiddenNeurons.length; neuronI++ ) {
            if( networkDescriptor.hiddenNeurons[neuronI].isEnabled == enabled ) {
                resultIndices.add(neuronI);
            }
        }

        return resultIndices;
    }
}
