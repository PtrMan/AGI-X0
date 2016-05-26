import std.stdio : writeln;

import sensors.internal.Wallclock;

void main() {

	Wallclock sensorWallclock = new Wallclock();

	sensorWallclock.restart();

	foreach( i; 0..50 ) {
		writeln(i);
	}

	sensorWallclock.stop();

	writeln(sensorWallclock.value, " ns");
}
