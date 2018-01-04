using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Test0.math.fft;
using Test0.neurons;

namespace Test0.genericNeuralNetwork {
    public static class CompressedNNSearchHelper {
        
        // transforms phase and amplitude to signal with the help of inverse FFT
        public static void calcInverseFft(int numberOfBins, double[] amplitude, double[] phase,  int logarithmicWidth, double[] fftReal, double[] fftImaginary) {
            double normalization = (1.0 / numberOfBins) * 2;

            // real[0] and imaginary[0] are for the bin of the constant value wih frequency zero
            fftReal[0] = amplitude[0];
            fftImaginary[0] = 0; // we don't use this

            for (int i = 0; i < numberOfBins - 1; i++) {
                fftReal[1 + i] = Math.Cos(phase[1+i]) * amplitude[1+i] * normalization;
                fftImaginary[1 + i] = Math.Sin(phase[1+i]) * amplitude[1+i] * normalization;
            }

            fftReal[numberOfBins] = Math.Cos(phase[numberOfBins-1]) * amplitude[numberOfBins-1] * normalization * 2.0;
            fftImaginary[numberOfBins] = Math.Sin(phase[numberOfBins-1]) * amplitude[numberOfBins-1] * normalization * 2.0;

            // mirror because it represents the negative frequency components
            // imaginary is reversed because of mathematical reasons
            for (int i = 0; i < numberOfBins - 1; i++) {
                fftReal[numberOfBins * 2 - i - 1] = fftReal[i + 1];
                fftImaginary[numberOfBins * 2 - i - 1] = -fftImaginary[i + 1];
            }

            FFT.fft1d(
                FFT.EnumDirection.BACKWARD,
                logarithmicWidth,
                ref fftReal,
                ref fftImaginary);
        }
    }

    // global context whic is used by all threads to collect the best result NN configuration
    public class CompressedNNSearchLASearchContext {
        public class BestNetworkCandidate {
            public double[] fftAmplitude, fftPhase; // amplitude and phase vectors of the coefficients describing the network

            public float error;

            public BestNetworkCandidate(double[] fftAmplitude, double[] fftPhase, float error) {
                this.fftAmplitude = fftAmplitude;
                this.fftPhase = fftPhase;
                this.error = error;
            }
        }

        public BestNetworkCandidate bestNetworkCandidate = null;
        public Mutex bestNetworkCandidateMutex = new Mutex();
    }

