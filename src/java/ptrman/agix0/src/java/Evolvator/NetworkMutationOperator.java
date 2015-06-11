package ptrman.agix0.src.java.Evolvator;

import org.apache.commons.math3.distribution.EnumeratedDistribution;
import org.apache.commons.math3.util.Pair;
import org.uncommons.watchmaker.framework.EvolutionaryOperator;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import static ptrman.mltoolset.Neuroid.helper.NetworkTopology.getConnectionsForChainBetweenNeurons;
import static ptrman.mltoolset.math.DistinctUtility.getTwoDisjunctNumbers;
import static ptrman.mltoolset.math.Math.getRandomElements;

public class NetworkMutationOperator implements EvolutionaryOperator<NetworkGeneticExpression> {
    final static public EnumeratedDistribution<EnumOperation> operationTypePropabilities = getOperationTypePropabilities();

    private static EnumeratedDistribution<EnumOperation> getOperationTypePropabilities() {
        List<Pair<EnumOperation, Double>> resultList = new ArrayList<>();

        resultList.add(new Pair<>(EnumOperation.REMOVE_CONNECTION, 1.0));
        resultList.add(new Pair<>(EnumOperation.ADD_CONNECTION, 1.0));
        resultList.add(new Pair<>(EnumOperation.ENABLE_NEURON, 0.8));
        resultList.add(new Pair<>(EnumOperation.DISBALE_NEURON, 0.8));
        resultList.add(new Pair<>(EnumOperation.WEAKEN_STRENGHTEN_THRESHOLD, 1.5)); // slightly bias to strength weaking/strengtening
        resultList.add(new Pair<>(EnumOperation.CREATE_CHAIN, 0.2));

        return new EnumeratedDistribution(resultList);
    }

    private enum EnumOperation {
        REMOVE_CONNECTION,
        ADD_CONNECTION,
        ENABLE_NEURON,
        DISBALE_NEURON,
        WEAKEN_STRENGHTEN_THRESHOLD,
        CREATE_CHAIN // creates a chain of multiple neurons connected with connections
    }

    @Override
    public List<NetworkGeneticExpression> apply(final List<NetworkGeneticExpression> list, Random random) {
        List<NetworkGeneticExpression> resultList = new ArrayList<>();

        for( NetworkGeneticExpression iterationGeneticExpression : list ) {
            NetworkGeneticExpression createdGeneticExpression = iterationGeneticExpression.getClone();

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
                    if( iterationConnection.sourceIndex.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceIndex, sourceType)) && iterationConnection.destinationIndex.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN)) ) {
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
        else if( operation == EnumOperation.WEAKEN_STRENGHTEN_THRESHOLD ) {
            final double DELTASCALE = 0.1;

            double delta = (random.nextDouble() * 2.0 - 1.0) * DELTASCALE;
            final int neuronIndex = random.nextInt(chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons.length);

            chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].firingThreshold += (float)delta;
        }
        else { // EnumOperation.CREATE_CHAIN
            // chain just in "hidden" neurons

            final float CONNECTION_WEIGHT = 0.5f;
            final int COUNT = 2;

            List<Integer> neuronIndices = getIndicesOfDisabledNeurons(chosenMutationGeneticExpression, COUNT, random);
            // add a random neuron in front to connect it to the chain
            neuronIndices.add(0, getRandomIndexOfEnabledNeuron(chosenMutationGeneticExpression, random));

            List<Neuroid.Helper.EdgeWeightTuple.NeuronAdress> neuronAdresses = new ArrayList<>();
            for( final int iterationNeuronIndex : neuronIndices ) {
                neuronAdresses.add(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(iterationNeuronIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN));
            }

            // enabled neurons
            for( int neuronIndex : neuronIndices ) {
                chosenMutationGeneticExpression.networkDescriptor.hiddenNeurons[neuronIndex].isEnabled = true;
            }

            List<Neuroid.Helper.EdgeWeightTuple<Float>> connections = getConnectionsForChainBetweenNeurons(neuronAdresses, CONNECTION_WEIGHT);
            chosenMutationGeneticExpression.networkDescriptor.connections.addAll(connections);
        }
    }

    private static List<Integer> getIndicesOfDisabledNeurons(final NetworkGeneticExpression networkGeneticExpression, final int count, Random random) {
        final List<Integer> indicesOfDisabledNeurons = networkGeneticExpression.getIndicesOfEnabledNeurons(false);
        return getRandomElements(indicesOfDisabledNeurons, count, random);
    }

    private static int getRandomIndexOfEnabledNeuron(NetworkGeneticExpression networkGeneticExpression, Random random) {
        final List<Integer> indicesOfEnabledNeurons = networkGeneticExpression.getIndicesOfEnabledNeurons(true);
        return getRandomElements(indicesOfEnabledNeurons, 1, random).get(0);
    }
}

