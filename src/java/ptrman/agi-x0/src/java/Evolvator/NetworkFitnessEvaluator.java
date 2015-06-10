package Evolvator;

import Evolvator.Evironment.Entity;
import Evolvator.Evironment.Playground;
import org.apache.commons.math3.linear.ArrayRealVector;
import org.uncommons.watchmaker.framework.FitnessEvaluator;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.List;
import java.util.Random;

import static java.lang.Math.max;


public class NetworkFitnessEvaluator implements FitnessEvaluator<NetworkGeneticExpression> {
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

    @Override
    public double getFitness(NetworkGeneticExpression networkGeneticExpression, List<? extends NetworkGeneticExpression> list) {
        // last neurons are
        // [last] move forward


        final float CONNECTION_PENELIZE = 0.08f; // how much does a connection cost?
        final float NEURON_PENELIZE = 0.8f; // how much does a neuron cost?

        final int numberOfInputNeurons = 1;

        final int latencyAfterActivation = 2;
        final float randomFiringPropability = 0.0f;

        Playground playground = new Playground();
        playground.entities.add(new Entity());
        playground.entities.get(0).position = new ArrayRealVector(new double[]{0.0, 0.0});
        playground.entities.get(0).direction = new ArrayRealVector(new double[]{1.0, 0.0});

        float fitness = 5000.0f;

        // evaluate how many times the output neuron (neuron 0) got stimulated

        Neuroid<Float, Integer> neuroid = new Neuroid<>(new Neuroid.FloatWeighttypeHelper());
        neuroid.update = new Update(latencyAfterActivation, randomFiringPropability);

        neuroid.allocateNeurons(networkGeneticExpression.neurons.length, numberOfInputNeurons);
        neuroid.input = new boolean[numberOfInputNeurons];

        for( int neuronI = 0; neuronI < networkGeneticExpression.neurons.length; neuronI++ ) {
            neuroid.getGraph().neuronNodes[neuronI].graphElement.threshold = new Float(0.4f);
        }

        neuroid.addEdgeWeightTuples(networkGeneticExpression.connectionsWithWeights);

        neuroid.initialize();

        // simulate network
        for( int timestep = 0; timestep < 80; timestep++ ) {
            // stimulate

            if( (timestep % 5) == 0 ) {
                neuroid.input[0] = true;
            }

            neuroid.timestep();

            // read out result and rate

            final boolean[] neuronActivation = neuroid.getActiviationOfNeurons();

            final boolean moveForwardNeuralSignal = neuronActivation[networkGeneticExpression.neurons.length-1];

            if( moveForwardNeuralSignal ) {
                playground.entities.get(0).speed = 1.0f;
            }
            else {
                playground.entities.get(0).speed = 0.0f;
            }

            playground.timestep();


        }


        // reward for tranveled distance
        fitness += playground.entities.get(0).position.getNorm();

        //System.out.println(networkGeneticExpression.connectionsWithWeights.size());

        fitness -= ((float)networkGeneticExpression.connectionsWithWeights.size() * CONNECTION_PENELIZE);

        fitness -= ((float)networkGeneticExpression.getEnabledNeurons() * NEURON_PENELIZE);

        fitness = max(fitness, 0.0f);

        return fitness;
    }

    @Override
    public boolean isNatural() {
        return true;
    }
}
