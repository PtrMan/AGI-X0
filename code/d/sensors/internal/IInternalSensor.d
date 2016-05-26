module sensors.internal.IInternalSensor;

import std.variant;

interface IInternalSensor {
	public enum EnumType {
		COUNTER, // system can increment the sensor like a counter
		MEASURE // just passivly measures something
	}

	@property EnumType type();

	@property string humanreadableSensorName();

	void restart();
	void stop();
	void reset();

	void increment(ulong delta);

	@property Variant value();
}
