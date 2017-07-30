using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteSphereEngine.subsystems.sound {
    // abstraction for concrete implementation of the sound engine
    public interface ISoundEngine {
        // throws exception on hard error
        // must not be called after not calling shutdown
        void initialize();

        // ...
        // can be called without calling initialize
        void shutdown();

        // play the sound generated with an generator
        // the generator can decide when the stream ends
        // the delegate can be called in another thread, so be careful about statemodifications
        void playGenerator(SoundEngineTypes.GeneratorDelegateType generator);

        GeneratorHandle playGeneratorAndGetHandle(SoundEngineTypes.GeneratorDelegateType generator);
    }

    public abstract class GeneratorHandle {
        public abstract void stopAndDestroy();
    }

    public class SoundEngineTypes {
        public delegate void GeneratorDelegateType(float[] resultArray, out bool finished);
    }


    // encapsulation to give sound stuff names and retrive it by names
    public class SoundEngineUtilityNamedObjects {
        public SoundEngineUtilityNamedObjects(ISoundEngine soundEngine) {
            this.soundEngine = soundEngine;
        }
        
        /*public void playNamedGenerator(SoundEngineTypes.GeneratorDelegateType generator, string name) {
         * Trace.Assert(isInitialized);
         * 
            throw new NotImplementedException();
        }

        public GeneratorHandle retrieveGeneratorByNameOrThrow(string name) {
         * Trace.Assert(isInitialized);
         * 
            throw new NotImplementedException();
        }*/

        
        ISoundEngine soundEngine;
    }
}
