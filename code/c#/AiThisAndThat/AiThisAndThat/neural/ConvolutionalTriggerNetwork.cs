using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using misc;

namespace neural {
    /**
     * key principle of this network is that neurons sample from a blured map of neuron activations, neurons fire if the input is stronger than a threshold
     * 
     * 
     * 
     */


    class Neuron {
        public Neuron(float threshold, List<Vector2d<uint>> probePositions, float targetStrength, Vector2d<uint> targetPosition) {
            this.threshold = threshold;
            this.probePositions = probePositions;
            this.targetStrength = targetStrength;
            this.targetPosition = targetPosition;
        }

        public List<Vector2d<uint>> probePositions = new List<Vector2d<uint>>();
        public float threshold;
        public Vector2d<uint> targetPosition;
        public float targetStrength;

        bool isFiring(Map2d<float> map) {
            float activation = 0.0f;
            
            foreach(Vector2d<uint> iterationProbePosition in probePositions ) {
                activation += map.read(iterationProbePosition);
            }

            return activation > threshold;
        }
    }

    // rating for genetic algorithm
    class Rating : evolutionaryAlgorithms.geneticAlgorithm.GeneticAlgorithm.IRating {
        public uint numberOfProbesForEachNeuron = 3;
        public uint numberOfNeurons = 15;

        public void rating(List<evolutionaryAlgorithms.geneticAlgorithm.Genome> genomes) {
            foreach (evolutionaryAlgorithms.geneticAlgorithm.Genome iterationGenome in genomes) {
                bool[] genome = iterationGenome.genome;

                int currentIndex = 0;

                int genomeNumberOfActiveNeurons = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, 4); currentIndex += 4;
                int numberOfActiveNeurons = Math.Min(genomeNumberOfActiveNeurons, (int)numberOfNeurons);

                List<Neuron> neurons = new List<Neuron>();

                for (int neuronI = 0; neuronI < numberOfActiveNeurons; neuronI++) {
                    List<Vector2d<uint>> probePositions = readProbePositions(ref currentIndex, genome);

                    int activateProbes = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, 3); currentIndex += 3;
                    activateProbes = Math.Min(activateProbes, (int)numberOfProbesForEachNeuron);

                    // limit list
                    probePositions.RemoveRange(activateProbes, Math.Max(0, activateProbes - probePositions.Count));

                    // read targetPosition
                    Vector2d<uint> targetPosition = readPosition(ref currentIndex, genome);


                    // read threshold
                    float maxThreshold = 5.0f;
                    int thresholdBits = 5;
                    int thresholdInt = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, thresholdBits); currentIndex += thresholdBits;
                    float threshold = ((float)thresholdInt / (float)(1 << thresholdBits)) * maxThreshold;

                    // read target strength
                    float maxTargetStrength = 5.0f;
                    int targetStrengthBits = 5;
                    int targetStrengthInt = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, targetStrengthBits); currentIndex += targetStrengthBits;
                    float targetStrength = ((((float)targetStrengthInt / (float)(1 << targetStrengthBits)) * maxTargetStrength) * 2.0f) - maxTargetStrength;


                    neurons.Add(new Neuron(threshold, probePositions, targetStrength, targetPosition));
                }
                
                // TODO< evaluate network >
                // TODO
                Debug.Assert(false, "TODO");
            }
        }

        private List<Vector2d<uint>> readProbePositions(ref int currentIndex, bool[] genome) {
            List<Vector2d<uint>> probePositions = new List<Vector2d<uint>>();

            for (int probeI = 0; probeI < numberOfProbesForEachNeuron; probeI++) {
                probePositions.Add(readPosition(ref currentIndex, genome));
            }

            return probePositions;
        }

        private Vector2d<uint> readPosition(ref int currentIndex, bool[] genome) {
            // TODO< use bitstream facility >
            int x = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, 5); currentIndex += 5;
            int y = evolutionaryAlgorithms.geneticAlgorithm.Helper.toUint(genome, currentIndex, 5); currentIndex += 5;

            return new Vector2d<uint>((uint)x, (uint)y);
        }
    }
    
    class ConvolutionalTriggerNetwork {


        public static void test() {
            Map2d<float> toConvolute = new Map2d<float>(new Vector2d<uint>(10, 10));

            toConvolute.write(new Vector2d<uint>(3, 2), 5.0f);
            toConvolute.write(new Vector2d<uint>(7, 5), -2.0f);

            Map2d<float> kernel = createGaussianKernel(5);
            

            Map2d<float> convolutionResult = Convolution2d.convolution(toConvolute, kernel);

            int debugBreakpointHere = 1;


            
        }

        static private Map2d<float> createGaussianKernel(uint size) {
            Map2d<float> resultKernel = new Map2d<float>(new Vector2d<uint>(size, size));

            float variance = (float)System.Math.Sqrt(0.15f);
            float normalisation = Gaussian.calculateGaussianDistribution(0.0f, 0.0f, variance);

            for( int y = 0; y < size; y++) {
                for( int x = 0; x < size; x++) {
                    float relativeX = ((float)x / size) * 2.0f - 1.0f;
                    float relativeY = ((float)y / size) * 2.0f - 1.0f;

                    float distanceFromCenter = (float)System.Math.Sqrt(relativeX*relativeX + relativeY*relativeY);

                    float gaussianResult = Gaussian.calculateGaussianDistribution(distanceFromCenter, 0.0f, variance);
                    float normalizedGaussianResult = gaussianResult / normalisation;
                    resultKernel.write(new Vector2d<uint>((uint)x, (uint)y), normalizedGaussianResult);
                }
            }

            return resultKernel;
        }
    }
}
