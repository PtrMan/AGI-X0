using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace misc {
    class Gaussian {
        public static float calculateGaussianDistribution(float x, float mu, float delta) {
            const float ONEDIVSQRTTWO = 0.707107f; // 1.0 / Sqrt(2.0)

            float distToMean;

            distToMean = x - mu;

            return ONEDIVSQRTTWO * delta * gaussianExponentTerm(distToMean, delta);
        }

        // http://de.wikipedia.org/wiki/David_Marr
        /**
            * 
            * also known as the "Mexican hat" function
            * 
            */
        public static float calculateMarrWavelet(float x, float y, float delta) {
            float distToMean;
            float factorA;
            float factorB;
            float gaussianTerm;

            distToMean = x * x + y * y;

            factorA = -1.0f / ((float)System.Math.PI * delta * delta * delta * delta);
            factorB = 1.0f - distToMean / (2.0f * delta * delta);
            gaussianTerm = gaussianExponentTerm(distToMean, delta);

            return factorA * factorB * gaussianTerm;
        }

        /**
            * 
            * has extra function because it is used for other stuff
            * 
            */
        public static float gaussianExponentTerm(float distToMean, float delta) {
            return (float)System.Math.Exp(-0.5f * ((distToMean * distToMean) / (delta * delta)));
        }
    }
}
