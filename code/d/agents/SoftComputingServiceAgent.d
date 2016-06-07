import misc.Guid;

import std.stdint;

import distributed.agent.Agent;


import std.stdio : writeln;

import misc.FiniteStateMachine;
import misc.TracingLogging;
import distributed.GlobalStructs;

import misc.Queue;

enum EnumRequestType : uint8_t {
	CARTESIANGENETICPROGRAMMING = 0,
}

enum EnumDescriptorType : uint8_t {
	SETUP = 0,
	GENOTYPES,
}

struct Genotype {
	uint32_t[] genes;
}

struct Descriptor {
	EnumDescriptorType type;

	union  {
		EnumRequestType cartesianGeneticProgrammingType;
	}

	//union {
		//uint16_t[][] typeIdsOfOperatorsToCreate;
		
		
		//struct Setup {
			uint16_t[][] typeIdsOfOperatorsToCreate;

			uint32_t numberOfInputs;
			uint32_t numberOfOutputs;

		//}

		//struct {
			Genotype[] genotypes;
		//}
	//}
}

struct RequestQueueElement {
	uint32_t agentServiceContextId;

	Descriptor descriptor;
}

final class SoftComputingServiceAgent : IAgentCallbacks, IReport {
	final this() {
		Tracer tracer = new Tracer(this);

		agent = new Agent(tracer, this);
		initFsm();
	}



	protected final void initFsm() {
		ControlFsmType.StateTransitions[EnumAgentState] transitionTable = [
			EnumAgentState.INIT : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.INIT_TO_NEEDCONNECT : EnumAgentState.NEEDCONNECT]),
			EnumAgentState.NEEDCONNECT : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.CONNECTED : EnumAgentState.CONNECTED]),
			EnumAgentState.CONNECTED : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.NEEDTOSENDAGENTIDENFICATION : EnumAgentState.NEEDTOSENDAGENTIDENFICATION]),
			EnumAgentState.NEEDTOSENDAGENTIDENFICATION : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.SENTAGENTIDENTIFICATION_WAITFORRESPONSE : EnumAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE]),
			EnumAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.REGISTERSERVICES : EnumAgentState.SENDREGISTERSERVICES]),
			EnumAgentState.SENDREGISTERSERVICES : new ControlFsmType.StateTransitions([
				EnumAgentFsmSignal.TO_STANDBY : EnumAgentState.STANDBY]),
		];

		controlFsm = ControlFsmType.make(transitionTable);
		controlFsm.signal(EnumAgentFsmSignal.INIT);
	}



	protected final void connectAsClient(string host, ushort port) {
		agent.connectAsClient(host, port);
	}

	final protected void sendAgentIdenficationHandshake() {
		ubyte[16] guid = generateGuid("SoftComputingServiceAgent", 1);
		agent.sendAgentIdenficationHandshake(guid);
	}

	protected final void sendRegisterServices() {
		RegisterServices structure;

		structure.service.length = 1;
		structure.service[0].locator.name = "CartesianGeneticProgramming";
		structure.service[0].locator.version_ = 1;

		structure.service[0].capabilities.isSoftComputing = true;
		structure.service[0].capabilities.stateful = EnumStateful.PARTIAL;

		structure.service[0].contract.serviceRequestFrequency = EnumContractServiceRequestFrequency.NONE;
		structure.service[0].contract.serviceUsageFrequency = EnumContractServiceRequestFrequency.SPORADIC;
		structure.service[0].contract.lifecylce = EnumLifecycle.TESTING;

		agent.sendRegisterServices(structure);
	}

	final void update() {
		agent.update();
		handleFsm();
	}

	protected final void handleFsm() {
		EnumAgentState agentState = controlFsm.currentState;

		
		if( agentState == EnumAgentState.NEEDCONNECT ) {
			report("INFO", "connect");

			connectAsClient("127.0.0.1", 1555);
			controlFsm.signal(EnumAgentFsmSignal.CONNECTED);
		}
		else if( agentState == EnumAgentState.CONNECTED ) {
			//after beeing connected we need to send the agent identification
			controlFsm.signal(EnumAgentFsmSignal.NEEDTOSENDAGENTIDENFICATION);
		}
		else if( agentState == EnumAgentState.NEEDTOSENDAGENTIDENFICATION ) {
			report("INFO", "called sendAgentIdenficationHandshake()");

			sendAgentIdenficationHandshake();
			controlFsm.signal(EnumAgentFsmSignal.SENTAGENTIDENTIFICATION_WAITFORRESPONSE);
		}
		else if( agentState == EnumAgentState.WAITFORAGENTIDENTIFICATIONRESPONSE ) {
			// we don't do anything in here for now because the protocol currently doesn't implement the agent identification response

			// for now we change the state
			controlFsm.signal(EnumAgentFsmSignal.REGISTERSERVICES);
		}
		else if( agentState == EnumAgentState.SENDREGISTERSERVICES ) {
			report("INFO", "called sendRegisterServices()");

			sendRegisterServices();

			controlFsm.signal(EnumAgentFsmSignal.TO_STANDBY);
		}
		else if( agentState == EnumAgentState.STANDBY ) {
			// do nothing
		}
		
		
	}

	////////////////////////////////////
	// implementation of IAgentCallbacks

	protected override void agentConnectToServiceResponse(AgentConnectToServiceResponse structure) {
		report("ERROR", "agentConnectToServiceResponse() called but has no functionality, ignored");
	}

	protected override void agentCreatedContext(ref AgentCreatedContext structure) {
		import std.format : format;

		report("INFO", format("agent created context with agentServiceContextId=%d", structure.agentServiceContextId));
		
		// TODO
	}

	protected override void queueMessageWithFlowControl(ref QueueMessageWithFlowControl structure) {
		import misc.BitstreamSource;
		import misc.GenericSerializer;
		import serialisation.BitstreamReader;

		BitstreamSource bitstreamSource = new BitstreamSource();
		bitstreamSource.resetToArray(structure.payload);
		BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);

		bool successChained = true;
		Descriptor descriptor;

		deserialize(descriptor, successChained, bitstreamReader);
		if( !successChained ) {
			// TODO
		}




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

	protected enum EnumAgentState {
		INIT = 0, // must be zero
		NEEDCONNECT,
		CONNECTED,
		NEEDTOSENDAGENTIDENFICATION,
		WAITFORAGENTIDENTIFICATIONRESPONSE,
		SENDREGISTERSERVICES,
		STANDBY,
	}

	protected enum EnumAgentFsmSignal {
		INIT,
		INIT_TO_NEEDCONNECT,
		CONNECTED,
		NEEDTOSENDAGENTIDENFICATION,
		SENTAGENTIDENTIFICATION_WAITFORRESPONSE,
		REGISTERSERVICES,
		TO_STANDBY,
	}

	protected ControlFsmType controlFsm;

	// global queue with all requests
	protected RequestQueueType globalRequestQueue = new RequestQueueType();


	protected alias FiniteStateMachine!(EnumAgentState, EnumAgentFsmSignal) ControlFsmType;

	
	protected alias QueueNonConcurrentGcArray!RequestQueueElement RequestQueueType;
}


void main() {
	SoftComputingServiceAgent agent = new SoftComputingServiceAgent();

	for(;;) {
		agent.update();
	}
}

// TODO< queue filling and feedback >

// TODO< keeping track of context >

// TODO< use CGP, serialisation >

