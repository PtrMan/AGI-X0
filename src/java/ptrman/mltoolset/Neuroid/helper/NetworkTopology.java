package ptrman.mltoolset.Neuroid.helper;

import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class NetworkTopology {
    public static List<Neuroid.Helper.EdgeWeightTuple<Float>> getConnectionsForChainBetweenNeurons(final List<Neuroid.Helper.EdgeWeightTuple.NeuronAdress> neuronAdresses, final float weight) {
        List<Neuroid.Helper.EdgeWeightTuple<Float>> resultList = new ArrayList<>();

        for( int neuronIndicesIndex = 0; neuronIndicesIndex < neuronAdresses.size()-1; neuronIndicesIndex++ ) {
            final Neuroid.Helper.EdgeWeightTuple.NeuronAdress sourceAdress = neuronAdresses.get(neuronIndicesIndex);
            final Neuroid.Helper.EdgeWeightTuple.NeuronAdress destinationAdress = neuronAdresses.get(neuronIndicesIndex+1);

            resultList.add(new Neuroid.Helper.EdgeWeightTuple<>(sourceAdress, destinationAdress, weight));
        }

        return resultList;
    }
}
