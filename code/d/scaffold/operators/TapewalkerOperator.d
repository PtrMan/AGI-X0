module scaffold.operators.TapewalkOperator;

import std.stdint;

// scaffold inspired

// tapewalker operator
// The input are two values and the (stateful) control mechanism decides how to move the head

struct Context {
}

struct Node {
	uint64_t valueU64;
}

enum EnumTerminationResult {
	SUCCESS,
	ERROR, // hard error, such as out of bounds, too big values, etc
	FAILURE, // soft error
}

struct TapewalkOperatorMixin {
	final EnumTerminationResult run(ref Context context, uint64_t valueA, uint64_t valueB, out uint64_t result, Node*[] configurationArgs) {
		uint startBit = cast(uint)configurationArgs[0].valueU64;
		uint64_t maxNumberOfIterations = configurationArgs[1].valueU64;

		int currentBit = startBit;

		if( currentBit >= valueA.sizeof*8 ) {
			return EnumTerminationResult.ERROR;
		}

		result = 0;

		foreach( iterationCounter; 0..maxNumberOfIterations ) {
			if( currentBit >= valueA.sizeof*8 || currentBit < 0 ) {
				return EnumTerminationResult.SUCCESS; // we return success because it takes the burden from the control mechanism to count the bits or actions. This leads to a more general alogritm of the control mechanism, because the same algorithm can be applied to any bitsize and startingposition. furthermore the algorithm doesn't kow the startposition.
			}

			bool
				actionLeft, actionRight, terminate, executionError,
				resultBit,
				valueABit = (valueA & (1 << currentBit)) != 0,
				valueBBit = (valueB & (1 << currentBit)) != 0;

			// call mixin method
			control(valueABit, valueBBit, /*out*/resultBit, /*out*/actionLeft, /*out*/actionRight, /*out*/terminate, /*out*/executionError);

			if( executionError ) {
				return EnumTerminationResult.ERROR;
			}

			if( resultBit ) {
				result |= (1 << currentBit);
			}
			else {
				result = result & ~cast(uint64_t)(1 << currentBit);
			}

			if( terminate ) {
				return EnumTerminationResult.SUCCESS;
			}

			if( actionLeft ) {
				currentBit++;
			}
			if( actionLeft ) {
				currentBit--;
			}
		}

		// if we are here we ran out of cycles
		return EnumTerminationResult.ERROR;
	}
}
