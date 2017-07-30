using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MetaNix.schmidhuber.slimRnn;

namespace MetaNixExperimentalPrivate {
    public static class TestSlimRnn {

        class TestWorld : ISlimRnnWorld {
            public void executeEnvironmentChangingActions(bool[] outputNeuronActivations, ref double time) {
                time += 1.0f; // increment time by constant value
            }

            public float[] retriveInputVector() {
                return new float[] { 0.2f, 0.5f, 0.7f };
            }
        }

        // tests if the network successfully calculates a complex formula
        public static void testCalculation() {
            SlimRnn rnn = new SlimRnn();

            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [0] input
            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [1] input
            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [2] input
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE)); // [3] 
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.MULTIPLICATIVE)); // [4] output
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE)); // [5] termination

            rnn.outputNeuronsStartIndex = 4;
            rnn.numberOfOutputNeurons = 1;

            rnn.terminatingNeuronIndex = 5;
            rnn.terminatingNeuronThreshold = 0.5f;

            rnn.numberOfInputNeurons = 3;

            rnn.t_lim = 5; // are actually less steps but fine for testing

            rnn.world = new TestWorld();

            rnn.neurons[0].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[0], rnn.neurons[3], 0.5f, 0));
            rnn.neurons[1].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[1], rnn.neurons[3], 0.81f, 0));
            rnn.neurons[2].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[2], rnn.neurons[4], 1.5f, 0));
            rnn.neurons[3].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[3], rnn.neurons[4], 0.5f, 0));
            rnn.neurons[4].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[4], rnn.neurons[5], 5000.0f, 0)); // terminate if output is set
            
            rnn.initializeNeurons();

            IList<SlimRnnNeuronWithWeight> trace;
            double time;
            bool wasTerminated;

            rnn.spread(out trace, out time, out wasTerminated);
            Debug.Assert(wasTerminated); // must terminate
        }

        // task tester which let all terminating solutions pass
        class AlwaysSuccessfulTester : ITaskSolvedAndVerifiedTester {
            public bool doesSlimRnnSolveTask(SlimRnn slimRnn, bool mustHalt, double tLim) {

                IList<SlimRnnNeuronWithWeight> trace;
                slimRnn.t_lim = tLim;
                double time;
                bool wasTerminated;
                slimRnn.spread(out trace, out time, out wasTerminated);

                return true;
            }

            public bool doesSlimRnnWithTraceSolveTask(SlimRnn slimRnn, IList<SlimRnnNeuronWithWeight> trace) {
                return true;
            }
        }

        // used for interactivly checking if the learning algorithm works correctly
        public static void interactiveCheckLearningAlgorithm() {
            SlimRnn rnn = new SlimRnn();

            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [0] input
            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [1] input
            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron()); // [2] input
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE)); // [3] termination/output
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE)); // [4] output

            rnn.outputNeuronsStartIndex = 3;
            rnn.numberOfOutputNeurons = 2;

            rnn.terminatingNeuronIndex = 3;
            rnn.terminatingNeuronThreshold = 0.5f;

            rnn.numberOfInputNeurons = 3;

            rnn.t_lim = 5; // are actually less steps but fine for testing

            rnn.world = new TestWorld();

            // insert unused neurons as possible neurons for learning
            rnn.neurons[0].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[0], rnn.neurons[3], 0.0f, 0, true));
            rnn.neurons[0].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[0], rnn.neurons[4], 0.0f, 1, true));
            rnn.neurons[1].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[1], rnn.neurons[3], 1.0f, 0));
            rnn.neurons[1].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[1], rnn.neurons[4], 0.0f, 1, true));
            rnn.neurons[2].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[2], rnn.neurons[3], 1.0f, 0));

            rnn.initializeNeurons();

            UniversalSlimRnnSearch learningAlgorithm = new UniversalSlimRnnSearch(rnn, new AlwaysSuccessfulTester());
            learningAlgorithm.weightWithPropabilityTable = new List<UniversalSlimRnnSearch.WeightWithPropability> {
                /*new UniversalSlimRnnSearch.WeightWithPropability(-50.0f, 0.01),
                new UniversalSlimRnnSearch.WeightWithPropability(-40.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-30.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-20.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-10.0f, 0.02),

                new UniversalSlimRnnSearch.WeightWithPropability(-9.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-8.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-7.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-6.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-5.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-4.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-3.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-2.0f, 0.02),
                new UniversalSlimRnnSearch.WeightWithPropability(-1.5f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-1.0f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.99f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.98f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.95f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.92f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.9f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.85f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.8f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.75f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.7f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.65f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.6f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.55f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.5f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.45f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.4f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.35f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.3f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.25f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.2f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.15f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.1f, 0.03),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.05f, 0.01),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.02f, 0.01),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.01f, 0.01),

                // TODO< positive side >
                */


                // just for testing
                new UniversalSlimRnnSearch.WeightWithPropability(1.0f, 0.4),
                new UniversalSlimRnnSearch.WeightWithPropability(-1.0f, 0.4),
                new UniversalSlimRnnSearch.WeightWithPropability(0.5f, 0.1),
                new UniversalSlimRnnSearch.WeightWithPropability(-0.5f, 0.1),
            };

            // invoke learning algorithm
            bool wasSolved;
            SlimRnn solutionRnn;
            learningAlgorithm.search(1, true, out wasSolved, out solutionRnn);
        }
    }
}
