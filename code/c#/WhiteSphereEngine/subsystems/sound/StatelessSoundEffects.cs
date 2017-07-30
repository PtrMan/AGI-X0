using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteSphereEngine.subsystems.sound {
    // sound effects which don't hold a state
    public class StatelessSoundEffects {
        private StatelessSoundEffects() { } // just static members

        public static void exponentInPlaceFast(float[] arr, float multiplicator) {
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = expFast(arr[i] * multiplicator);
            }
        }

        // TODO< move to math helper >

        // from https://codingforspeed.com/using-faster-exponential-approximation/
        static float expFast(float x) {
          // exponent doesn't have to be 8, can be other number too, like for example 10 like in the article
          x = (float)1.0 + x / (float)(1<<8);
  
          for( int n = 0; n < 8; n++ ) {
             x *= x;
          }
          return x;
        }
    }
}
