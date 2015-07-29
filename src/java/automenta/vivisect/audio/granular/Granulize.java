package automenta.vivisect.audio.granular;

import automenta.vivisect.audio.SoundProducer;
import automenta.vivisect.audio.sample.SonarSample;
import com.google.common.util.concurrent.AtomicDouble;

public class Granulize implements SoundProducer, SoundProducer.Amplifiable {

	private final float[] sourceBuffer;
	private float now = 0L;

    /** this actually represents the target amplitude which the current amplitude will continuously interpolate towards */
    public final AtomicDouble amplitude = new AtomicDouble(1.0);

    protected float currentAmplitude = amplitude.floatValue();

	public final AtomicDouble stretchFactor = new AtomicDouble(1.0);
    public final AtomicDouble pitchFactor = new AtomicDouble(1.0);

    /** grains are represented as a triple of long integers (see Granulator.createGrain() which constructs these) */
	private long[] currentGrain = null;
	private long[] fadingGrain = null;

	private final Granulator granulator;
	private boolean isPlaying = false;
	private float playTime = 0L;
	private int playOffset;



    public Granulize(SonarSample s, float grainSizeSecs, float windowSizeFactor) {
        this(s.buf, s.rate, grainSizeSecs, windowSizeFactor);
        play();
    }

	public Granulize(float[] buffer, float sampleRate, float grainSizeSecs, float windowSizeFactor) {
		this.sourceBuffer = buffer;
		this.granulator = new Granulator(buffer, sampleRate, grainSizeSecs, windowSizeFactor);
	}

	public void process(float[] output, int readRate) {
		if (currentGrain == null && isPlaying) {
			currentGrain = createGrain(currentGrain);
		}
        final float dNow = ((granulator.sampleRate / (float)readRate)) * pitchFactor.floatValue();

        float amp = currentAmplitude;
        float dAmp = (amplitude.floatValue() - amp) / output.length;

        float n = now;

        final Granulator g = granulator;


        final boolean p = isPlaying;
        if (!p)
            dAmp = (0 - amp) / output.length; //fade out smoothly if isPlaying false

        final long samples = output.length;

        long[] cGrain = currentGrain;
        long[] fGrain = fadingGrain;

		for (int i = 0; i < samples; i++ ) {
            float nextSample = 0;
            long lnow = (long)n;
			if (cGrain != null) {
				nextSample = g.getSample(cGrain, lnow);
				if (g.isFading(cGrain, lnow)) {
					fGrain = cGrain;
                    if (p)
                        cGrain = createGrain(cGrain);
                    else
                        cGrain = null;
				}
			}
			if (fGrain != null) {
                nextSample += g.getSample(fGrain, lnow);
				if (!g.hasMoreSamples(fGrain, lnow)) {
					fGrain = null;
				}
			}
			n += dNow;
            output[i] = nextSample * amp;
            amp += dAmp;
		}


        //access and modify these fields only outside of the critical rendering loop
        currentGrain = cGrain;
        fadingGrain = fGrain;
        now = n;
        currentAmplitude = amp;
	}

    public void setAmplitude(float amplitude) {
        this.amplitude.set(amplitude);
    }

    @Override
    public float getAmplitude() {
        return amplitude.floatValue();
    }

    public void play() {
		playOffset = 0;
		playTime = now;
		isPlaying = true;
	}

	public void stop() {
		isPlaying = false;
	}

	private long[] createGrain(long[] targetGrain) {
		//System.out.println("create grain: " + calculateCurrentBufferIndex() + " " + now);
        targetGrain = granulator.createGrain(targetGrain, calculateCurrentBufferIndex(), (long)now);
        return targetGrain;
	}

	private int calculateCurrentBufferIndex() {
        float sf = stretchFactor.floatValue();

		return (playOffset + Math.round((now - playTime) / sf)) % sourceBuffer.length;
	}

	public Granulize setStretchFactor(float stretchFactor) {
		playOffset = calculateCurrentBufferIndex();
		playTime = now;
		this.stretchFactor.set(stretchFactor);
        return this;
	}

    @Override
    public float read(float[] buf, int readRate) {
        process(buf, readRate);
        return 0f;
    }

    @Override
    public void skip(int samplesToSkip, int readRate) {
        //TODO
    }

    @Override
    public boolean isLive() {
        return isPlaying;
    }



}
