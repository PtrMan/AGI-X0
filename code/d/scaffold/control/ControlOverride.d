module scaffold.control.ControlOverride;

// overrides some control values which are returned by the inner control function
// is a class because we have default values for easy readability
class ControlOverride {
	final void control(bool valueA, bool valueB, out bool result, out bool left, out bool right, out bool terminate, out bool executionError) {
		innerControl(valueA, valueB, result, left, right, terminate, executionError);
		if( executionError ) {
			return;
		}

		if( overrideResult ) {
			result = overrideResultValue;
		}
		if( overrideLeft ) {
			left = overrideLeftValue;
		}
		if( overrideRight ) {
			right = overrideRightValue;
		}
		if( overrideTerminate ) {
			terminate = overrideTerminateValue;
		}
	}

	bool
		overrideResult = false,
		overrideResultValue,
		overrideLeft = false,
		overrideLeftValue,
		overrideRight = false,
		overrideRightValue,
		overrideTerminate = false,
		overrideTerminateValue;


	alias void delegate(bool valueA, bool valueB, out bool result, out bool left, out bool right, out bool terminate, out bool executionError) ControlType;
	ControlType innerControl;
}
