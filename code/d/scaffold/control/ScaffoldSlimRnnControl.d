module scaffold.control.ScaffoldSlimRnnControl;

import slimRnn.SlimRnn;

// mixin structure
// passes the control to a slimRnn and feeds the control values back
class ControlSlimRnn {
	SlimRnn slimRnn;

	uint slimRnnMaxIterations;

	// -1 means it wont be read
	int
		controlResultResultIndex = 0,
		controlResultLeftIndex = 1,
		controlResultRightIndex = 2,
		controlResultTerminateIndex = 3;

	float booleanThreshold = 0.5f;
	float inputStrength = 1.0f;

	// called by the operator(s) for requesting the next control command
	private final void controlSlimRnn(bool valueA, bool valueB, out bool result, out bool left, out bool right, out bool terminate, out bool executionError) {
		uint iterations;
		bool wasTerminated, slimRnnExecutionError;
		slimRnn.map.arr[0] = valueA ? inputStrength : 0.0f;
		slimRnn.map.arr[1] = valueB ? inputStrength : 0.0f;
		slimRnn.run(slimRnnMaxIterations, /*out*/iterations, /*out*/wasTerminated, /*out*/slimRnnExecutionError);

		executionError = !wasTerminated || slimRnnExecutionError;
		if( executionError ) {
			return;
		}

		result = readBoolean(controlResultResultIndex);
		left = readBoolean(controlResultLeftIndex);
		right = readBoolean(controlResultRightIndex);
		terminate = readBoolean(controlResultTerminateIndex);
	}

	private final bool readBoolean(int index) {
		if( index == -1 ) {
			return false;
		}
		return slimRnn.outputValues[index] > booleanThreshold;
	}
}
