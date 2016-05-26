module sensors.internal.Wallclock;

import core.time : TickDuration;
import std.datetime : StopWatch;
import std.variant;

import sensors.internal.IInternalSensor;

class Wallclock : IInternalSensor {
	public final @property string humanreadableSensorName() {
		return "Wallclock";
	}

	public final @property EnumType type() {
		return IInternalSensor.EnumType.MEASURE;
	}

	public final void restart() {
		if( stopWatch.running ) {
			stopWatch.stop();
		}
		else {
			stopWatch.start();
		}
	}

	public final void stop() {
		stopWatch.stop();
	}

	public final void reset() {
		stopWatch.reset();
	}



	public final void increment(ulong delta) {
		// ignore
		// TODO< throw an error? >
	}


	// returns time in nanoseconds
	public final @property Variant value() {
		TickDuration duration = stopWatch.peek();
		return Variant(duration.nsecs);
	}

	protected StopWatch stopWatch;
}