    /**
     * /brief search algorithm(SA) for the generalized feed forward neural networks using fast fourier transformation
     * 
     * Uses a combination of levin search for determining the coefficients and FFT for calculating the detailed weights for the NN
     * as described by Schmidhuber [schmidhuberCompressedNNSearch].
     * 
     * [schmidhuberCompressedNNSearch] http://people.idsia.ch/~juergen/compressednetworksearch.html
     */
    public class CompressedNNSearchLA<NeuronType>
        : IGenericClassicalNetworkLearningAlgorithm<NeuronType, float>
        where NeuronType : AbstractFloatNeuron<NeuronType>
    {
        public CompressedNNSearchLA(CompressedNNSearchLASearchContext context, int logarithmicWidth) {
            this.context = context;
            this.logarithmicWidth = logarithmicWidth;


            fftReal = new double[1 << logarithmicWidth];
            fftImaginary = new double[1 << logarithmicWidth];
        }

        int logarithmicWidth;

        // cache for fft
        double[] fftReal, fftImaginary;

        //public float[] expectedNetworkResult0, expectedNetworkResult1, expectedNetworkResult2, expectedNetworkResult3;
        //public IList<float> input0, input1, input2, input3;
        public IList<TrainingTuple<float>> trainingSet = new List<TrainingTuple<float>>();
        
        // values which can be used for configuration
        public double
            configWeightExpRangeMin = -8.5, // 13.07.2017 was  -7.5
            configWeightExpRangeMax = 8;
        
        public int
            configStride = 1;

        private CompressedNNSearchLASearchContext context;



        public void learn(AbstractGenericClassicalNetwork<NeuronType, float> network) {
            Stopwatch searchRuntimeStopwatch = new Stopwatch();
            searchRuntimeStopwatch.Start();

            // we just do as stupid enumeration
            long enumerationCounter = 0;

            for(;;) {

                const int numberOfBits = 5;

                int[] enumerationValues = new int[5*2];
                for (int i = 0; i < enumerationValues.Length; i++) {
                    enumerationValues[i] = (int)(enumerationCounter >> (numberOfBits*i)) & ((1 << (numberOfBits+1)) - 1);
                }

                int numberOfBins = (1 << logarithmicWidth) / 2;

                double[] amplitude = new double[numberOfBins];
                double[] phase = new double[numberOfBins];


                bool signBool = (enumerationValues[0] % 2) == 0;
                
                amplitude[0] = (signBool ? -1.0 : 1.0) * (enumerationValues[0] / 2) * (1.0 / (1<<numberOfBits)) * 2.0; // multiply with two to get a range from 0 to 1


                for (int iAmplitudeAndPhase = 1; iAmplitudeAndPhase < 6; iAmplitudeAndPhase++) {
                    int enumerationValuesIdx = 2*iAmplitudeAndPhase;

                    if (enumerationValues[enumerationValuesIdx-1] == 0) {
                        amplitude[iAmplitudeAndPhase*configStride] = 0;
                        phase[iAmplitudeAndPhase*configStride] = 0;
                        continue;
                    }

                    
                    double sign = ((enumerationValuesIdx-1) % 2) == 0 ? 1 : -1;
                    double enumerationValue = enumerationValues[enumerationValuesIdx-1] * (1.0 / (1<<numberOfBits));
                    //double value = Math.Exp(-(-5.0 + 10.0*enumerationValue)); // use the exponential to shift the distribution to smaller values
                    
                    // previous run double value = Math.Exp(-(-6.0 + 12.0*enumerationValue)); // use the exponential to shift the distribution to smaller values
                    //double value = Math.Exp(-(-6.0 + 13.5*enumerationValue)); // use the exponential to shift the distribution to smaller values
                    //double value = Math.Exp(-(-8.0 + 15.5*enumerationValue)); // use the exponential to shift the distribution to smaller values
                    double value = Math.Exp(configWeightExpRangeMin + enumerationValue*(configWeightExpRangeMax - configWeightExpRangeMin));

                    // sign only for constant addition with frequency zero
                    amplitude[iAmplitudeAndPhase*configStride] = ((enumerationValuesIdx-1) == 0 ? sign : 1.0) * value;
                        //3.5 * sign * ((double)(enumerationValues[enumerationValuesIdx] / 2) / (1<<numberOfBits));
                    phase[iAmplitudeAndPhase*configStride] = Math.PI * (((enumerationValues[enumerationValuesIdx-1+1] % 2) == 0) ? -1 : 1) * ((double)(enumerationValues[enumerationValuesIdx-1+1] / 2) / (1<<numberOfBits));
                }

                if ((enumerationCounter % 10000000) == 0) {
                    searchRuntimeStopwatch.Stop();
                    string formatedElapsedTime = searchRuntimeStopwatch.Elapsed.ToString(@"hh\:mm");
                    searchRuntimeStopwatch.Start();
                    Console.WriteLine("time={0}, enumValues={1}", formatedElapsedTime, string.Join(",", enumerationValues));
                }

                //Console.WriteLine("{0},{1},{2}", amplitude[0], amplitude[1], amplitude[2]);

                learnForAmplitudeAndPhase(
                    network,
                    amplitude,
                    phase,
                    numberOfBins);

                enumerationCounter++;
            }

        }

        void learnForAmplitudeAndPhase(
            AbstractGenericClassicalNetwork<NeuronType, float> network,

            double[] amplitude,
            double[] phase,
            int numberOfBins
        ) {

            CompressedNNSearchHelper.calcInverseFft(numberOfBins, amplitude, phase, logarithmicWidth, fftReal, fftImaginary);

            int fftWeightIdx = 0;

            // transfer fft weight coefficients to neuron weights and configuration
            for( int layerIdx = 1; layerIdx < network.layers.Length; layerIdx++ ) {
                Layer<NeuronType> currentLayer = network.layers[layerIdx];

                foreach (var iNeuron in currentLayer.neurons) {
                    // transfer values to float configuration values of the neuron
                    var neuronConfiguration = iNeuron.neuronConfiguration;
                    for( int neuronConfigIdx = 0; neuronConfigIdx < neuronConfiguration.numberOfConfigurationValues; neuronConfigIdx++) {
                        neuronConfiguration.updateConfiguration(neuronConfigIdx, (float)fftReal[fftWeightIdx++]);
                    }

                    // tranfer values for weights
                    for( int weightIdx = 0; weightIdx < iNeuron.weights.Length; weightIdx++ ) {
                        iNeuron.weights[weightIdx] = (float)fftReal[fftWeightIdx++];
                    }
                }
            }

            float sumOfError = 0;

            foreach (TrainingTuple<float> iTrainingTuple in trainingSet) {
                
                // update input with trainingset
                network.updateInputLayer(iTrainingTuple.input);

                // forward
                network.forwardPropagation();


                // calculate error of output
                float error = calcResultError(network.output, iTrainingTuple.output);
                sumOfError += error;
            }

            
            // store and debug coefficients if network is best
            lock(context.bestNetworkCandidateMutex) {
                bool isNetworkBetter = context.bestNetworkCandidate == null || sumOfError < context.bestNetworkCandidate.error;
                if (isNetworkBetter) {
                    Console.WriteLine("{0}, stride={1}, error={2}", string.Join(",", new List<float>(network.output).ToArray()), configStride, sumOfError);

                    context.bestNetworkCandidate = new CompressedNNSearchLASearchContext.BestNetworkCandidate(
                        fftReal.deepCopy(),
                        fftImaginary.deepCopy(),
                        sumOfError
                    );
                }
            }
        }


        // calc after squared sum error
        private static float calcResultError(IList<float> list, float[] expectedNetworkResult) {
            Trace.Assert(list.Count == expectedNetworkResult.Length);
            float sum = 0;

            for (int i = 0; i < list.Count; i++) {
                float diff = list[i] - expectedNetworkResult[i];
                sum += (diff*diff);
            }
            return sum;
        }
    }
}
