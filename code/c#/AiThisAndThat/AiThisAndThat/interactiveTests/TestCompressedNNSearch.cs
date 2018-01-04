using AiThisAndThat.neural;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AiThisAndThat.interactiveTests {
    static class TestCompressedNNSearch {
        public static void test() {
            CompressedNNSearchLASearchContext context = new CompressedNNSearchLASearchContext();

            IList<Thread> threads = new List<Thread>();

            for (int threadIdx = 0; threadIdx < 7; threadIdx++) {

                // calculate the stride of the frequencies for the search
                int strideForSearch = Math.Max(1, (1<<(threadIdx + 4)) - 1);

                if (strideForSearch > 256) {
                    continue;
                }

                strideForSearch = Math.Min(256, strideForSearch);

                Thread thread = new Thread(
                    delegate() {                 
                        int logarithmicWidth = 13;
                        CompressedNNSearchLA<NeuralNetworkIdentityTanh> la =
                            new CompressedNNSearchLA<NeuralNetworkIdentityTanh>(context, logarithmicWidth);
                            
                        la.configStride = strideForSearch;

                        NeuralNetworkIdentityTanh network = new NeuralNetworkIdentityTanh();

                
                        network.layers = new Layer<NeuronPropertiesIdentityTanh>[5];
                        network.layers[0] = new Layer<NeuronPropertiesIdentityTanh>();
                        network.layers[1] = new Layer<NeuronPropertiesIdentityTanh>();
                        network.layers[2] = new Layer<NeuronPropertiesIdentityTanh>();
                        network.layers[3] = new Layer<NeuronPropertiesIdentityTanh>();
                        network.layers[4] = new Layer<NeuronPropertiesIdentityTanh>();

                        network.layers[0].neurons = new NeuronPropertiesIdentityTanh[5];
                        for (int i = 0; i < network.layers[0].neurons.Length; i++) {
                            network.layers[0].neurons[i] = new NeuronPropertiesIdentityTanh();
                        }

                        network.layers[1].neurons = new NeuronPropertiesIdentityTanh[80];
                        for (int i = 0; i < network.layers[1].neurons.Length; i++) {
                            network.layers[1].neurons[i] = new NeuronPropertiesIdentityTanh();
                        }

                        network.layers[2].neurons = new NeuronPropertiesIdentityTanh[30];
                        for (int i = 0; i < network.layers[2].neurons.Length; i++) {
                            network.layers[2].neurons[i] = new NeuronPropertiesIdentityTanh();
                        }

                        network.layers[3].neurons = new NeuronPropertiesIdentityTanh[10];
                        for (int i = 0; i < network.layers[3].neurons.Length; i++) {
                            network.layers[3].neurons[i] = new NeuronPropertiesIdentityTanh();
                        }


                        //network.layers[0].neurons = new SigmoidNeuron[]{new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(),new SigmoidNeuron(),}; // input neurons
                        //network.layers[1].neurons = new SigmoidNeuron[]{new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(),new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron()}; // hidden layer                            
                        //network.layers[2].neurons = new SigmoidNeuron[]{new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(), new SigmoidNeuron(),new SigmoidNeuron(), new SigmoidNeuron() }; // hidden layer
                        //network.layers[3].neurons = new SigmoidNeuron[]{new SigmoidNeuron(), new SigmoidNeuron(),new SigmoidNeuron(), new SigmoidNeuron()}; // output layer
                        network.layers[4].neurons = new NeuronPropertiesIdentityTanh[]{new NeuronPropertiesIdentityTanh(), new NeuronPropertiesIdentityTanh()}; // output layer


                        // allocate weights
                        foreach( var iNeuron in network.layers[0].neurons)
                            iNeuron.weights = new float[5];

                        for( int layerIdx = 1; layerIdx < network.layers.Length; layerIdx++ ) {
                            foreach( var iNeuron in network.layers[layerIdx].neurons)
                                iNeuron.weights = new float[network.layers[layerIdx-1].neurons.Length];
                        }
                
                        la.trainingSet = new List<TrainingTuple<float>>{
                            new TrainingTuple<float>(
                                new float[]{1.0f, 0.5f, 0.3f, -1.0f, 5.0f},
                                new float[]{1.0f, 0.0f}
                            ),
                            new TrainingTuple<float>(
                                new float[]{0.1f, -0.5f, 0.9f, 0.45f, 5.0f},
                                new float[]{0.0f, 1.0f}
                            ),
                            new TrainingTuple<float>(
                                new float[]{-0.1886f, 0.5889f, -0.9f, 0.126f, 5.0f},
                                new float[]{0.0f, 1.0f}
                            ),
                            new TrainingTuple<float>(
                                new float[]{0.6996f, 0.399595f, -0.72654f, 0.2f, 5.0f},
                                new float[]{1.0f, 0.0f}
                            ),


                            new TrainingTuple<float>(
                                new float[]{0.156654f, -0.3345f, -0.09f, -0.444333467f, 5.0f},
                                new float[]{0.0f, 1.0f}
                            ),
                            new TrainingTuple<float>(
                                new float[]{-0.1886f, 0.165f, -0.1f, -0.52645554f, 5.0f},
                                new float[]{0.0f, 1.0f}
                            ),
                            new TrainingTuple<float>(
                                new float[]{-0.1069496f, 0.203595f, 0.904f, 0.02f, 5.0f},
                                new float[]{1.0f, 0.0f}
                            ),
                        };

                        la.learn(network);
                });
                
                threads.Add(thread);
            }

            foreach( Thread iThread in threads ) {
                iThread.Start();
            }

            // wait till threads finished, should not happen
            foreach (Thread iThread in threads) {
                iThread.Join();
            }
        }
    }
}
