package ptrman.mltoolset.Neuroid.helper;

import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class NetworkTopology {
    public static List<Neuroid.NeuroidGraph.WeightTuple<Float>> getConnectionsForChainBetweenNeurons(final List<Integer> neuronIndices, final float weight) {
        List<Neuroid.NeuroidGraph.WeightTuple<Float>> resultList = new ArrayList<>();

        for( int neuronIndicesIndex = 0; neuronIndicesIndex < neuronIndices.size()-1; neuronIndicesIndex++ ) {
            final int fromIndex = neuronIndices.get(neuronIndicesIndex);
            final int toIndex = neuronIndices.get(neuronIndicesIndex+1);

            resultList.add(new Neuroid.NeuroidGraph.WeightTuple<>(fromIndex, toIndex, weight));
        }

        return resultList;
    }
}
