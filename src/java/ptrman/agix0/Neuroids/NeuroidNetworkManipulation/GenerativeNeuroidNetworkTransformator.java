package ptrman.agix0.Neuroids.NeuroidNetworkManipulation;

import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuronDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

import static ptrman.mltoolset.Neuroid.helper.NetworkTopology.getConnectionsForChainBetweenNeurons;

/**
 *
 */
public class GenerativeNeuroidNetworkTransformator {
    public static NeuroidNetworkDescriptor generateNetwork(final NeuroidNetworkDescriptor neuroidNetworkTemplate, GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor) {
        NeuroidNetworkDescriptor resultNeuroidNetworkDescriptor = neuroidNetworkTemplate.getClone();

        List<NeuronDescriptor> neurons = new ArrayList<>();
        List<Neuroid.Helper.EdgeWeightTuple<Float>> connections = resultNeuroidNetworkDescriptor.connections;

        for( final GenerativeNeuroidNetworkDescriptor.NeuronCluster iterationNeuronCluster : generativeNeuroidNetworkDescriptor.neuronClusters ) {
            decorateClusterLevel0(iterationNeuronCluster, neurons);
            generateAndLinkClusterLevel0(iterationNeuronCluster, neurons.size(), neurons, connections, resultNeuroidNetworkDescriptor.connectionDefaultWeight);
        }

        // connections between clusters
        clusterLevel0AddInterClusterConnections(resultNeuroidNetworkDescriptor, generativeNeuroidNetworkDescriptor, neurons, connections);

        addInputConnections(resultNeuroidNetworkDescriptor, generativeNeuroidNetworkDescriptor);
        addOutputConnections(resultNeuroidNetworkDescriptor, generativeNeuroidNetworkDescriptor);

        resultNeuroidNetworkDescriptor.hiddenNeurons = new NeuronDescriptor[neurons.size()];
        resultNeuroidNetworkDescriptor.hiddenNeurons = neurons.toArray(resultNeuroidNetworkDescriptor.hiddenNeurons);

        return resultNeuroidNetworkDescriptor;
    }

    private static void addInputConnections(NeuroidNetworkDescriptor resultNeuroidNetworkDescriptor, GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor) {
        int inputIndex = 0;

        for( final GenerativeNeuroidNetworkDescriptor.OutsideConnection iterationConnection : generativeNeuroidNetworkDescriptor.inputConnections ) {
            if( iterationConnection == null ) {
                continue;
            }

            final GenerativeNeuroidNetworkDescriptor.NeuronCluster selectedNeuronCluster = generativeNeuroidNetworkDescriptor.neuronClusters.get(iterationConnection.neuronClusterIndex);

            int clusterNeuronStartIndex = selectedNeuronCluster.cachedFirstNeuronIndex;
            int destinationNeuronIndex = clusterNeuronStartIndex + (iterationConnection.neuronClusterNeuronIndex % selectedNeuronCluster.neuronsOfCluster.length);

            resultNeuroidNetworkDescriptor.connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(inputIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationNeuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), resultNeuroidNetworkDescriptor.connectionDefaultWeight));

