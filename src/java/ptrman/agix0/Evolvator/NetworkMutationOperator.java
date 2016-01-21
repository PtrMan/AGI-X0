package ptrman.agix0.Evolvator;

import org.apache.commons.math3.distribution.EnumeratedDistribution;
import org.apache.commons.math3.util.Pair;
import org.uncommons.watchmaker.framework.EvolutionaryOperator;
import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;
import ptrman.misc.Indexing;
import ptrman.misc.Tuple;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Random;
import java.util.function.Function;

import static ptrman.mltoolset.math.Math.power2;

public class NetworkMutationOperator implements EvolutionaryOperator<NetworkGeneticExpression> {
    final static public EnumeratedDistribution<EnumOperation> operationTypePropabilities = getOperationTypePropabilities();

    private static EnumeratedDistribution<EnumOperation> getOperationTypePropabilities() {
        List<Pair<EnumOperation, Double>> resultList = new ArrayList<>();

        resultList.add(new Pair<>(EnumOperation.TOGGLE_INTERNEURONCLUSTER_CONNECTION, 0.5));
        resultList.add(new Pair<>(EnumOperation.MUTATE_INTERNEURONCLUSTER_CONNECTION, 0.7));
        resultList.add(new Pair<>(EnumOperation.TOGGLE_NEURONCLUSTER_LOOPBACK, 0.45));
        resultList.add(new Pair<>(EnumOperation.TOGGLE_NEURONCLUSTER_CONNECTION, 1.0));

        //resultList.add(new Pair<>(EnumOperation.WEAKEN_STRENGHTEN_THRESHOLD, 1.5)); // slightly bias to strength weaking/strengtening
        //resultList.add(new Pair<>(EnumOperation.MODIFY_LATENCY, 0.1));

        return new EnumeratedDistribution(resultList);
    }

    private enum EnumOperation {
        TOGGLE_INTERNEURONCLUSTER_CONNECTION,
        MUTATE_INTERNEURONCLUSTER_CONNECTION,
        TOGGLE_NEURONCLUSTER_LOOPBACK,
        TOGGLE_NEURONCLUSTER_CONNECTION,

        // MODIFY_LATENCY
        // WEAKEN_STRENGHTEN_THRESHOLD
        // TODO< manipulate CA rules, other parameters of neurons, parameters of intercluster connections >
    }

    @Override
    public List<NetworkGeneticExpression> apply(final List<NetworkGeneticExpression> list, Random random) {
        List<NetworkGeneticExpression> resultList = new ArrayList<>();

        for( NetworkGeneticExpression iterationGeneticExpression : list ) {
            NetworkGeneticExpression createdGeneticExpression = null;
            try {
                createdGeneticExpression = (NetworkGeneticExpression)iterationGeneticExpression.clone();
            } catch (CloneNotSupportedException e) {
                throw new RuntimeException();
            }

            resultList.add(createdGeneticExpression);
        }

        //int mutationChosenIndex = random.nextInt(resultList.size());
        //NetworkGeneticExpression chosenMutationGeneticExpression = resultList.get(mutationChosenIndex);

        //mutate(random, chosenMutationGeneticExpression);

        for( NetworkGeneticExpression iterationGeneticExpression : resultList ) {
            mutate(random, iterationGeneticExpression);
        }

        return resultList;
    }

