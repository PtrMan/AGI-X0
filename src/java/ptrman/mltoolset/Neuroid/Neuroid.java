// translated from C#

package mltoolset.Neuroid;

import mltoolset.Datastructures.NotDirectedGraph;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * 
 * Idea of the neural network is from the book "Circuits of the mind"
 * 
 */
public class Neuroid<Weighttype, ModeType> {
    public static class FloatWeighttypeHelper implements Neuroid.IWeighttypeHelper {
        @Override
        public Object getValueForZero() {
            return 0.0f;
        }

        @Override
        public boolean greater(Object left, Object right) {
            final float leftAsFloat = (Float)left;
            final float rightAsFloat = (Float)right;

            return leftAsFloat > rightAsFloat;
        }

        @Override
        public boolean greaterEqual(Object left, Object right) {
            final float leftAsFloat = (Float)left;
            final float rightAsFloat = (Float)right;

            return leftAsFloat >= rightAsFloat;
        }

        @Override
        public Object add(Object left, Object right) {
            final float leftAsFloat = (Float)left;
            final float rightAsFloat = (Float)right;

            return new Float(leftAsFloat + rightAsFloat);
        }
    }

    public interface IWeighttypeHelper<Weighttype> {
        public Weighttype getValueForZero();
        public boolean greater(Weighttype left, Weighttype right);
        public boolean greaterEqual(Weighttype left, Weighttype right);

        public Weighttype add(Weighttype left, Weighttype right);
    }

    public static class NeuroidGraph<Weighttype> {
        public static class WeightTuple<Weighttype> {
            public WeightTuple(final int from, final int to, final Weighttype weight) {
                this.from = from;
                this.to = to;
                this.weight = weight;
            }

            public final int from;
            public final int to;
            public Weighttype weight;
        }

        public NotDirectedGraph<NeuroidGraphElement> graph = new NotDirectedGraph<>();

        public List<WeightTuple> weights = new ArrayList<>();

        public Weighttype getEdgeWeight(final int from, int to) {
            //return (Weighttype)graph.elements.get(from).content.weights.get(to);

            // TODO< use hashtable >
            for( final WeightTuple iterationWeight : weights ) {
                if( iterationWeight.from == from && iterationWeight.to == to ) {
                    return (Weighttype)iterationWeight.weight;
                }
            }

            throw new RuntimeException("Weight not set!");
        }
    }

    public static class NeuroidGraphElement<Weighttype, ModeType> {
        public boolean isInputNeuroid = false;
        /** \brief indicates if this neuroid is an input neuroid, means that it gets its activation from outside and has no parent connections or theshold and so on */
        public Weighttype threshold;
        public List<ModeType> mode = new ArrayList<ModeType>();
        public int state = 0;
        // startstate
        public boolean firing = false;
        public boolean nextFiring = false;
        /** \brief indicates if the neuron should fore on the next timestep, is updated by the update function */
        //public List<Weighttype> weights = new ArrayList<>(); /* \brief weight for each children in the graph */
        public int remainingLatency = 0;
        /** \brief as long as this is > 0 its mode nor its weights can be changed */
        public Weighttype sumOfIncommingWeights;
        
        public boolean isStimulated(IWeighttypeHelper<Weighttype> weighttypeHelper) {
            return weighttypeHelper.greaterEqual(sumOfIncommingWeights, threshold);
        }

        public void updateFiring() {
            firing = nextFiring;
            nextFiring = false;
        }
    
    }

    public static class State {
        public State() {
        }

        public int latency;
    }

    public interface IUpdate<Weighttype, ModeType> {
        void calculateUpdateFunction(int neuronIndex, NeuroidGraphElement neuroid, List<ModeType> updatedMode, List<Weighttype> updatedWeights, IWeighttypeHelper<Weighttype> weighttypeHelper);

        void initialize(NeuroidGraphElement neuroid, List<Integer> parentIndices, List<ModeType> updatedMode, List<Weighttype> updatedWeights);
    
    }
    
    public Neuroid(IWeighttypeHelper<Weighttype> weighttypeHelper)
    {
        this.weighttypeHelper = weighttypeHelper;
    }
    
    public void initialize() {
        for( int neuronI = 0;neuronI < neuroidsGraph.graph.elements.size(); neuronI++ ) {
            List<ModeType> modes = new ArrayList<ModeType>();
            List<Weighttype> weights = new ArrayList<Weighttype>();
            boolean thresholdValid;
            // a input neuroid doesn't have to be initialized
            if( neuroidsGraph.graph.elements.get(neuronI).content.isInputNeuroid ) {
                continue;
            }

            modes = new ArrayList<ModeType>();
            weights = new ArrayList<Weighttype>();
            update.initialize(neuroidsGraph.graph.elements.get(neuronI).content, neuroidsGraph.graph.elements.get(neuronI).parentIndices, modes, weights);
            neuroidsGraph.graph.elements.get(neuronI).content.mode = modes;
            //neuroidsGraph.elements[neuronI].content.weights = weights;
            thresholdValid = weighttypeHelper.greater((Weighttype) neuroidsGraph.graph.elements.get(neuronI).content.threshold, weighttypeHelper.getValueForZero());
            mltoolset.misc.Assert.Assert(thresholdValid, "threshold must be greater than 0.0!");
        }
    }

    // just for debugging
    public void debugAllNeurons() {
        int neuronI;
        System.out.format("===");
        for (neuronI = 0;neuronI < neuroidsGraph.graph.elements.size();neuronI++) {
            NeuroidGraphElement neuroidGraphElement = neuroidsGraph.graph.elements.get(neuronI).content;
            System.out.println("neuronI " + Integer.toString(neuronI) + " isFiring " + Boolean.toString(neuroidGraphElement.firing));
        }
    }