            inputIndex++;
        }
    }

    private static void addOutputConnections(NeuroidNetworkDescriptor resultNeuroidNetworkDescriptor, GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor) {
        int outputIndex = 0;

        for( final GenerativeNeuroidNetworkDescriptor.OutsideConnection iterationConnection : generativeNeuroidNetworkDescriptor.outputConnections ) {
            if( iterationConnection == null ) {
                continue;
            }

            final GenerativeNeuroidNetworkDescriptor.NeuronCluster selectedNeuronCluster = generativeNeuroidNetworkDescriptor.neuronClusters.get(iterationConnection.neuronClusterIndex);

            int clusterNeuronStartIndex = selectedNeuronCluster.cachedFirstNeuronIndex;
            int sourceNeuronIndex = clusterNeuronStartIndex + (iterationConnection.neuronClusterNeuronIndex % selectedNeuronCluster.neuronsOfCluster.length);

            resultNeuroidNetworkDescriptor.connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceNeuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(outputIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.OUTPUT), resultNeuroidNetworkDescriptor.connectionDefaultWeight));

            outputIndex++;
        }
    }


    private static void clusterLevel0AddInterClusterConnections(NeuroidNetworkDescriptor resultNeuroidNetworkDescriptor, GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor, List<NeuronDescriptor> neurons, List<Neuroid.Helper.EdgeWeightTuple<Float>> connections) {
        for( GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection currentInterNeuronClusterConnection : generativeNeuroidNetworkDescriptor.interNeuronClusterConnections ) {
            final GenerativeNeuroidNetworkDescriptor.NeuronCluster sourceNeuronCluster = generativeNeuroidNetworkDescriptor.neuronClusters.get(currentInterNeuronClusterConnection.sourceClusterIndex);
            final GenerativeNeuroidNetworkDescriptor.NeuronCluster destinationNeuronCluster = generativeNeuroidNetworkDescriptor.neuronClusters.get(currentInterNeuronClusterConnection.destinationClusterIndex);

            final int sourceNeuronClusterNeuronIndex = currentInterNeuronClusterConnection.sourceClusterNeuronIndex % sourceNeuronCluster.neuronsOfCluster.length;
            final int destinationNeuronClusterNeuronIndex = currentInterNeuronClusterConnection.destinationClusterNeuronIndex % destinationNeuronCluster.neuronsOfCluster.length;

            final int indexOfSourceNeuron = sourceNeuronCluster.cachedFirstNeuronIndex + sourceNeuronClusterNeuronIndex;
            final int indexOfDestinationNeuron = destinationNeuronCluster.cachedFirstNeuronIndex + destinationNeuronClusterNeuronIndex;

            // debug
            if( indexOfSourceNeuron < 0 || indexOfDestinationNeuron < 0 ) {
                int debug = 0;
            }

            assert indexOfSourceNeuron >= 0;
            assert indexOfDestinationNeuron >= 0;



            // build and link the connection
            List<Neuroid.Helper.EdgeWeightTuple.NeuronAdress> neuronAddressedOfNeuronsOfConnection = new ArrayList<>();
            neuronAddressedOfNeuronsOfConnection.add(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(indexOfSourceNeuron, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN));

            // add the neurons in between
            for( NeuronDescriptor currentInterconnectionNeuronDescriptor : currentInterNeuronClusterConnection.connectionNeuronDescriptors ) {
                neuronAddressedOfNeuronsOfConnection.add(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(neuronAddressedOfNeuronsOfConnection.size(), Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN));

                try {
                    neurons.add(currentInterconnectionNeuronDescriptor.clone());
                } catch (CloneNotSupportedException e) {
                    throw new RuntimeException();
                }
            }

            neuronAddressedOfNeuronsOfConnection.add(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(indexOfDestinationNeuron, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN));

            // link the connection
            resultNeuroidNetworkDescriptor.connections.addAll(getConnectionsForChainBetweenNeurons(neuronAddressedOfNeuronsOfConnection, resultNeuroidNetworkDescriptor.connectionDefaultWeight));
        }
    }

    private static void decorateClusterLevel0(GenerativeNeuroidNetworkDescriptor.NeuronCluster neuronCluster, List<NeuronDescriptor> neurons) {
        neuronCluster.cachedFirstNeuronIndex = neurons.size();
    }

    private static void generateAndLinkClusterLevel0(final GenerativeNeuroidNetworkDescriptor.NeuronCluster neuronCluster, final int neuronIndexOffset, List<NeuronDescriptor> neurons, List<Neuroid.Helper.EdgeWeightTuple<Float>> connections, float connectionStrength) {
        List<Integer> globalIndicesOfNeurons = new ArrayList<>();

        for( NeuronDescriptor iterationNeuron : neuronCluster.neuronsOfCluster ) {
            try {
                neurons.add(iterationNeuron.clone());
            } catch (CloneNotSupportedException e) {
                throw new RuntimeException();
            }
        }

        int currentGlobalNeuronIndex = neuronIndexOffset;

        globalIndicesOfNeurons.add(currentGlobalNeuronIndex);
        currentGlobalNeuronIndex++;

        // add connections between neurons
        for( int neuronConnectionsI = 0; neuronConnectionsI < neuronCluster.neuronConnections.size(); neuronConnectionsI++ ) {
            GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection[] currentNeuronConnections = neuronCluster.neuronConnections.get(neuronConnectionsI);

            boolean debug = false;
            if( debug ) {
                debugTwoWayConnections(currentNeuronConnections);
            }

            globalIndicesOfNeurons.add(currentGlobalNeuronIndex);
            currentGlobalNeuronIndex++;

            int lastGlobalIndexOfNeuron = globalIndicesOfNeurons.get(globalIndicesOfNeurons.size() - 1);

            for( int currentNeuronConnectionsI = 0; currentNeuronConnectionsI < currentNeuronConnections.length; currentNeuronConnectionsI++ ) {
                GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection currentConnection = currentNeuronConnections[currentNeuronConnectionsI];

                if( currentConnection.connections[GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection.EnumDirectionIndex.FORWARD.ordinal()] ) {
                    connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(lastGlobalIndexOfNeuron, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN) , new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(globalIndicesOfNeurons.get(currentNeuronConnectionsI), Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), connectionStrength));
                }

                if( currentConnection.connections[GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection.EnumDirectionIndex.BACKWARD.ordinal()] ) {
                    connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(globalIndicesOfNeurons.get(currentNeuronConnectionsI), Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(lastGlobalIndexOfNeuron, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), connectionStrength));
                }
            }
        }

        // add loopback connections
        for( int loopbackNeuronI = 0; loopbackNeuronI < neuronCluster.neuronConnectionLoopbacks.length; loopbackNeuronI++ ) {
            if( neuronCluster.neuronConnectionLoopbacks[loopbackNeuronI] ) {
                int neuronIndex = globalIndicesOfNeurons.get(loopbackNeuronI);

                connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(neuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(neuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), connectionStrength));
            }
        }
    }

    private static void debugTwoWayConnections(GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection[] currentNeuronConnections) {
        for( GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection iterationConnection : currentNeuronConnections ) {
            if( iterationConnection.connections[GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection.EnumDirectionIndex.BACKWARD.ordinal()] ) {
                System.out.print("<");
            }
            else {
                System.out.print("-");
            }

            if( iterationConnection.connections[GenerativeNeuroidNetworkDescriptor.NeuronCluster.TwoWayConnection.EnumDirectionIndex.FORWARD.ordinal()] ) {
                System.out.print(">");
            }
            else {
                System.out.print("-");
            }

            System.out.print(" ");
        }

        System.out.println();
    }
}
