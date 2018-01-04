using System;
using System.Diagnostics;

namespace AiThisAndThat {
    public interface IForwardPropagatableNetwork {
        void forwardPropagate(float[] input);
    }

    public interface IWeightedNetwork {
        int numberOfWeights {get;}

        void updateWeights(float[] weights);
    }

    public interface IBiasedNetwork {
        int numberOfBiases {get;}

        void updateBiases(float[] biases);
    }

    public interface IHasResult<Type> {
        Type[] result {get;}
    }


    public class Layer<NeuronPropertiesType> {
        public NeuronPropertiesType[] neurons;
    }

    
    public static class Activation {
        public static float exp(float v) {
            return MathMisc.expFast10(v); // fast approximation
        }

        public static float tanh(float v) {
            return (float)Math.Tanh(v);
        }


        // https://en.wikipedia.org/wiki/Rectifier_(neural_networks)#ELUs
        public static float elu(float v, float alpha) {
            return v > 0 ? v : alpha*((float)Math.Exp(v)-1);
        }
    }

    public static class NeuralNetworkMath {
        public static float dot(float[] a, float[] b) {
            Debug.Assert(a.Length == b.Length);
            
            float res = 0;
            for( int i = 0; i < a.Length; i++ )    res += (a[i] * b[i]);
            return res;
        }
    }

    public static class Permutation {
        // /param res must already be resized to the right size
        public static void identity<Type>(Type[] src, ref Type[] res) {
            res = src;
        }

        // reads from src by the indices idxs
        // /param res must already be resized to the right size
        public static void permutation<Type>(Type[] src, ref Type[] res, int[] idxs) {
            Debug.Assert(res.Length == idxs.Length);

            for( int i = 0; i < idxs.Length; i++ ) {
                int idx = idxs[i];
                Type v = src[idx];
                res[i] = v;
            }
        }
    }



    

    
}
