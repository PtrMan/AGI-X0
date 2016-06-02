
import network.Networking;
import network.AbstractNetworking;

import misc.Guid;



import distributed.GlobalStructs;
import misc.Guid;
import misc.TracingLogging;
import misc.BitstreamDestination;
import serialisation.BitstreamWriter;
import misc.GenericSerializer;

// TODO< move into own file >

interface IAgentCallbacks {
	protected void agentConnectToServiceResponse(AgentConnectToServiceResponse structure);
}

// common functionality of each agent
// doesn't carry/control state >
final class Agent : INetworkCallback {
	final this(Tracer tracer, IAgentCallbacks agentCallbacks) {
		networkHost = new NetworkHost(this, AbstractNetworkHost!NetworkClient.EnumRole.CLIENT, tracer);
		this.tracer = tracer;
		this.agentCallbacks = agentCallbacks;
	}

	final void connectAsClient(string host, ushort port) {
		networkHost.connectAsClient(host, port);
	}

	final void update() {
		networkHost.iteration();
	}

	public final void sendAgentIdenficationHandshake(ubyte[16] guid) {

		BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
		BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

		bool successChained = true;


		bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTIDENTIFICATIONHANDSHAKE, 16, successChained); // type of message
		
		{
			AgentIdentificationHandshake agentIdentificationHandshake;
			agentIdentificationHandshake.guid = guid;
			serialize(agentIdentificationHandshake, successChained, bitstreamWriterForPayload);

			if( !successChained ) {
				tracer.internalEvent("serialisation failed!", agentIdentificationHandshake, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
			}
		}


		networkHost.sendMessageToClient(networkHost.getClientForRoleClient(), bitstreamDestinationForPayload);
	}

	public final void sendAgentConnectToService(AgentConnectToService structure) {
		BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
		BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

		bool successChained = true;


		bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTCONNECTTOSERVICE, 16, successChained); // type of message
		
		{
			serialize(structure, successChained, bitstreamWriterForPayload);

			if( !successChained ) {
				tracer.internalEvent("serialisation failed!", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
			}
		}


		networkHost.sendMessageToClient(networkHost.getClientForRoleClient(), bitstreamDestinationForPayload);
	}

	/////////////////////////////
	// implement INetworkCallback
	protected final override void networkCallbackRegisterServices(NetworkClient networkClient, ref RegisterServices structure) {
		tracer.internalEvent("called, ignored because in role \"Agent\"", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
	}

	protected final override void networkCallbackAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
		tracer.internalEvent("called, ignored because in role \"Agent\"", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
	}

	protected final override void networkCallbackAgentCreateContext(NetworkClient client, ref AgentCreateContext structure) {
		tracer.internalEvent("called, ignored because in role \"Agent\"", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
	}

	protected final override void networkCallbackAgentConnectToServiceResponse(NetworkClient client, ref AgentConnectToServiceResponse structure) {
		tracer.internalEvent("called", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
		scope(exit) tracer.internalEvent("exit", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		agentCallbacks.agentConnectToServiceResponse(structure);
	}

	final override protected void networkClientDisconnected(NetworkClient client) {
		tracer.internalEvent("called, finishing...", null, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		// TODO
	}

	protected NetworkHost networkHost;
	protected IAgentCallbacks agentCallbacks;
	protected Tracer tracer;
}


import std.stdio : writeln;

import misc.FiniteStateMachine;

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



	final void connectAsClient(string host, ushort port) {
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
			writeln("[succes]   connecting to service was successful");

			// TODO< use service >
		}
		else {
			writeln("[error]   failed -> keep in state");
		}
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
	


	// TODO< create context >


	for(;;) {
		agent.update();
	}
}
