package ptrman.agix0.src.java.Neuroids.Datastructures;

import java.util.ArrayList;
import java.util.List;

/**
 * Describes interconnected clusters of neurons
 */
public class GenerativeNeuroidNetworkDescriptor {
    public static class NeuronCluster {
        public static class TwoWayConnection {
            public boolean forwardConnectionEnabled;
            public boolean backwardConnectionEnabled;
        }

        public boolean[] neuronConnectionLoopbacks; // flags for the loopbacks of the neurons (connections connecting to themself)

        // connections between the neurons
        // each new index specifies the connections to all previous neurons
        // is a list for simpler access
        // a
        // bb
        // ccc
        // dddd
        // eeeee
        public List<TwoWayConnection[]> neuronConnections;

        public NeuronDescriptor[] neuronsOfCluster;

        // updated/used by the generator
        public int cachedFirstNeuronIndex;

        public static NeuronCluster createByNumberOfNeurons(int numberOfNeurons, NeuronDescriptor neuronTemplate) {
            NeuronCluster resultCluster = new NeuronCluster();
            resultCluster.neuronConnectionLoopbacks = new boolean[numberOfNeurons];

            for( int neuronConnectionsCounter = 1; neuronConnectionsCounter < numberOfNeurons; neuronConnectionsCounter++ ) {
                resultCluster.neuronConnections.add(createArrayOfTwoWayConnections(neuronConnectionsCounter));
            }

            resultCluster.neuronsOfCluster = new NeuronDescriptor[numberOfNeurons];
            for( int neuronIndex = 0; neuronIndex < numberOfNeurons; neuronIndex++ ) {
                resultCluster.neuronsOfCluster[neuronIndex] = neuronTemplate.getClone();
            }

            return resultCluster;
        }

        private static TwoWayConnection[] createArrayOfTwoWayConnections(int numberOfConnections) {
            TwoWayConnection[] resultArray = new TwoWayConnection[numberOfConnections];

            for( int connectionI = 0; connectionI < numberOfConnections; connectionI++ ) {
                resultArray[connectionI] = new TwoWayConnection();
            }

            return new TwoWayConnection[0];
        }
    }

    public static class InterNeuronClusterConnection {
        public int sourceClusterIndex;
        public int sourceClusterNeuronIndex; // can be greater than # of neurons of the cluster, because its calculated with modulo

        public int destinationClusterIndex;
        public int destinationClusterNeuronIndex; // can be greater than # of neurons of the cluster, because its calculated with modulo

        public List<NeuronDescriptor> connectionNeuronDescriptors = new ArrayList<>();

        private InterNeuronClusterConnection() {
        }

        public static InterNeuronClusterConnection createWithoutConnectionNeuronDescriptors(int sourceClusterIndex, int sourceClusterNeuronIndex, int destinationClusterIndex, int destinationClusterNeuronIndex) {
            InterNeuronClusterConnection result = new InterNeuronClusterConnection();
            result.sourceClusterIndex = sourceClusterIndex;
            result.sourceClusterNeuronIndex = sourceClusterNeuronIndex;
            result.destinationClusterIndex = destinationClusterIndex;
            result.destinationClusterNeuronIndex = destinationClusterNeuronIndex;
            return result;
        }
    }

    public static class OutsideConnection {
        final public int neuronClusterIndex;
        final public int neuronClusterNeuronIndex; // can be greater than # of neurons in cluster, because its calculated with modulo

        public OutsideConnection(final int neuronClusterIndex, final int neuronClusterNeuronIndex) {
            this.neuronClusterIndex = neuronClusterIndex;
            this.neuronClusterNeuronIndex = neuronClusterNeuronIndex;
        }
    }

    public List<NeuronCluster> neuronClusters;

    public List<InterNeuronClusterConnection> interNeuronClusterConnections;

    public OutsideConnection[] inputConnections;
    public OutsideConnection[] outputConnections;

    /**
     *
     * creates a new GenerativeNeuroidNetworkDescriptor after a family
     *
     * @param family the number of neurons in the cluster
     * @return
     */
    public static GenerativeNeuroidNetworkDescriptor createAfterFamily(List<Integer> family, NeuronDescriptor neuronTemplate) {
        GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor = new GenerativeNeuroidNetworkDescriptor();

        for( int currentClusterNeurons : family ) {
            generativeNeuroidNetworkDescriptor.neuronClusters.add(NeuronCluster.createByNumberOfNeurons(currentClusterNeurons, neuronTemplate));
        }

        return generativeNeuroidNetworkDescriptor;
    }
}
