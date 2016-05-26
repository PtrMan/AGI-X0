module sensors.internal.Counter;

import std.variant;

import sensors.internal.IInternalSensor;

class Counter : IInternalSensor {
	public final @property string humanreadableSensorName() {
		return "Counter";
	}

	public final @property EnumType type() {
		return IInternalSensor.EnumType.COUNTER;
	}

	public final void restart() {
		// TODO< throw somehing >
	}

	public final void stop() {
		// TODO< throw somehing >
	}

	public final void reset() {
		currentCounter = 0;
	}

	public final void increment(ulong delta) {
		currentCounter += delta;
	}

	public final @property Variant value() {
		return Variant(currentCounter);
	}

	protected ulong currentCounter;
}
