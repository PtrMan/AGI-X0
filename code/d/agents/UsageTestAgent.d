import misc.Guid;

import distributed.agent.Agent;


import std.stdio : writeln;

import misc.FiniteStateMachine;
import misc.TracingLogging;
import distributed.GlobalStructs;

final class UsageTestAgent : IAgentCallbacks, IReport {
	final this() {
		Tracer tracer = new Tracer(this);

		agent = new Agent(tracer, this);
		initFsm();
	}



	protected final void initFsm() {
		ControlFsmType.StateTransitions[EnumUsageTestAgentState] transitionTable = [
			EnumUsageTestAgentState.INIT : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.INIT_TO_NEEDCONNECT : EnumUsageTestAgentState.NEEDCONNECT]),
			EnumUsageTestAgentState.NEEDCONNECT : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.CONNECTED : EnumUsageTestAgentState.CONNECTED]),
			EnumUsageTestAgentState.CONNECTED : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.NEEDTOSENDAGENTIDENFICATION : EnumUsageTestAgentState.NEEDTOSENDAGENTIDENFICATION]),
			EnumUsageTestAgentState.NEEDTOSENDAGENTIDENFICATION : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.SENTAGENTIDENTIFICATION_WAITFORRESPONSE : EnumUsageTestAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE]),
			EnumUsageTestAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.CONNECTTOSERVICE : EnumUsageTestAgentState.SENDCONNECTTOSERVICE]),
			EnumUsageTestAgentState.SENDCONNECTTOSERVICE : new ControlFsmType.StateTransitions([
				EnumUsageTestAgentFsmSignal.CONNECTOTOSERVICESENT : EnumUsageTestAgentState.WAITFORCONNECTTOSERVICERESPONSE]),
		];

		controlFsm = ControlFsmType.make(transitionTable);
		controlFsm.signal(EnumUsageTestAgentFsmSignal.INIT_TO_NEEDCONNECT);
	}



	protected final void connectAsClient(string host, ushort port) {
		agent.connectAsClient(host, port);
	}

	final protected void sendAgentIdenficationHandshake() {
		ubyte[16] guid = generateGuid("UsageTestAgent", 1);
		agent.sendAgentIdenficationHandshake(guid);
	}

	final protected void sendAgentConnectToService() {
		AgentConnectToService structure;
		structure.serviceName = "CartesianGeneticProgramming";
		structure.acceptedVersions = [1];
		structure.serviceVersionsAndUp = true; 

		agent.sendAgentConnectToService(structure);
	}

	final void update() {
		agent.update();
		handleFsm();
	}

	protected final void handleFsm() {
		EnumUsageTestAgentState agentState = controlFsm.currentState;

		if( agentState == EnumUsageTestAgentState.NEEDCONNECT ) {
			writeln("[info] connect");

			connectAsClient("127.0.0.1", 1555);
			controlFsm.signal(EnumUsageTestAgentFsmSignal.CONNECTED);
		}
		else if( agentState == EnumUsageTestAgentState.CONNECTED ) {
			//after beeing connected we need to send the agent identification
			controlFsm.signal(EnumUsageTestAgentFsmSignal.NEEDTOSENDAGENTIDENFICATION);
		}
		else if( agentState == EnumUsageTestAgentState.NEEDTOSENDAGENTIDENFICATION ) {
			writeln("[info] called sendAgentIdenficationHandshake()");

			sendAgentIdenficationHandshake();
			controlFsm.signal(EnumUsageTestAgentFsmSignal.SENTAGENTIDENTIFICATION_WAITFORRESPONSE);
		}
		else if( agentState == EnumUsageTestAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE ) {
			// we don't do anything in here for now because the protocol currently doesn't implement the agent identification response

			// for now we change the state
			controlFsm.signal(EnumUsageTestAgentFsmSignal.CONNECTTOSERVICE);
		}
		else if( agentState == EnumUsageTestAgentState.SENDCONNECTTOSERVICE ) {
			writeln("[info] called sendAgentConnectToService()");

			sendAgentConnectToService();

			// TODO< got to state to wait for response >
			controlFsm.signal(EnumUsageTestAgentFsmSignal.CONNECTOTOSERVICESENT);
		}
		else if( agentState == EnumUsageTestAgentState.WAITFORCONNECTTOSERVICERESPONSE ) {
			// not handled here
			// see agentConnectToServiceResponse()
		}

		
	}

	////////////////////////////////////
	// implementation of IAgentCallbacks

	protected override void agentConnectToServiceResponse(AgentConnectToServiceResponse structure) {
		if( controlFsm.currentState != EnumUsageTestAgentState.WAITFORCONNECTTOSERVICERESPONSE ) {
			writeln("[warning] agentConnectToServiceResponse() called but was not in receiving state, ignored");
			return;
		}

		writeln("[info] agentConnectToServiceResponse()");
		writeln("[info]    serviceName     : ", structure.serviceName);
		writeln("[info]    providedVersions: ", structure.providedVersions);
		writeln("[info]    connectSuccess  : ", structure.connectSuccess);
		
		if( structure.connectSuccess ) {
			writeln("[success]   connecting to service was successful");

			// TODO< use service >
		}
		else {
			writeln("[error]   failed -> keep in state");
		}
	}

	protected override void queueMessageWithFlowControl(ref QueueMessageWithFlowControl structure) {
		// TODO
	}

	//////////////////////////////
	// implement interface IReport
	protected final override void reportError(EnumErrorType errorType, string message) {
		report("noncritical", message);
	}


	protected final override void report(string prefix, string message) {
		writeln("[", prefix, "] ", message);
	}
	
	protected Agent agent;

	protected enum EnumUsageTestAgentState {
		INIT = 0, // must be zero
		NEEDCONNECT,
		CONNECTED,
		NEEDTOSENDAGENTIDENFICATION,
		WAITFORAGENTIDENTIFICATIONRESPONSE,
		SENDCONNECTTOSERVICE,
		WAITFORCONNECTTOSERVICERESPONSE
	}

	protected enum EnumUsageTestAgentFsmSignal {
		INIT_TO_NEEDCONNECT,
		CONNECTED,
		NEEDTOSENDAGENTIDENFICATION,
		SENTAGENTIDENTIFICATION_WAITFORRESPONSE,
		CONNECTTOSERVICE,
		CONNECTOTOSERVICESENT
	}

	ControlFsmType controlFsm;

	protected alias FiniteStateMachine!(EnumUsageTestAgentState, EnumUsageTestAgentFsmSignal) ControlFsmType;
}


void main() {
	UsageTestAgent agent = new UsageTestAgent();
	


	// TODO< create context by using context and use service >


	for(;;) {
		agent.update();
	}
}