    public void timestep() {
        // order is important, we first update input and then all neuroids
        updateFiringForInputNeuroids();
        updateFiringForAllNeuroids();
        updateIncommingWeigthsForAllNeuroids();
        updateNeuronStates();
        decreaseLatency();
    }

    public void addConnections(List<NeuroidGraph.WeightTuple<Weighttype>> connections) {
        neuroidsGraph.weights.addAll(connections);

        for( final NeuroidGraph.WeightTuple<Weighttype> iterationConnection : connections ) {
            // needed for acceleration of the update
            neuroidsGraph.graph.elements.get(iterationConnection.to).parentIndices.add(iterationConnection.from);
        }
    }

    public void addTwoWayConnection(int a, int b, Weighttype weight) {
        addConnections(Arrays.asList(new NeuroidGraph.WeightTuple<Weighttype>(a, b, weight)));
        addConnections(Arrays.asList(new NeuroidGraph.WeightTuple<Weighttype>(b, a, weight)));
    }

    public boolean[] getActiviationOfNeurons() {
        boolean[] activationResult = new boolean[neuroidsGraph.graph.elements.size()];
        
        for( int neuronI = 0; neuronI < neuroidsGraph.graph.elements.size(); neuronI++ ) {
            activationResult[neuronI] = neuroidsGraph.graph.elements.get(neuronI).content.firing;
        }
        return activationResult;
    }

    /** \brief reallocates the neurons
     *
     * the neuronCount includes the count of the input neurons
     *
     */
    public void allocateNeurons(int neuronCount, int inputCount) {
        allocatedInputNeuroids = inputCount;
        neuroidsGraph.graph.elements.clear();
        for( int neuronI = 0;neuronI < neuronCount; neuronI++ ) {
            getGraph().elements.add(new NotDirectedGraph.Element(new NeuroidGraphElement()));
        }
        for( int neuronI = 0;neuronI < inputCount; neuronI++ ) {
            getGraph().elements.get(neuronI).content.isInputNeuroid = true;
        }
    }

    public NotDirectedGraph<NeuroidGraphElement> getGraph() {
        return neuroidsGraph.graph;
    }

    private void updateNeuronStates() {
        int neuronIndex = 0;

        for( NotDirectedGraph.Element<NeuroidGraphElement> iterationNeuron : neuroidsGraph.graph.elements ) {
            List<ModeType> updatedMode = new ArrayList<ModeType>();
            List<Weighttype> updatedWeights = new ArrayList<Weighttype>();
            // input neuron doesn't have to be updated
            if (iterationNeuron.content.isInputNeuroid) {
                continue;
            }
             
            // neurons with latency doesn't have to be updated
            if (iterationNeuron.content.remainingLatency > 0) {
                continue;
            }
             
            updatedMode = null;
            updatedWeights = null;
            update.calculateUpdateFunction(neuronIndex, iterationNeuron.content, updatedMode, updatedWeights, weighttypeHelper);
            iterationNeuron.content.mode = updatedMode;

            neuronIndex++;
        }
    }

    //iterationNeuron.content.weights = updatedWeights;
    private void updateIncommingWeigthsForAllNeuroids() {
        // add up the weights of the incomming edges
        for( int iterationNeuronI = 0; iterationNeuronI < neuroidsGraph.graph.elements.size(); iterationNeuronI++ ) {
            NotDirectedGraph.Element<NeuroidGraphElement> iterationNeuron = neuroidsGraph.graph.elements.get(iterationNeuronI);
            
            // activation of a input neuron doesn't have to be calculated
            if (iterationNeuron.content.isInputNeuroid) {
                continue;
            }

            Weighttype sumOfWeightsOfThisNeuron = weighttypeHelper.getValueForZero();
            for( int iterationParentIndex : iterationNeuron.parentIndices ) {
                final boolean activation = neuroidsGraph.graph.elements.get(iterationParentIndex).content.firing;
                if (activation) {
                    Weighttype edgeWeight = neuroidsGraph.getEdgeWeight(iterationParentIndex, iterationNeuronI);
                    sumOfWeightsOfThisNeuron = weighttypeHelper.add(sumOfWeightsOfThisNeuron, edgeWeight);
                }
                 
            }

            iterationNeuron.content.sumOfIncommingWeights = sumOfWeightsOfThisNeuron;
        }
    }

    private void updateFiringForAllNeuroids() {
        for( NotDirectedGraph.Element<NeuroidGraphElement> iterationNeuron : neuroidsGraph.graph.elements ) {
            iterationNeuron.content.updateFiring();
        }
    }

    private void updateFiringForInputNeuroids() {
        mltoolset.misc.Assert.Assert(allocatedInputNeuroids == input.length, "");
        
        for( int inputNeuroidI = 0;inputNeuroidI < input.length; inputNeuroidI++ ) {
            neuroidsGraph.graph.elements.get(inputNeuroidI).content.nextFiring = input[inputNeuroidI];
        }
    }

    private void decreaseLatency() {
        for( NotDirectedGraph.Element<NeuroidGraphElement> iterationNeuron : neuroidsGraph.graph.elements ) {
            if( iterationNeuron.content.remainingLatency > 0 ) {
                iterationNeuron.content.remainingLatency--;
            }
        }
    }

    // input from outside
    // must be set and resized from outside
    public boolean[] input;
    public IUpdate update;
    public State[] stateInformations;
    private int allocatedInputNeuroids;
    
    private IWeighttypeHelper<Weighttype> weighttypeHelper;

    private NeuroidGraph<Weighttype> neuroidsGraph = new NeuroidGraph();
}
