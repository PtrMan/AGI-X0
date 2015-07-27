package ptrman.agix0.Evolvator;

import org.uncommons.watchmaker.framework.CandidateFactory;
import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Random;

public class NetworkCandidateFactory implements CandidateFactory<NetworkGeneticExpression> {
    NetworkGlobalState networkGlobalState;

    public NetworkCandidateFactory(NetworkGlobalState networkGlobalState) {
        this.networkGlobalState = networkGlobalState;
    }

    public List<NetworkGeneticExpression> generateInitialPopulation(int count, Random random) {
        List<NetworkGeneticExpression> result = new ArrayList<>();

        for( int i = 0; i < count; i++ ) {
            result.add(generateRandomCandidate(random));
        }

        return result;
    }

    public List<NetworkGeneticExpression> generateInitialPopulation(int count, Collection<NetworkGeneticExpression> collection, Random random) {
        return generateInitialPopulation(count, random);
    }

    public NetworkGeneticExpression generateRandomCandidate(Random random) {
        List<Integer> neuronFamily = networkGlobalState.generativeNeuronNetworkSettings.getRandomNeuronFamily(random);
        GenerativeNeuroidNetworkDescriptor createdGenerativeNeuroidNetworkDescriptor = GenerativeNeuroidNetworkDescriptor.createAfterFamily(neuronFamily, NetworkGlobalState.templateNeuronDescriptor);

        // fill the generative descriptor

        for( GenerativeNeuroidNetworkDescriptor.NeuronCluster iterationNeuronCluster : createdGenerativeNeuroidNetworkDescriptor.neuronClusters ) {
            setNeuronLoopbacksToRandom(iterationNeuronCluster, random);

            iterationNeuronCluster.addLoop(GenerativeNeuroidNetworkDescriptor.NeuronCluster.EnumDirection.ANTICLOCKWISE);
        }

        // TODO< add connections between clusters (at random) >


        NetworkGeneticExpression result = new NetworkGeneticExpression(createdGenerativeNeuroidNetworkDescriptor);
        result.generativeNeuroidNetworkDescriptor = createdGenerativeNeuroidNetworkDescriptor;
        result.networkGlobalState = networkGlobalState;

        return result;
    }

    private static void setNeuronLoopbacksToRandom(GenerativeNeuroidNetworkDescriptor.NeuronCluster neuronCluster, Random random) {
        final float PROPABILITY = 0.5f;
        neuronCluster.neuronConnectionLoopbacks = booleanGetRandomVector(neuronCluster.neuronConnectionLoopbacks.length, PROPABILITY, random);
    }

    // TODO< move to misc >
    private static boolean[] booleanGetRandomVector(int length, float propability, Random random) {
        boolean[] result = new boolean[length];

        for( int i = 0; i < length; i++ ) {
            result[i] = random.nextFloat() < propability;
        }

        return result;
    }
}
