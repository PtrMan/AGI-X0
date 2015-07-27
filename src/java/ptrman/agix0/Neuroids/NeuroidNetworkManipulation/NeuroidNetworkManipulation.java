package ptrman.agix0.Neuroids.NeuroidNetworkManipulation;

import com.syncleus.dann.graph.MutableDirectedAdjacencyGraph;
import com.syncleus.dann.graph.search.pathfinding.AstarPathFinder;
import com.syncleus.dann.graph.search.pathfinding.HeuristicPathCost;
import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuronDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import static ptrman.mltoolset.math.DistinctUtility.getDisjuctNumbersTo;

/**
 * Generates random directed connections between Vertices and checks if the candidates are connected
 */
public class NeuroidNetworkManipulation {
    public static class CostFunction implements HeuristicPathCost {
        @Override
        public double getHeuristicPathCost(Object o, Object n1) {
            return 0.0;
        }

        @Override
        public boolean isOptimistic() {
            return true;
        }

        @Override
        public boolean isConsistent() {
            return false;
        }
    }

    public GenerativeNeuroidNetworkDescriptor generateNewCandidate(List<Integer> family, NeuronDescriptor template, Random random) {
        GenerativeNeuroidNetworkDescriptor resultGenerativeNeuroidNetworkDescriptor = GenerativeNeuroidNetworkDescriptor.createAfterFamily(family, template);

        List<IManipulationOperation> manipulationOperations = new ArrayList<>();
        manipulationOperations.add(new CreateRandomConnectionsBetweenInputAndOutput());
        executeManipulations(manipulationOperations, resultGenerativeNeuroidNetworkDescriptor, random);

        return resultGenerativeNeuroidNetworkDescriptor;
    }

    public void executeManipulations(final List<IManipulationOperation> manipulationOperations, GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor, Random random) {
        for( IManipulationOperation iterationManipulation : manipulationOperations ) {
            iterationManipulation.manipulate(generativeNeuroidNetworkDescriptor, random);
        }
    }

    private static boolean areNeuronsConnected(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> source, Neuroid.NeuroidGraph.NeuronNode<Float, Integer> destination, MutableDirectedAdjacencyGraph<Neuroid.NeuroidGraph.NeuronNode<Float, Integer>, Neuroid.NeuroidGraph.Edge<Float, Integer>> graph) {
        AstarPathFinder<Neuroid.NeuroidGraph.NeuronNode<Float, Integer>, Neuroid.NeuroidGraph.Edge<Float, Integer>> aStar = new AstarPathFinder<>(graph, new CostFunction());
        return aStar.isConnected(source, destination);
    }

    /**
     *
     * Is an interface because we can implement new Strategies easily this way.
     * (calls could be evolved/tinkered by an AI)
     *
     */
    private interface IManipulationOperation {
        void manipulate(GenerativeNeuroidNetworkDescriptor descriptor, Random random);
    }

    /**
     *
     * connects inputs/output and random connections between at least one input/output pair
     *
     */
    private static class CreateRandomConnectionsBetweenInputAndOutput implements IManipulationOperation {
        public CreateRandomConnectionsBetweenInputAndOutput() {
        }

        @Override
        public void manipulate(GenerativeNeuroidNetworkDescriptor descriptor, Random random) {
            final List<Integer> clusterIndicesToConnect = getDisjuctNumbersTo(random, new ArrayList<>(), descriptor.neuronClusters.size(), descriptor.neuronClusters.size());

            for( int clusterIndicesToConnectIndex = 0; clusterIndicesToConnectIndex < clusterIndicesToConnect.size()-1; clusterIndicesToConnectIndex++ ) {
                int clusterIndexSource = clusterIndicesToConnect.get(clusterIndicesToConnectIndex);
                int clusterIndexDestination = clusterIndicesToConnect.get(clusterIndicesToConnectIndex+1);

                int neuronIndexSourceCluster = random.nextInt();
                int neuronIndexDestinationCluster = random.nextInt();

                descriptor.interNeuronClusterConnections.add(GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection.createWithoutConnectionNeuronDescriptors(clusterIndexSource, neuronIndexSourceCluster, clusterIndexDestination, neuronIndexDestinationCluster));
            }

            int inputNeuron = random.nextInt(descriptor.inputConnections.length);
            descriptor.inputConnections[inputNeuron] = new GenerativeNeuroidNetworkDescriptor.OutsideConnection(clusterIndicesToConnect.get(0), random.nextInt());

            int outputNeuron = random.nextInt(descriptor.outputConnections.length);
            descriptor.outputConnections[outputNeuron] = new GenerativeNeuroidNetworkDescriptor.OutsideConnection(clusterIndicesToConnect.get(clusterIndicesToConnect.size()-1), random.nextInt());
        }
    }
}
