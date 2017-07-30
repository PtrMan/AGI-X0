using System;

using SharpDX.DirectSound;
using SharpDX.Multimedia;
using SharpDX;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace WhiteSphereEngine.subsystems.sound {
    // waits for playback buffer events and refills buffer in own thread
    internal class DirectSoundStreamer {
        // Default WaveFormat Stereo 44100 16 bit
        public WaveFormat waveFormat = new WaveFormat();

        public DirectSoundPlayback.FillBufferDelegateType fillBuffer;

        
        // blocks
        public void shutdown() {
            terminate = true;

            // wait for termination 
            wasTerminated.WaitOne();
        }

        volatile bool terminate = false;

        public uint bufferSizeInSamples {
            get {
                var capabilities = secondarySoundBuffer.Capabilities;
                return (uint)(capabilities.BufferBytes / waveFormat.BlockAlign);
            }
        }

        public void loop() {
            // Get Capabilties from secondary sound buffer
            var capabilities = secondarySoundBuffer.Capabilities;

            FastOscillator oscillator = new FastOscillator();
            float[] tempSampleBuffer = null;
            
            int counter = 0;

            
            int sampleCounter = 0;



            int retriggerCounter = 0;



            IntPtr bufferPtr = Marshal.AllocHGlobal(capabilities.BufferBytes / 2);

            for(;;) {
                for (;;) {
                    bool hasReceivedSignal = wait.WaitOne(200);

                    if (terminate) {
                        goto TerminateLabel;
                    }
                    
                    if (hasReceivedSignal) {
                        break;
                    }
                }
                wait.Reset();

                // retrigger generator if required
                if ((retriggerCounter % 7) == 0) {
                    float frequency = (float)(new Random().Next()) * 18000.0f;

                    oscillator.recalcSinus(frequency, 0.999999f, 3.0);
                    oscillator.reset(0.5f);
                }

                retriggerCounter += 1;


                
                int numberOfSamples3 = (capabilities.BufferBytes / waveFormat.BlockAlign) / 2;

                float[] sampleBuffer = new float[numberOfSamples3];

                // recalculate next samples
                if(false){
                    
                    int sampleBufferOffset = 0;
                    oscillator.sampleIntoBuffer(sampleBuffer, ref sampleBufferOffset);

                    // exponent filter
                    if (true) {
                        StatelessSoundEffects.exponentInPlaceFast(sampleBuffer, /* multiplicator*/3.0f);


                        for (int i = 0; i < sampleBuffer.Length; i++) {
                            sampleBuffer[i] = (sampleBuffer[i] - 10.5f) / 10.0f;
                        }

                    }
                }
                else {
                    fillBuffer(ref sampleBuffer);
                }



                // recalculate buffer for direct sound
                int bufferI = 0; // index into buffer
                for (int offset2 = 0; offset2 < capabilities.BufferBytes / 2; offset2 += waveFormat.BlockAlign)
                {
                    const float scalingFactor = ((1 << 15) / 2);
                    short value = (short)(sampleBuffer[bufferI]*scalingFactor);

                    Marshal.WriteInt16(bufferPtr, offset2, value);
                    Marshal.WriteInt16(bufferPtr, offset2+2, value);

                    bufferI++;

                    sampleCounter++;
                }


                // fill up with next chunk

                
                
                int currentPlayCursorPosition, currentwriteCursorPosition;
                secondarySoundBuffer.GetCurrentPosition(out currentPlayCursorPosition, out currentwriteCursorPosition);

                ///Console.WriteLine("pp={0}, writeCursorPosition={1}", currentPlayCursorPosition, currentwriteCursorPosition);
                
                // we treat the write cursor as if it were the read cursor position
                // because they are close together and it seems tto desync with the read cursor
                int offset = currentwriteCursorPosition > capabilities.BufferBytes / 2 ? 0 : capabilities.BufferBytes / 2;

                // lock, write data, unlock
                
                // TODO BUG< reaquire if not still aquired >
                DataStream dataPart2;
                DataStream dataPart1 = secondarySoundBuffer.Lock(
                    offset,
                    capabilities.BufferBytes / 2,
                    LockFlags.None,
                    out dataPart2);
                Debug.Assert(dataPart2 == null); // must be the case bacause we lock the whole buffer
                dataPart1.Write(bufferPtr, 0, capabilities.BufferBytes / 2);
                secondarySoundBuffer.Unlock(dataPart1, dataPart2);

                counter++;

            }
            TerminateLabel:
            // if we are here, we have to stop playback and free local resoures

            secondarySoundBuffer.Stop();

            wasTerminated.Set();
        }

        public SecondarySoundBuffer secondarySoundBuffer;

        // waits on notifications of playback buffer from direct sound
        public ManualResetEvent
            wait = new ManualResetEvent(false),

            wasTerminated = new ManualResetEvent(false); // used by the termination logic to confirm stopping
    }

    public class DirectSoundPlayback {
        public delegate void FillBufferDelegateType(ref float[] buffer);


        [DllImport("User32.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr GetDesktopWindow();

        DirectSound directSound;
        PrimarySoundBuffer primarySoundBuffer;
        SecondarySoundBuffer secondarySoundBuffer;

        Thread soundStreamerThread;
        DirectSoundStreamer soundStreamer;

        public DirectSoundPlayback() {
            
            // used in another special sound thread to playback sound
            soundStreamer = new DirectSoundStreamer();
        }

        public void setFillBufferDelegate(FillBufferDelegateType fillBuffer) {
            soundStreamer.fillBuffer = fillBuffer;
        }

        public uint bufferSizeInSamples { get {
                return soundStreamer.bufferSizeInSamples;
            }
        }

        public void shutdownAndDispose() {
            soundStreamer.shutdown(); // blocks

            soundStreamerThread.Join();

            secondarySoundBuffer.Dispose();
            primarySoundBuffer.Dispose();

            directSound.Dispose();
            directSound = null;
        }

        public void run() {


            directSound = new DirectSound();

            
            IntPtr hwnd = GetDesktopWindow();
            // Set Cooperative Level to PRIORITY (priority level can call the SetFormat and Compact methods)
            directSound.SetCooperativeLevel(hwnd, CooperativeLevel.Priority);

            // Create PrimarySoundBuffer
            var primaryBufferDesc = new SoundBufferDescription();
            primaryBufferDesc.Flags = BufferFlags.PrimaryBuffer;
            primaryBufferDesc.AlgorithmFor3D = Guid.Empty;

            primarySoundBuffer = new PrimarySoundBuffer(directSound, primaryBufferDesc);


            // Create SecondarySoundBuffer
            int soundLatencyInMilliseconds = 100;

            var secondaryBufferDesc = new SoundBufferDescription();
            secondaryBufferDesc.BufferBytes = soundStreamer.waveFormat.ConvertLatencyToByteSize(soundLatencyInMilliseconds);
            secondaryBufferDesc.Format = soundStreamer.waveFormat;
            secondaryBufferDesc.Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlPositionNotify | BufferFlags.GlobalFocus |
                                        BufferFlags.ControlVolume | BufferFlags.StickyFocus | BufferFlags.Trueplayposition;
            secondaryBufferDesc.AlgorithmFor3D = Guid.Empty;



            secondarySoundBuffer = new SecondarySoundBuffer(directSound, secondaryBufferDesc);

            
            
            NotificationPosition n = new NotificationPosition();
            n.Offset = (secondaryBufferDesc.BufferBytes / 4) * 1;
            n.WaitHandle = soundStreamer.wait;

            NotificationPosition n2 = new NotificationPosition();
            n2.Offset = (secondaryBufferDesc.BufferBytes / 4) * 3;
            n2.WaitHandle = soundStreamer.wait;

            secondarySoundBuffer.SetNotificationPositions(new NotificationPosition[]{n, n2});

            soundStreamerThread = new Thread(soundStreamer.loop);
            soundStreamer.secondarySoundBuffer = secondarySoundBuffer;
            soundStreamerThread.Start();


            // play the sound
            secondarySoundBuffer.Play(0, PlayFlags.Looping);
        }
    }
}
