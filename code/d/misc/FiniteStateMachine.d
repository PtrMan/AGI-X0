module misc.FiniteStateMachine;

class FiniteStateMachine(StateType, SignalType) {
	static class StateTransitions {
		public StateType[SignalType] next;
	}

	public static FiniteStateMachine!(StateType, SignalType) make(StateTransitions[StateType] stateTransitions) {
		return new FiniteStateMachine!(StateType, SignalType)(stateTransitions);
	}

	protected final this(StateTransitions[StateType] stateTransitions) {
		this.stateTransitions = stateTransitions;
		protectedCurrentState = cast(StateType)0;
	}

	public final void signal(SignalType signalValue) {
		protectedCurrentState = stateTransitions[protectedCurrentState].next[signalValue];
	}

	public final @property StateType currentState() {
		return protectedCurrentState;
	}

	protected StateType protectedCurrentState;

	protected StateTransitions[StateType] stateTransitions;
}
