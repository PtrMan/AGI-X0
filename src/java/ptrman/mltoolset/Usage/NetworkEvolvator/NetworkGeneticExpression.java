package ptrman.mltoolset.Usage.NetworkEvolvator;


import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class NetworkGeneticExpression {
    public boolean[] neuronCandidatesActive; // contains flags of enabled neurons

    public List<Neuroid.Helper.EdgeWeightTuple<Float>> connectionsWithWeights = new ArrayList<>();

    public NetworkGeneticExpression(int numberOfNeurons) {
        neuronCandidatesActive = new boolean[numberOfNeurons];
    }

    public NetworkGeneticExpression getClone() {
        NetworkGeneticExpression cloned;

        cloned = new NetworkGeneticExpression(neuronCandidatesActive.length);
        cloned.neuronCandidatesActive = neuronCandidatesActive;

        for( final Neuroid.Helper.EdgeWeightTuple<Float> iterationConnection : connectionsWithWeights ) {
            cloned.connectionsWithWeights.add(iterationConnection);
        }

        return cloned;
    }
}
