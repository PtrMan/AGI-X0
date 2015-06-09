package ptrman.mltoolset.Neuroid.tests;

import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.List;
import java.util.Random;

/**
 * Simple test case for standard neuroid impl.
 */
public class TestNeuroidSimple {


    private static class Update implements Neuroid.IUpdate<Float, Integer> {
        private final int latencyAfterActivation;
        private final float randomFiringPropability;

        public Update(final int latencyAfterActivation, final float randomFiringPropability) {
                this.latencyAfterActivation = latencyAfterActivation;
                this.randomFiringPropability = randomFiringPropability;
        }

        @Override
        public void calculateUpdateFunction(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> neuroid, Neuroid.IWeighttypeHelper<Float> weighttypeHelper) {
            neuroid.graphElement.nextFiring = neuroid.graphElement.isStimulated(weighttypeHelper);

            if (neuroid.graphElement.nextFiring) {
                neuroid.graphElement.remainingLatency = latencyAfterActivation;
            }
            else {
                boolean isFiring = (float)random.nextFloat() < randomFiringPropability;

                neuroid.graphElement.nextFiring = isFiring;
            }
        }

        @Override
        public void initialize(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> neuroid, List<Integer> updatedMode, List<Float> updatedWeights) {

        }

        private Random random = new Random();

    }

    public static void main(String[] args) {
        final int latencyAfterActivation = 3;
        final float randomFiringPropability =.0f;


        Neuroid<Float, Integer> neuroid = new Neuroid<>(new Neuroid.FloatWeighttypeHelper());
        neuroid.update = new Update(latencyAfterActivation, randomFiringPropability);

        neuroid.allocateNeurons(3, 3);
        neuroid.input = new boolean[3];

        neuroid.getGraph().neuronNodes[0].graphElement.threshold = 0.5f;
        neuroid.getGraph().neuronNodes[1].graphElement.threshold = 0.5f;
        neuroid.getGraph().neuronNodes[2].graphElement.threshold = 0.5f;

        neuroid.addConnection(new Neuroid.NeuroidGraph.Edge<>(neuroid.getGraph().inputNeuronNodes[2], neuroid.getGraph().neuronNodes[0], 0.9f));
        neuroid.addConnection(new Neuroid.NeuroidGraph.Edge<>(neuroid.getGraph().neuronNodes[0], neuroid.getGraph().neuronNodes[1], 0.9f));

        neuroid.initialize();

        for( int timestep = 0; timestep < 5; timestep++ ) {
            System.out.println("=A=A=A=A");

            neuroid.debugAllNeurons();

            // stimulate
            neuroid.input[2] = true;

            neuroid.timestep();

            neuroid.debugAllNeurons();

        }

        int debug = 0;
    }

}
