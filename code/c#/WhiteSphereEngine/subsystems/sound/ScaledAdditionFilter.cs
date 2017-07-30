using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WhiteSphereEngine.subsystems.sound {
    // asycnronous filter which adds the inputs with an scalar multiplier
    // is constructed after the architectural pattern "pipers and filters", makes software more composable
    // can be used as a mixer for the sound engine
    public class ScaledAdditionFilterForFloat {
        public ScaledAdditionFilterForFloat(uint blocksize) {
            scratchpad = new float[blocksize];
        }

        public void resizeChannels(int numberOfChannels) {
            channels = new Channel[numberOfChannels];
        }

        public void enqueueBlockForChannel(int channel, float[] block) {
            Debug.Assert(block.Length == scratchpad.Length);
            channels[channel].dataBlocks.Enqueue(block);
        }

        public void setMultiplier(int channel, float multiplier) {
            channels[channel].multiplier = multiplier;
        }

        // can block
        public void process() {
            for(;;) {
                if (!channels.Any(v => v.dataBlocks.Count > 0)) {
                    break;
                }

                // null
                nullScatchpad();

                // add
                foreach (Channel iChannel in channels) {
                    addToScaled(scratchpad, iChannel.dataBlocks.First(), iChannel.multiplier);
                }

                // remove
                foreach (Channel iChannel in channels) {
                    float[] dequeuResult;
                    iChannel.dataBlocks.TryDequeue(out dequeuResult);
                }

                // push result
                float[] mixed_ = new float[mixed.Count];
                scratchpad.CopyTo(mixed_, 0);
                mixed.Enqueue(mixed_);
            }
        }

        private static void addToScaled(float[] dest, float[] source, float multiplier) {
            Debug.Assert(dest.Length == source.Length);
            for (int i = 0; i < dest.Length; i++) {
                dest[i] += source[i];
            }
        }

        void nullScatchpad() {
            for (int i = 0; i < scratchpad.Length; i++) {
                scratchpad[i] = 0.0f;
            }
        }

        public uint blocksize { get {
                return (uint)scratchpad.Length;
            }
        }

        float[] scratchpad;

        class Channel {
            public ConcurrentQueue<float[]> dataBlocks = new ConcurrentQueue<float[]>();
            public float multiplier = 0.0f;
        }

        Channel[] channels = new Channel[0];

        public ConcurrentQueue<float[]> mixed = new ConcurrentQueue<float[]>();
    }
}
