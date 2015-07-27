package ptrman.agix0.Neuroids.Datastructures;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Describes interconnected clusters of neurons
 */
public class GenerativeNeuroidNetworkDescriptor implements Cloneable {
    public static class NeuronCluster implements Cloneable {
        public static class TwoWayConnection {
            public enum EnumDirectionIndex {
                FORWARD,
                BACKWARD
            }

            public boolean[] connections = new boolean[2];
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
        public List<TwoWayConnection[]> neuronConnections = new ArrayList<>();

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
                try {
                    resultCluster.neuronsOfCluster[neuronIndex] = neuronTemplate.clone();
                } catch (CloneNotSupportedException e) {
                    throw new RuntimeException();
                }
            }

            return resultCluster;
        }

        private static TwoWayConnection[] createArrayOfTwoWayConnections(int numberOfConnections) {
            TwoWayConnection[] resultArray = new TwoWayConnection[numberOfConnections];

            for( int connectionI = 0; connectionI < numberOfConnections; connectionI++ ) {
                resultArray[connectionI] = new TwoWayConnection();
            }

            return resultArray;
        }

        public Object clone() throws CloneNotSupportedException {
            NeuronCluster cloned = new NeuronCluster();

            cloned.neuronConnectionLoopbacks = Arrays.copyOf(neuronConnectionLoopbacks, neuronConnectionLoopbacks.length);

            for( final TwoWayConnection[] iterationTwoWayConnectionArray : neuronConnections ) {
                cloned.neuronConnections.add(Arrays.copyOf(iterationTwoWayConnectionArray, iterationTwoWayConnectionArray.length));
            }

            cloned.neuronsOfCluster = new NeuronDescriptor[neuronsOfCluster.length];
            for( int i = 0; i < neuronsOfCluster.length; i++ ) {
                cloned.neuronsOfCluster[i] = neuronsOfCluster[i].clone();
            }

            cloned.cachedFirstNeuronIndex = cachedFirstNeuronIndex;

            return cloned;
        }

        public enum EnumDirection {
            ANTICLOCKWISE,
            CLOCKWISE
        }

        public void addLoop(EnumDirection direction) {
            // add loop connections
            for( int i = 0; i < neuronConnections.size(); i++ ) {
                final int arrayLength = neuronConnections.get(i).length;
                if( direction == EnumDirection.ANTICLOCKWISE ) {
                    neuronConnections.get(i)[arrayLength - 1].connections[TwoWayConnection.EnumDirectionIndex.FORWARD.ordinal()] = true;
                }
                else {
                    neuronConnections.get(i)[arrayLength - 1].connections[TwoWayConnection.EnumDirectionIndex.BACKWARD.ordinal()] = true;
                }
            }

            // needed for the loopback connection
            if( direction == EnumDirection.ANTICLOCKWISE ) {
                neuronConnections.get(neuronConnections.size() - 1)[0].connections[TwoWayConnection.EnumDirectionIndex.BACKWARD.ordinal()] = true;
            }
            else {
                neuronConnections.get(neuronConnections.size() - 1)[0].connections[TwoWayConnection.EnumDirectionIndex.FORWARD.ordinal()] = true;
            }
        }
    }

    public static class InterNeuronClusterConnection implements Cloneable {
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

        public Object clone() throws CloneNotSupportedException {
            InterNeuronClusterConnection cloned = new InterNeuronClusterConnection();
            cloned.sourceClusterIndex = sourceClusterIndex;
            cloned.sourceClusterNeuronIndex = sourceClusterNeuronIndex;
            cloned.destinationClusterIndex = destinationClusterIndex;
            cloned.destinationClusterNeuronIndex = destinationClusterNeuronIndex;

            for( final NeuronDescriptor iterationNeuronDescriptor : connectionNeuronDescriptors ) {
                cloned.connectionNeuronDescriptors.add(iterationNeuronDescriptor.clone());
            }

            return cloned;
        }
    }

    public static class OutsideConnection implements Cloneable {
        final public int neuronClusterIndex;
        final public int neuronClusterNeuronIndex; // can be greater than # of neurons in cluster, because its calculated with modulo

        public OutsideConnection(final int neuronClusterIndex, final int neuronClusterNeuronIndex) {
            this.neuronClusterIndex = neuronClusterIndex;
            this.neuronClusterNeuronIndex = neuronClusterNeuronIndex;
        }

        public OutsideConnection clone() throws CloneNotSupportedException {
            return new OutsideConnection(neuronClusterIndex, neuronClusterNeuronIndex);
        }
    }


    public List<NeuronCluster> neuronClusters = new ArrayList<>();

    public List<InterNeuronClusterConnection> interNeuronClusterConnections = new ArrayList<>();

    public OutsideConnection[] inputConnections;
    public OutsideConnection[] outputConnections;

    public GenerativeNeuroidNetworkDescriptor clone() throws CloneNotSupportedException {
        GenerativeNeuroidNetworkDescriptor cloned = new GenerativeNeuroidNetworkDescriptor();

        for( final NeuronCluster iterationNeuronCluster : neuronClusters ) {
            cloned.neuronClusters.add((NeuronCluster)iterationNeuronCluster.clone());
        }

        for( final InterNeuronClusterConnection iterationInterNeuronClusterConnection : interNeuronClusterConnections ) {
            cloned.interNeuronClusterConnections.add((InterNeuronClusterConnection)iterationInterNeuronClusterConnection.clone());
        }

        cloned.inputConnections = new OutsideConnection[inputConnections.length];
        for( int i = 0; i < inputConnections.length; i++ ) {
            cloned.inputConnections[i] = inputConnections[i].clone();
        }

        cloned.outputConnections = new OutsideConnection[outputConnections.length];
        for( int i = 0; i < inputConnections.length; i++ ) {
            cloned.outputConnections[i] = outputConnections[i].clone();
        }

        return cloned;
    }

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
