package ptrman.mltoolset.Usage.NetworkEvolvator;


import org.uncommons.watchmaker.framework.EvolutionaryOperator;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import static ptrman.mltoolset.math.DistinctUtility.getTwoDisjunctNumbers;

public class NetworkMutationOperator implements EvolutionaryOperator<NetworkGeneticExpression> {
    private enum EnumOperation {
        REMOVECONNECTION,
        ADDCONNECTION,
        ENABLENEURON,
        DISBALENEURON
    }

    @Override
    public List<NetworkGeneticExpression> apply(List<NetworkGeneticExpression> list, Random random) {
        List<NetworkGeneticExpression> resultList = new ArrayList<>();

        for( NetworkGeneticExpression iterationGeneticExpression : list ) {
            NetworkGeneticExpression createdGeneticExpression = iterationGeneticExpression.getClone();

            resultList.add(createdGeneticExpression);
        }

        for( NetworkGeneticExpression iterationGeneticExpression : resultList ) {
            mutate(random, iterationGeneticExpression);
        }

        return resultList;
    }

    private static void mutate(Random random, NetworkGeneticExpression chosenMutationGeneticExpression) {
        final EnumOperation operation = getRandomOperation(random);

        if( operation == EnumOperation.ADDCONNECTION ) {
            final int NUMBER_OF_TRIES = 10;

            boolean addedConnection = false;

            for( int tryCounter = 0; tryCounter < NUMBER_OF_TRIES; tryCounter++ ) {
                boolean connectionExists = false;

                final Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType sourceType = random.nextInt(2) == 0 ? Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT : Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN;

                final int sourceIndex, destinationIndex;

                if( sourceType == Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN ) {
                    List<Integer> neuronIndices = getTwoDisjunctNumbers(random, chosenMutationGeneticExpression.neuronCandidatesActive.length);

                    sourceIndex = neuronIndices.get(0);
                    destinationIndex = neuronIndices.get(1);
                }
                else {
                    // TODO< get number of input neurons
                    sourceIndex = random.nextInt(1);
                    destinationIndex = random.nextInt(chosenMutationGeneticExpression.neuronCandidatesActive.length);
                }

                for( final Neuroid.Helper.EdgeWeightTuple<Float> iterationConnection : chosenMutationGeneticExpression.connectionsWithWeights ) {
                    if( iterationConnection.sourceIndex.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceIndex, sourceType)) && iterationConnection.destinationIndex.equals(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN)) ) {
                        connectionExists = true;
                        break;
                    }
                }

                if( connectionExists ) {
                    continue;
                }

                chosenMutationGeneticExpression.connectionsWithWeights.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(sourceIndex, sourceType), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(destinationIndex, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), 0.5f));

                addedConnection = true;

                break;
            }
        }
        else if( operation == EnumOperation.REMOVECONNECTION ) {
            if( !chosenMutationGeneticExpression.connectionsWithWeights.isEmpty() ) {
                final int index = random.nextInt(chosenMutationGeneticExpression.connectionsWithWeights.size());
                chosenMutationGeneticExpression.connectionsWithWeights.remove(index);
            }
        }
        else if( operation == EnumOperation.ENABLENEURON ) {
            final int index = random.nextInt(chosenMutationGeneticExpression.neuronCandidatesActive.length);
            chosenMutationGeneticExpression.neuronCandidatesActive[index] = true;
        }
        else { // EnumOperation.DISABLENEURON
            final int index = random.nextInt(chosenMutationGeneticExpression.neuronCandidatesActive.length);
            chosenMutationGeneticExpression.neuronCandidatesActive[index] = false;
        }
    }

    private static EnumOperation getRandomOperation(Random random) {
        final int randomValue = random.nextInt(4);

        if( randomValue == 0 ) {
            return EnumOperation.REMOVECONNECTION;
        }
        else if( randomValue == 1 ) {
            return EnumOperation.ADDCONNECTION;
        }
        else if( randomValue == 2 ) {
            return EnumOperation.ENABLENEURON;
        }
        else {
            return EnumOperation.DISBALENEURON;
        }
    }

}
