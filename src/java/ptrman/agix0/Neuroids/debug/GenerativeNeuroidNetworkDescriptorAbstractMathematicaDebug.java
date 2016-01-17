package ptrman.agix0.Neuroids.debug;

import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;

/**
 * Generates mathematica code for visualizing the abstract wiring of the generativeNeuroidNetworkDescriptor
 */
public class GenerativeNeuroidNetworkDescriptorAbstractMathematicaDebug {
    public static String generateMathematicaCodeFor(GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor) {
        StringBuilder builder = new StringBuilder();

        builder.append("Graph[");

        builder.append("{");

        // TODO< add a label to edge to say from which neuroid to which neuroid it goes >
        int interConnectionCounter = 0;
        for(GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection iterationInterClusterConnection : generativeNeuroidNetworkDescriptor.interNeuronClusterConnections) {
            builder.append(String.format("%d->%d", iterationInterClusterConnection.sourceClusterIndex+1, iterationInterClusterConnection.destinationClusterIndex+1));

            if( interConnectionCounter != generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.size()-1 ) {
                builder.append(",");
            }

            interConnectionCounter++;
        }

        builder.append("}");

        builder.append(",");

        builder.append("VertexLabels -> {");

        int neuronClusterCounter = 0;
        for( GenerativeNeuroidNetworkDescriptor.NeuronCluster iterationCluster : generativeNeuroidNetworkDescriptor.neuronClusters ) {
            int numberOfNeuronsInCluster = iterationCluster.neuronsOfCluster.length;

            builder.append(String.format("%d -> \"Index = %d, NeuroidCount = %d\"", neuronClusterCounter+1, neuronClusterCounter, numberOfNeuronsInCluster));

            if( neuronClusterCounter != generativeNeuroidNetworkDescriptor.neuronClusters.size()-1 ) {
                builder.append(",");
            }

            neuronClusterCounter++;
        }

        builder.append("}");

        builder.append("]");

        return builder.toString();
    }
}
