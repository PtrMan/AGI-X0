using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteSphereEngine.subsystems.sound {
    
    // calculates sinus "rotations" with my (PtrMan/Square) DSP math trick
    // I couldn't find this trick in any DSP resource
    // the trick is to just take a (usually normalized) vector and rotate it with its own tangent
    public class FastOscillator {
        float x, y;

        float sampleFactor = 0.0f; // how much do we rotate per sample?
        float renormalization = 1.0f;

        int samplesPerSecond;

        float sweepFactor = 1.0f; // use this to sweep the frequency with time
                                  // this comes in this formulaztion for free

        public FastOscillator() {
            reset();
            samplesPerSecond = 44100;
        }

        // /param frequency
        // /param sweepFactor use this to sweep up or down the frequency with time
        // /param decayHalflife after how many seconds has the 
        public void recalcSinus(
            float frequency,
            float sweepFactor = 1.0f,
            double decayRate = 0.0
        ) {

            this.sweepFactor = sweepFactor;

            float anglePerSample = (frequency / ((float)samplesPerSecond)) * 2.0f * (float)System.Math.PI;

            sampleFactor = (float)System.Math.Tan(anglePerSample);


            { // recalc renormalization
                float diffX = 1.0f;
                float diffY = 0.0f + 1.0f * sampleFactor;

                renormalization = 1.0f / (float)System.Math.Sqrt(diffX*diffX + diffY*diffY);
            }

            double deltaTime = 1.0 / samplesPerSecond;

            // calculate decay
            // TODO< refactor with using our decay equation from the nuclear decay code >
            double decayFactor = 1.0;
            double remainingAfterDecay = System.Math.Exp(-(decayRate*deltaTime));
            decayFactor = remainingAfterDecay;

            
            // factor in decay to renormalization
            renormalization = (float)(renormalization * decayFactor);
            
            
        }

        // /param volume how loud is it
        public void reset(float volume = 1.0f) {
            x = 0.0f;
            y = volume;
        }

        public void sampleIntoBuffer(float[] arr, ref int j) {
            for (int i = 0; i < arr.Length; i++) {
                float
                    crossX = y,
                    crossY = -x,

                    newX = x + crossX * sampleFactor,
                    newY = y + crossY * sampleFactor;

                newX *= renormalization;
                newY *= renormalization;

                x = newX;
                y = newY;

                sampleFactor *= sweepFactor;

                // wrap around in the buffer
                arr[(j+i) % arr.Length] = y;
            }
        }

        public bool checkIsSilent(float threshold = SILENCE_TRESHOLD) {
            // fast way to check distance
            return x*x + y*y < threshold*threshold;
        }

        const float SILENCE_TRESHOLD = 0.001f; // 
    }
}
