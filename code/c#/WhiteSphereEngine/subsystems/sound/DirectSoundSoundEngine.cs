using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace WhiteSphereEngine.subsystems.sound {
    public class DirectSoundSoundEngine : ISoundEngine {
        // TODO< refactor to use logger >
        public DirectSoundSoundEngine(int logger) {

        }

        public void initialize() {
            // TODO< log >

            if (isInitialized) {
                throw new Exception("Multiple initalization is illegal!");
            }

            playback = new DirectSoundPlayback();
            playback.setFillBufferDelegate(fillBuffer);

            // starts and initializes directsound
            playback.run();

            Debug.Assert( (playback.bufferSizeInSamples%2) == 0 );
            mixingFilter = new ScaledAdditionFilterForFloat(playback.bufferSizeInSamples/2);

            isInitialized = true;
        }

        public void shutdown() {
            if (!isInitialized) {
                return;
            }

            playback.shutdownAndDispose();
            playback = null;

            isInitialized = false;
        }

        public void playGenerator(SoundEngineTypes.GeneratorDelegateType generator) {
            Trace.Assert(isInitialized);
            playGeneratorAndGetHandle(generator);
        }

        public GeneratorHandle playGeneratorAndGetHandle(SoundEngineTypes.GeneratorDelegateType generator) {
            Trace.Assert(isInitialized);

            generators.Add(generator);

            throw new NotImplementedException();
        }

        // is called from direct sound playback
        // can happen in any thread, but we don't care because we don't mutate unsyncronized state(s)
        // should be processes fast, under ~50ms which is not a problem
        void fillBuffer(ref float[] buffer) {
            // must be half length because the playback is doing double buffering
            Debug.Assert(buffer.Length == playback.bufferSizeInSamples/2);

            // TODO< get data from generators >

            mixingFilter.process();

            float[] dequeuedBuffer;
            bool wasDequeued = mixingFilter.mixed.TryDequeue(out dequeuedBuffer);
            if (wasDequeued) { // to avoid returning null
                buffer = dequeuedBuffer;
            }
            else {
                // null it
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] = 0.0f;
                }
            }
            int breakpointHere5 = 42;
        }

        IList<SoundEngineTypes.GeneratorDelegateType> generators = new List<SoundEngineTypes.GeneratorDelegateType>();

        ScaledAdditionFilterForFloat mixingFilter;

        DirectSoundPlayback playback;

        bool isInitialized;
    }
}
