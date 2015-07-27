package ptrman.agix0.Evolvator;

import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;

public class NetworkGeneticExpression implements Cloneable {
    //public NeuroidNetworkDescriptor networkDescriptor = new NeuroidNetworkDescriptor();

    public NetworkGlobalState networkGlobalState = new NetworkGlobalState();

    public GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor;

    public NetworkGeneticExpression(GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor) {
        this.generativeNeuroidNetworkDescriptor = generativeNeuroidNetworkDescriptor;
    }

    /*
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
    */

    public Object clone() throws CloneNotSupportedException {
        NetworkGeneticExpression cloned = new NetworkGeneticExpression(generativeNeuroidNetworkDescriptor.clone());
        cloned.networkGlobalState = networkGlobalState;
        return cloned;
    }
}
