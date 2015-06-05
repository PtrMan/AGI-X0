package ptrman.mltoolset.Usage.NetworkEvolvator;


import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class NetworkGeneticExpression {
    public boolean[] neuronCandidatesActive; // contains flags of enabled neurons

    public List<Neuroid.NeuroidGraph.WeightTuple<Float>> connectionsWithWeights = new ArrayList<Neuroid.NeuroidGraph.WeightTuple<Float>>();

    public NetworkGeneticExpression(int numberOfNeurons) {
        neuronCandidatesActive = new boolean[numberOfNeurons];
    }

    public NetworkGeneticExpression getClone() {
        NetworkGeneticExpression cloned;

        cloned = new NetworkGeneticExpression(neuronCandidatesActive.length);
        cloned.neuronCandidatesActive = neuronCandidatesActive;

        for( final Neuroid.NeuroidGraph.WeightTuple<Float> iterationConnection : connectionsWithWeights ) {
            cloned.connectionsWithWeights.add(iterationConnection);
        }

        return cloned;
    }
}