    private static void mutate(Random random, NetworkGeneticExpression chosenMutationGeneticExpression) {
        final EnumOperation operation = operationTypePropabilities.sample();

        if( operation == EnumOperation.TOGGLE_INTERNEURONCLUSTER_CONNECTION) {
            final int numberOfNeuronClusters = chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters.size();
            boolean[][] connectionMatrix = new boolean[numberOfNeuronClusters][numberOfNeuronClusters];
            int[][] indexMatrix = new int[numberOfNeuronClusters][numberOfNeuronClusters];

            for( int i = 0; i < indexMatrix.length; i++ ) {
                for( int j = 0; j < indexMatrix[i].length; j++ ) {
                    indexMatrix[i][j] = -1;
                }
            }

            // fill array
            int index = 0;
            for( final GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection iterationConnection : chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections ) {
                connectionMatrix[iterationConnection.sourceClusterIndex][iterationConnection.destinationClusterIndex] = true;
                indexMatrix[iterationConnection.sourceClusterIndex][iterationConnection.destinationClusterIndex] = index;

                index++;
            }

            // flip bit / add/remove connection
            for(;;) {
                final int flippingConnectionIndex = random.nextInt((int) power2((double) chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters.size()));

                final int sourceClusterIndex = flippingConnectionIndex % chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters.size();
                final int destinationClusterIndex = flippingConnectionIndex / chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters.size();

                // flip
                connectionMatrix[sourceClusterIndex][destinationClusterIndex] = !connectionMatrix[sourceClusterIndex][destinationClusterIndex];

                if( connectionMatrix[sourceClusterIndex][destinationClusterIndex] ) {
                    // add connection

                    final int sourceClusterNeuronIndex = random.nextInt(Integer.MAX_VALUE);
                    final int destinationClusterNeuronIndex = random.nextInt(Integer.MAX_VALUE);

                    chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.add(GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection.createWithoutConnectionNeuronDescriptors(sourceClusterIndex, sourceClusterNeuronIndex, destinationClusterIndex, destinationClusterNeuronIndex));
                }
                else {
                    // remove connection if its there

                    // we do this in here to not screw the propability distribution
                    if( indexMatrix[sourceClusterIndex][destinationClusterIndex] != -1 ) {
                        chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.remove(indexMatrix[sourceClusterIndex][destinationClusterIndex]);
                    }
                }

                return;
            }
        }
        else if( operation == EnumOperation.MUTATE_INTERNEURONCLUSTER_CONNECTION ) {
            if( chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.size() == 0 ) {
                return;
            }

            final int interclusterConnectionIndex = random.nextInt(chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.size());

            GenerativeNeuroidNetworkDescriptor.InterNeuronClusterConnection chosenInterNeuronClusterConnection = chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.interNeuronClusterConnections.get(interclusterConnectionIndex);
            chosenInterNeuronClusterConnection.destinationClusterNeuronIndex = random.nextInt(Integer.MAX_VALUE);
            chosenInterNeuronClusterConnection.sourceClusterNeuronIndex = random.nextInt(Integer.MAX_VALUE);
        }
        else if( operation == EnumOperation.TOGGLE_NEURONCLUSTER_LOOPBACK ) {
            GenerativeNeuroidNetworkDescriptor.NeuronCluster chosenNeuronCluster = getRandomElementFromList(chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters, random);
            executeLamdaForRandomIndexOfBooleanArray(chosenNeuronCluster.neuronConnectionLoopbacks, random, value -> !value);
        }
        else { // if( operation == EnumOperation.TOGGLE_NEURONCLUSTER_CONNECTION ) {
            GenerativeNeuroidNetworkDescriptor.NeuronCluster chosenNeuronCluster = getRandomElementFromList(chosenMutationGeneticExpression.generativeNeuroidNetworkDescriptor.neuronClusters, random);

            final int connectionIndex = random.nextInt(Indexing.calculateMaxIndexOfTriangle(chosenNeuronCluster.neuronConnections.size()));
            final Tuple<Integer, Integer> connectionAddress = Indexing.getIndicesOfTriangle(connectionIndex);

            // flip bit
            //  choose direction we want to flip
            final int directionIndex = random.nextInt(2);
            //  ...
            chosenNeuronCluster.neuronConnections.get(connectionAddress.right)[connectionAddress.left].connections[directionIndex] = !chosenNeuronCluster.neuronConnections.get(connectionAddress.right)[connectionAddress.left].connections[directionIndex];
        }

        /*
        if( operation == EnumOperation.ADD_CONNECTION ) {
            final int NUMBER_OF_TRIES = 10;

            boolean addedConnection = false;

            for( int tryCounter = 0; tryCounter < NUMBER_OF_TRIES; tryCounter++ ) {
                boolean connectionExists = false;

                final Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType sourceType = random.nextInt(2) == 0 ? Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT : Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN;

                final int sourceIndex, destinationIndex;

                if( sourceType == Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN ) {
                    List<Integer> neuronIndices = getTwoDisjunctNumbers(random, chosenMutationGeneticExpression.networkDescriptor.getNumberOfHiddenNeurons());

                    sourceIndex = neuronIndices.get(0);
                    destinationIndex = neuronIndices.get(1);
                }
                else {
                    // TODO< get number of input neurons >
                    sourceIndex = random.nextInt(1);
                    destinationIndex = random.nextInt(chosenMutationGeneticExpression.networkDescriptor.getNumberOfHiddenNeurons());
                }

                for( final Neuroid.Helper.EdgeWeightTuple<Float> iterationConnection : chosenMutationGeneticExpression.networkDescriptor.connections ) {
                    if( iterationConnection.sourceAdress.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceIndex, sourceType)) && iterationConnection.destinationAdress.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN)) ) {
                        connectionExists = true;
                        break;
                    }
                }

                if( connectionExists ) {
                    continue;
                }

                chosenMutationGeneticExpression.networkDescriptor.connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceIndex, sourceType), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), 0.5f));

                addedConnection = true;

                break;
            }
        }
        else if( operation == EnumOperation.REMOVE_CONNECTION ) {
            if( !chosenMutationGeneticExpression.networkDescriptor.connections.isEmpty() ) {
                final int index = random.nextInt(chosenMutationGeneticExpression.networkDescriptor.connections.size());
                chosenMutationGeneticExpression.networkDescriptor.connections.remove(index);
            }
        }
        else if( operation == EnumOperation.ENABLE_NEURON ) {
            final int index = random.nextInt(chosenMutationGeneticExpression.getEnabledNeurons());
            chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[index].isEnabled = true;
        }
        else if( operation == EnumOperation.DISBALE_NEURON ) {
            final int index = random.nextInt(chosenMutationGeneticExpression.getEnabledNeurons());
            chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[index].isEnabled = false;
        }
        */








        /* TOMODIFY for new GenerativeNeuroidNetworkDescriptor
        else if( operation == EnumOperation.WEAKEN_STRENGHTEN_THRESHOLD ) {
            final double DELTASCALE = 0.1;

            double delta = (random.nextDouble() * 2.0 - 1.0) * DELTASCALE;
            final int neuronIndex = random.nextInt(chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons.length);

            float newThreshold = chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].firingThreshold + (float)delta;
            newThreshold = max(min(newThreshold, chosenMutationGeneticExpression.networkDescriptor.neuronThresholdMax), chosenMutationGeneticExpression.networkDescriptor.neuronThresholdMin);
            chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].firingThreshold = newThreshold;
        }
        else { // EnumOperation.MODIFY_LATENCY
            final int neuronIndex = random.nextInt(chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons.length);
            int delta = 1;
            if( random.nextInt(2) == 0 ) {
                delta *= -1;
            }

            final int oldLatency = chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].firingLatency;
            final int newLatency = max(min(oldLatency + delta, chosenMutationGeneticExpression.networkDescriptor.neuronLatencyMax), chosenMutationGeneticExpression.networkDescriptor.neuronLatencyMin);
            chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].firingLatency = newLatency;
        }
        */
    }

    private static <Type> void removeAnyElementFromList(List<Type> elements, Random random) {
        final int index = random.nextInt(elements.size());
        elements.remove(index);
    }

    private static <Type> Type getRandomElementFromList(List<Type> elements, Random random) {
        final int index = random.nextInt(elements.size());
        return elements.get(index);
    }

    private static void executeLamdaForRandomIndexOfBooleanArray(boolean[] array, Random random, Function<Boolean, Boolean> function) {
        final int index = random.nextInt(array.length);
        array[index] = function.apply(array[index]);
    }

}

