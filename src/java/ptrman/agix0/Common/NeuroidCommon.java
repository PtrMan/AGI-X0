package ptrman.agix0.Common;

import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.List;
import java.util.Random;

/**
 *
 */
public class NeuroidCommon {
    public static class Update implements Neuroid.IUpdate<Float, Integer> {
        private final NeuroidNetworkDescriptor networkDescriptor;

        public Update(NeuroidNetworkDescriptor networkDescriptor) {
            this.networkDescriptor = networkDescriptor;
        }

        @Override
        public void calculateUpdateFunction(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> neuroid, Neuroid.IWeighttypeHelper<Float> weighttypeHelper) {
            neuroid.graphElement.nextFiring = neuroid.graphElement.isStimulated(weighttypeHelper);

            if (neuroid.graphElement.nextFiring) {
                neuroid.graphElement.remainingLatency = networkDescriptor.hiddenNeurons[neuroid.index].firingLatency;
            }
            else {
                boolean isFiring = random.nextFloat() < networkDescriptor.hiddenNeurons[neuroid.index].randomFiringPropability;

                neuroid.graphElement.nextFiring = isFiring;
            }
        }

        @Override
        public void initialize(Neuroid.NeuroidGraph.NeuronNode<Float, Integer> neuroid, List<Integer> updatedMode, List<Float> updatedWeights) {

        }

        private Random random = new Random();
    }

    public static Neuroid<Float, Integer> createNeuroidNetworkFromDescriptor(NeuroidNetworkDescriptor networkDescriptor) {
        Neuroid<Float, Integer> neuroidNetwork = new Neuroid<>(new Neuroid.FloatWeighttypeHelper());

        neuroidNetwork.update = new NeuroidCommon.Update(networkDescriptor);

        neuroidNetwork.allocateNeurons(networkDescriptor.getNumberOfHiddenNeurons(), networkDescriptor.numberOfInputNeurons, networkDescriptor.numberOfOutputNeurons);
        neuroidNetwork.input = new boolean[networkDescriptor.numberOfInputNeurons];

        for( int neuronI = 0; neuronI < networkDescriptor.getNumberOfHiddenNeurons(); neuronI++ ) {
            neuroidNetwork.getGraph().neuronNodes[neuronI].graphElement.threshold = networkDescriptor.hiddenNeurons[neuronI].firingThreshold;
        }

        neuroidNetwork.addEdgeWeightTuples(networkDescriptor.connections);

        neuroidNetwork.initialize();

        return neuroidNetwork;
    }
}
