using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MetaNix.schmidhuber.slimRnn {
    // SLIM RNN as described by Schmidhuber in https://arxiv.org/pdf/1210.0118.pdf
    // see http://papers.nips.cc/paper/5059-compete-to-compute.pdf for explaination of Winner-takes-all, too 
    public class SlimRnn {
        // input neurons followed by normal neurons
        public IList<SlimRnnNeuron> neurons = new List<SlimRnnNeuron>();

        public ISlimRnnLearningAlgorithm learningAlgorithm = null; // null is fine if it should be ignored

        public ISlimRnnWorld world; // set before using

        public float terminatingNeuronThreshold = float.MaxValue; // set before using
        public uint terminatingNeuronIndex = uint.MaxValue; // set before using

        public uint outputNeuronsStartIndex = uint.MaxValue; // set before using
        public uint numberOfOutputNeurons = 0; // set before using

        public float globalThreshold = 0.5f;

        public double t_lim = double.MaxValue; // used to limit runtime, write a sensible value to it if required

        public uint numberOfInputNeurons = uint.MaxValue; // set before using

        // must be called after changing the order of the neurons
        public void initializeNeurons() {
            for( int i = 0; i < neurons.Count; i++ ) {
                neurons[i].neuronIndex = (uint)i;
            }
        }

        // implementation of spread() algorithm
        // see SLIM paper from schmidhuber
        public void spread(out IList<SlimRnnNeuronWithWeight> trace, out double time, out bool wasTerminated) {
            spreadPreparations();

            time = 0.0f;

            trace = new List<SlimRnnNeuronWithWeight>(); // trace tracks connections used at least once during the current episode

            // old and new sets as described in schmidhubers algorithm
            IList<SlimRnnNeuron>
                old = new List<SlimRnnNeuron>(),
                @new = new List<SlimRnnNeuron>();

            for(;;) {
                wasTerminated = neurons[(int)terminatingNeuronIndex].activation > terminatingNeuronThreshold;
                if( wasTerminated ) {
                    break;
                }

                // get new input vector
                float[] inputVector = retriveInputVector();
                Debug.Assert(inputVector.Length == numberOfInputNeurons);

                // transfer input to input neurons
                for( int i = 0; i < numberOfInputNeurons; i++ ) {
                    neurons[i].activation = inputVector[i];
                    if( inputVector[i] != 0.0f ) {
                        old.Add(neurons[i]);
                    }
                }

                bool gotoHalt;
                propagate(old, @new, trace, ref time, out gotoHalt);
                if( gotoHalt ) {
                    goto haltLoop;
                }

                // winner-takes-all logic
                {
                    IDictionary<uint, Tuple<uint, float>> winnerNeuronIndexAndActivationByWtaGroup = new Dictionary<uint, Tuple<uint, float>>();

                    // determine winner-takes-all winners
                    foreach (SlimRnnNeuron iNew in @new) {
                        if (!iNew.winnerTakesAllGroup.HasValue) { // if it is not in a WTA group then it's not considered
                            continue;
                        }

                        uint neuronIndex = iNew.neuronIndex;

                        if (winnerNeuronIndexAndActivationByWtaGroup.ContainsKey(iNew.winnerTakesAllGroup.Value)) {
                            Tuple<uint, float> winnerNeuronIndexAndActivation = winnerNeuronIndexAndActivationByWtaGroup[iNew.winnerTakesAllGroup.Value];
                            if (iNew.next > globalThreshold && iNew.next > winnerNeuronIndexAndActivation.Item2) {
                                winnerNeuronIndexAndActivationByWtaGroup[iNew.winnerTakesAllGroup.Value] = new Tuple<uint, float>(iNew.neuronIndex, iNew.next);
                            }
                        }
                        else {
                            winnerNeuronIndexAndActivationByWtaGroup[iNew.winnerTakesAllGroup.Value] = new Tuple<uint, float>(iNew.neuronIndex, iNew.next);
                        }
                    }

                    // set all activations to zero because this simplifies the setting of the WTA winner and the setting of the result of non-WTA neurons
                    foreach (SlimRnnNeuron iNew in @new) {
                        iNew.activation = 0.0f;
                    }

                    // set all activation of winners to one
                    foreach (var iWinnerNeuronIndexAndActivation in winnerNeuronIndexAndActivationByWtaGroup.Values) {
                        neurons[(int)iWinnerNeuronIndexAndActivation.Item1].activation = 1.0f;
                    }

                    // set all non-WTA results
                    foreach (SlimRnnNeuron iNew in @new) {
                        if (iNew.winnerTakesAllGroup.HasValue) { // if it is in a WTA group then it's not considered
                            continue;
                        }

                        iNew.activation = iNew.next > globalThreshold ? 1.0f : 0.0f;
                    }
                }
                
                foreach ( SlimRnnNeuron iNew in @new ) {
                    iNew.used = false;
                    iNew.resetNext();
                }


                old = @new; // after Schmidhuber: now old can't contain any input units
                @new = new List<SlimRnnNeuron>();

                // after Schmidhuber: delete from old all u^l with zero u^l(now)
                old = new List<SlimRnnNeuron>(old.Where(v => v.activation != 0.0f));

                /* after Schmidhuber:
                 * execute environment changing actions (if any) based on new output neurons; possibly update
                 * problem specific variables needed for an ongoing performance evaluation according to a given
                 * problem specific objective function; continually add the computational cost to time
                 * 
                 * once time > t_lim exit loop
                 * 
                 */
                executeEnvironmentChangingActions(ref time);
                if (time > t_lim) {
                    goto haltLoop;
                }
            }
            haltLoop:

            foreach (SlimRnnNeuron iNew in @new) {
                iNew.used = false;
                iNew.resetNext();
            }

            // reset mark of trace
            foreach(SlimRnnNeuronWithWeight c_lk in trace) {
                c_lk.isMark = false;
            }
        }

        // propagate the signal for the "next" activation
        void propagate(
            IList<SlimRnnNeuron> old,
            IList<SlimRnnNeuron> @new,
            IList<SlimRnnNeuronWithWeight> trace,
            ref double time,
            out bool gotoHaltLoop
        ) {

            gotoHaltLoop = false;

            // TODO PARALLELIZE
            
            foreach (SlimRnnNeuron u_l in old) {
                int outNeuronWithWeightIndex = -1; // set to -1 because the incrementing is done at the head of the loop
                foreach (SlimRnnNeuronWithWeight w_lk in u_l.outNeuronsWithWeights) {
                    outNeuronWithWeightIndex++;

                    // see Procedure 2.1: Spread   in Schmidhuber paper
                    // here we invoke the learning algorithm if the connection was not jet used
                    if (
                        learningAlgorithm != null &&
                        !w_lk.wasUsed && 
                        w_lk.isEligable // call the learning algorithm only if the weight has been marked to be changable by the learning algorithm
                    ) {

                        learningAlgorithm.opportunityToAdjustWeight(w_lk);
                    }
                    w_lk.wasUsed = true;

                    if (w_lk.weight == 0.0f) {
                        continue;
                    }

                    SlimRnnNeuron k = w_lk.target;

                    // add connection to trace
                    if (!w_lk.isMark) {
                        w_lk.isMark = true;
                        trace.Add(w_lk);
                    }

                    if (k.type == SlimRnnNeuron.EnumType.ADDITIVE) {
                        k.next += (w_lk.source.activation * w_lk.weight);
                    }
                    else {
                        Debug.Assert(k.type == SlimRnnNeuron.EnumType.MULTIPLICATIVE);

                        k.next *= (w_lk.source.activation * w_lk.weight);
                    }

                    if (!k.used) {
                        k.used = true;
                        @new.Add(k);
                    }

                    time += w_lk.cost; // after schmidhuber: long wires may cost more 

                    if (time > t_lim) {
                        gotoHaltLoop = true;
                        return;
                    }
                }
            }

            if( learningAlgorithm != null && learningAlgorithm.checkForceTermination() ) {
                gotoHaltLoop = true;
                return;
            }
        }

        private float[] retriveInputVector() {
            return world.retriveInputVector();
        }

        void executeEnvironmentChangingActions(ref double time) {
            bool[] outputNeuronActivations = calculateOutputNeuronActivations();
            world.executeEnvironmentChangingActions(outputNeuronActivations, ref time);
        }

        bool[] calculateOutputNeuronActivations() {
            bool[] arr = new bool[numberOfOutputNeurons];
            for( int i = 0; i < numberOfOutputNeurons; i++ ) {
                arr[i] = neurons[(int)outputNeuronsStartIndex + i].activation != 0.0f;
            }
            return arr;
        }

        // prepare all global and nonglobal state before doing the spread mainloop
        void spreadPreparations() {
            resetUsedOfAllNeurons(); // reset all used of all neurons
            resetNextOfAllNeurons(); // reset next of all neurons accordingly to type
            resetActivationOfAllNeurons();
            resetConnectionUsageOfAllNeurons();
        }

        void resetUsedOfAllNeurons() {
            foreach( SlimRnnNeuron iNeuron in neurons ) {
                iNeuron.used = false;
            }
        }
        
        void resetNextOfAllNeurons() {
            foreach (SlimRnnNeuron iNeuron in neurons) {
                iNeuron.resetNext();
            }
        }

        void resetActivationOfAllNeurons() {
            foreach (SlimRnnNeuron iNeuron in neurons) {
                iNeuron.activation = 0.0f;
            }
        }

        void resetConnectionUsageOfAllNeurons() {
            foreach( SlimRnnNeuron iNeuron in neurons ) {
                foreach( var iConnection in iNeuron.outNeuronsWithWeights ) {
                    iConnection.wasUsed = false;
                }
            }
        }
    }

    public class SlimRnnNeuron {
        public enum EnumType {
            ADDITIVE,
            MULTIPLICATIVE,
        }

        public SlimRnnNeuron(EnumType type) {
            this.type = type;
        }

        public static SlimRnnNeuron makeInputNeuron() {
            return new SlimRnnNeuron(EnumType.ADDITIVE); // type doesn't matter because there are no inputs
        }

        public EnumType type;
        public float next; // used to calculate activation
        public float activation;
        public IList<SlimRnnNeuronWithWeight> outNeuronsWithWeights = new List<SlimRnnNeuronWithWeight>(); // neurons to which the neuron is connection, list is used to spread activation
        public bool used; // is used by SlimRnn.spread()
        public uint? winnerTakesAllGroup;
        public uint neuronIndex = uint.MaxValue; // must be valid
        
        // reset next according to type
        public void resetNext() {
            next = type == EnumType.ADDITIVE ? 0.0f : 1.0f;
        }
    }

    public class SlimRnnNeuronWithWeight {
        public SlimRnnNeuronWithWeight(SlimRnnNeuron source, SlimRnnNeuron target, float weight, uint weightIndex, bool isEligable = false) {
            this.source = source;
            this.target = target;
            this.weight = weight;
            this.weightIndex = weightIndex;
            this.isEligable = isEligable;
        }

        public float weight;
        public SlimRnnNeuron source, target;
        public float cost = 0.0f; // after Schmidhuber: long wires cost more
                                  // default of 0.0f is perfectly fine for most usecases

        public bool isMark; // used for adding the connection to the trace

        public bool wasUsed;
        public readonly uint weightIndex;

        public bool isEligable; // is this connection used for learning?
    }

    // environment interaction
    public interface ISlimRnnWorld {
        // returns the input to the SLIM RNN for the next timestep
        float[] retriveInputVector();

        void executeEnvironmentChangingActions(bool[] outputNeuronActivations, ref double time);
    }

    // learning algorithm for the SlimRnn
    public interface ISlimRnnLearningAlgorithm {
        bool checkForceTermination();

        // opportunity to adjust the weight, the learning algorithm can decide to let it as it is
        void opportunityToAdjustWeight(SlimRnnNeuronWithWeight neuronWithWeight);
    }
}
