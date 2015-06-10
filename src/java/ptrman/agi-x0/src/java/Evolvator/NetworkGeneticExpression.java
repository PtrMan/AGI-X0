package Evolvator;

import ptrman.mltoolset.Neuroid.Neuroid;

import java.util.ArrayList;
import java.util.List;

public class NetworkGeneticExpression {
    public static class NeuronState {
        public boolean isEnabled;

        // input(s) -> threshold detector -> CA / state graph -> output
        public enum EnumInternalType {
            UNIFORM, // no internal state
            CA // internal CA is used as state
        }

        public EnumInternalType internalType = EnumInternalType.UNIFORM;

        public float firingThreshold = 0.5f;

        // internal CellularAutomata
        public int stateCaWriteinOffset = 0;
        public int stateCaReadoutOffset = 1;
        public boolean[] stateCaStartState;
        public int[] stateCaRuleIndices; // indices of the rules used for each bit
        public boolean stateCaWraparound = false;

        public NeuronState getClone() {
            NeuronState clone = new NeuronState();
            clone.isEnabled = isEnabled;
            clone.internalType = internalType;
            clone.firingThreshold = firingThreshold;
            clone.stateCaWriteinOffset = stateCaWriteinOffset;
            clone.stateCaReadoutOffset = stateCaReadoutOffset;
            clone.stateCaStartState = stateCaStartState;

            if( stateCaRuleIndices != null ) {
                clone.stateCaRuleIndices = new int[stateCaRuleIndices.length];
                for( int i = 0; i < stateCaRuleIndices.length; i++ ) {
                    clone.stateCaRuleIndices[i] = stateCaRuleIndices[i];
                }
            }
            clone.stateCaWraparound = stateCaWraparound;

            return clone;
        }
    }

    public NeuronState[] neurons;

    public NetworkGlobalSettings populationGlobalSettings = new NetworkGlobalSettings();

    public List<Neuroid.Helper.EdgeWeightTuple<Float>> connectionsWithWeights = new ArrayList<>();

    public NetworkGeneticExpression(int numberOfNeurons) {
        neurons = new NeuronState[numberOfNeurons];

        for( int neuronI = 0; neuronI < numberOfNeurons; neuronI++ ) {
            neurons[neuronI] = new NeuronState();
        }
    }

    public NetworkGeneticExpression getClone() {
        NetworkGeneticExpression cloned;

        cloned = new NetworkGeneticExpression(neurons.length);

        for( int neuronI = 0; neuronI < neurons.length; neuronI++ ) {
            cloned.neurons[neuronI] = neurons[neuronI].getClone();
        }

        for( final Neuroid.Helper.EdgeWeightTuple<Float> iterationConnection : connectionsWithWeights ) {
            cloned.connectionsWithWeights.add(iterationConnection);
        }

        return cloned;
    }

    // TODO< cache? >
    public int getEnabledNeurons() {
        return getIndicesOfEnabledNeurons(true).size();
    }

    public List<Integer> getIndicesOfEnabledNeurons(final boolean enabled) {
        List<Integer> resultIndices = new ArrayList<>();

        for( int neuronI = 0; neuronI < neurons.length; neuronI++ ) {
            if( neurons[neuronI].isEnabled == enabled ) {
                resultIndices.add(neuronI);
            }
        }

        return resultIndices;
    }
}
