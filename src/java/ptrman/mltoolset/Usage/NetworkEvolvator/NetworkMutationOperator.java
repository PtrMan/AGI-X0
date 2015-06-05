package mltoolset.Usage.NetworkEvolvator;

import mltoolset.Neuroid.Neuroid;
import org.uncommons.watchmaker.framework.EvolutionaryOperator;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import static mltoolset.math.DistinctUtility.getTwoDisjunctNumbers;

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

        //int mutationChosenIndex = random.nextInt(resultList.size());
        //NetworkGeneticExpression chosenMutationGeneticExpression = resultList.get(mutationChosenIndex);

        //mutate(random, chosenMutationGeneticExpression);

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

                List<Integer> neuronIndices = getTwoDisjunctNumbers(random, chosenMutationGeneticExpression.neuronCandidatesActive.length);
                final int fromIndex = neuronIndices.get(0);
                final int toIndex = neuronIndices.get(1);

                for( final Neuroid.NeuroidGraph.WeightTuple<Float> iterationConnection : chosenMutationGeneticExpression.connectionsWithWeights ) {
                    if( iterationConnection.from == fromIndex && iterationConnection.to == toIndex ) {
                        connectionExists = true;
                        break;
                    }
                }

                if( connectionExists ) {
                    continue;
                }

                chosenMutationGeneticExpression.connectionsWithWeights.add(new Neuroid.NeuroidGraph.WeightTuple<>(fromIndex, toIndex, 0.5f));

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
