package ptrman.agix0.src.java.Common;

import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.List;
import java.util.Random;

/**
 * A component of the "brain"
 * can be a neural network or a more symbolic mechanism.
 *
 */
public class Component {


    private static class Update implements Neuroid.IUpdate<Float, Integer> {
        private final float randomFiringPropability;
        private final NeuroidNetworkDescriptor networkDescriptor;

        public Update(NeuroidNetworkDescriptor networkDescriptor, final float randomFiringPropability) {
            this.randomFiringPropability = randomFiringPropability;
            this.networkDescriptor = networkDescriptor;
        }

        @Override
        public void calculateUpdateFunction(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> neuroid, Neuroid.IWeighttypeHelper<Float> weighttypeHelper) {
            neuroid.graphElement.nextFiring = neuroid.graphElement.isStimulated(weighttypeHelper);

            if (neuroid.graphElement.nextFiring) {
                neuroid.graphElement.remainingLatency = networkDescriptor.hiddenNeurons[neuroid.index].firingLatency;
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

    public Neuroid<Float, Integer> neuroidNetwork;

    private NeuroidNetworkDescriptor networkDescriptor;

    public NeuroidNetworkDescriptor getNeuralNetworkDescriptor() {
        return networkDescriptor;
    }

    public void setupNeuroidNetwork(NeuroidNetworkDescriptor networkDescriptor) {
        // we store the NeuroidNetworkDescriptor for Spinglass
        this.networkDescriptor = networkDescriptor;

        neuroidNetwork = new Neuroid<>(new Neuroid.FloatWeighttypeHelper());
        neuroidNetwork.update = new Update(this.networkDescriptor, networkDescriptor.randomFiringPropability);

        neuroidNetwork.allocateNeurons(networkDescriptor.getNumberOfHiddenNeurons(), networkDescriptor.numberOfInputNeurons);
        neuroidNetwork.input = new boolean[networkDescriptor.numberOfInputNeurons];

        for( int neuronI = 0; neuronI < networkDescriptor.getNumberOfHiddenNeurons(); neuronI++ ) {
            neuroidNetwork.getGraph().neuronNodes[neuronI].graphElement.threshold = networkDescriptor.hiddenNeurons[neuronI].firingThreshold;
        }

        neuroidNetwork.addEdgeWeightTuples(networkDescriptor.connections);

        neuroidNetwork.initialize();

    }

    public void setStimulus(boolean[] values) {
        neuroidNetwork.input = values;
    }

    public boolean[] getActiviationOfNeurons() {
        return neuroidNetwork.getActiviationOfNeurons();
    }

    public void timestep() {
        cellularAutomataTimestep();

        neuroidNetwork.timestep();
    }

    // simulate all cellular automata of all CA neurons
    private void cellularAutomataTimestep() {
        // TODO
    }
}
