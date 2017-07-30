using System;

namespace MetaNix.schmidhuber.slimRnn {
    public static class SlimRnnDebug {
        public static void debugConnections(SlimRnn slimRnn) {
            Console.WriteLine("SlimRnn");

            int neuronIndex = 0;
            foreach ( SlimRnnNeuron iNeuron in slimRnn.neurons ) {
                Console.WriteLine("   neuron idx={0}", neuronIndex);

                int connectionIndex = 0;
                foreach( var iConnection in iNeuron.outNeuronsWithWeights ) {
                    Console.WriteLine("      connection idx={0} wasUsed={1} weight={2}", connectionIndex, iConnection.wasUsed, iConnection.weight);
                    connectionIndex++;
                }

                neuronIndex++;
            }
        }
    }
}
