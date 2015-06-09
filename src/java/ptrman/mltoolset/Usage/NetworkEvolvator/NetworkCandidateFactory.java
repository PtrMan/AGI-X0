package ptrman.mltoolset.Usage.NetworkEvolvator;

import org.uncommons.watchmaker.framework.CandidateFactory;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Random;

import static ptrman.mltoolset.math.Math.getRandomIndices;

public class NetworkCandidateFactory implements CandidateFactory<NetworkGeneticExpression> {
    private final int numberOfNeurons;

    public NetworkCandidateFactory(int numberOfNeurons) {
        this.numberOfNeurons = numberOfNeurons;
    }

    public List<NetworkGeneticExpression> generateInitialPopulation(int count, Random random) {
        List<NetworkGeneticExpression> result = new ArrayList<NetworkGeneticExpression>();

        for( int i = 0; i < count; i++ ) {
            result.add(generateRandomCandidate(random));
        }

        return result;
    }

    public List<NetworkGeneticExpression> generateInitialPopulation(int count, Collection<NetworkGeneticExpression> collection, Random random) {
        return generateInitialPopulation(count, random);
    }

    public NetworkGeneticExpression generateRandomCandidate(Random random) {
        NetworkGeneticExpression result = new NetworkGeneticExpression(numberOfNeurons);

        final int numberOfActiveNeurons = 5;

        final List<Integer> neuronIndices = getRandomIndices(numberOfNeurons, numberOfActiveNeurons, random);

        // set the neurons
        for( final int neuronIndex : neuronIndices ) {
            result.neuronCandidatesActive[neuronIndex] = true;
        }

        // create random connections between neurons

        int counterOfConnections = 0;

        int numberOfConnections = 8;

        for(;;) {
            if( counterOfConnections >= numberOfConnections ) {
                break;
            }

            int sourceNeuronIndexIndex = random.nextInt(neuronIndices.size());
            int sourceNeuronIndex = neuronIndices.get(sourceNeuronIndexIndex);

            int targetNeuronIndexIndex = random.nextInt(neuronIndices.size());
            int targetNeuronIndex = neuronIndices.get(targetNeuronIndexIndex);

            if( sourceNeuronIndex == targetNeuronIndex ) {
                continue;
            }

            // for now just hidden connections because it can evolve the connections to the input later

            result.connectionsWithWeights.add(new Neuroid.Helper.EdgeWeightTuple<Float>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceNeuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress( targetNeuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), 0.5f));
            counterOfConnections++;
        }

        return result;
    }
}
