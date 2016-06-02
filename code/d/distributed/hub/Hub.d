import std.stdio; // TODO< just >
import std.format : format;


import serialisation.BitstreamWriter;
import serialisation.BitstreamReader;
import misc.BitstreamSource;
import misc.BitstreamDestination;

import misc.ConvertBitstream;

import distributed.GlobalStructs;
import network.Networking;
import network.AbstractNetworking;
import misc.GenericSerializer;







// a Agent can have any number of services, the same services can be provided by different agents, versions can overlap

// Agents can connect to Services provided by the same agent or other agents.
// The hub can force a disconnection (for some reason).

// Used services of agents can be relocated by the hub.

// state diagram of an service and the agent relationship
// ------------------------------------------------------

//    +--------------------------------------------------------------------------------------------------------+
//    |                                               disconnected                                             |
//    +--------------------------------------------------------------------------------------------------------+
//         |                               /\                                        /\                                           /\
//         |                               |                                         |                                            |
//   AGENT CONNECT TO SERVICE ----> AGENT CONNECT TO SERVICE RESPONSE       TODO: AGENT DISCONNECT FROM SERVICE                TODO:  AGENT FORCE DISCONNECT FROM SERVICE
//   agent->hub                      hub->agent                                  agent->hub                                      hub->agent
//                                         |                                          |                                            |
//                                         V                                          |                                            |
//    +--------------------------------------------------------------------------------------------------------+
//    |                                                connected                                               |
//    +--------------------------------------------------------------------------------------------------------+
//         |                                              /\
//         |                                              |
//    TODO:  AGENT SERVICE RELOCATE PENDING    TODO: AGENT SERVICE RELOCATED
//    hub->agent                                      hub->agent
//         |                                              |
//         V                                              |
//    +--------------------------------------------------------------------------------------------------------+
//    |                  relocation pending  [connection to Agent can get changed in the meantime]             |
//    +--------------------------------------------------------------------------------------------------------+


// relocation
// ==========

// Only the hub can activly request that an agent connected to an service connects to an other service (possibly of an other agent).





// TODO< state diagram for context of an service which is used by an agent >

// state diagram for the context of an service which is used by an agent

// +--------------------------------------------------------------------------------------------------------------+
// |       (not created)                                                                                          |
// +--------------------------------------------------------------------------------------------------------------+
//     /\                               /\                                           /\                         /\                              
//     |                                 |                                           |                          |                              
//  TODO: AGENT CREATE CONTEXT   TODO: AGENT CREATE CONTEXT RESPONSE         TODO: CONTEXT DISPOSE              |                                                    
//  agent->hub            --------->  hub->agent                                  hub->agent                    |                                         
//                                       |                                        agent->hub                    |                                                  
//                                       |                                           |                          |                              
//                                       | AGENT CREATED CONTEXT                     |                          |                                                
//                                       | hub->agent(who provides the service)      |                          |                                                                                    
//                                       |                                           |                          |                                                
//                                      \/                                           |                          |         
// +-----------------------------------------------------------------------------------+                        |               
// |  USABLE                                                                           |                        |                                   
// +-----------------------------------------------------------------------------------+                        |                                   
//      |                                               /\                                                      |                
//      \/                                               |                                                      |                  
//  TODO: CONTEXT RELOCATION REQUEST           TODO: CONTEXT RELOCATION SUCCESS                  TODO: CONTEXT RELOCATION DENIED                              
//  hub->agent                                         hub->agent                                           hub->agent
//  agent->hub                                       agent->hub?                                                |                      
//      |                                                |                                                      |                
//      \/                                               |                                                      |
// +--------------------------------------------------------------------------------------------------------------+
// |  RELOCATION PENDING                                                                                          |
// +--------------------------------------------------------------------------------------------------------------+

// 


// TODO< implement this >
// in the usable state a context can be used with

// types can be
//  REQUEST    agent->hub->agent   for requesting something, receiver agent doesn't have to answer it, depending on Service/Agent specific protocol
//  RESPONSE   agent->hub->agent   for responding to a request
//  PUSH       agent->hub->agent   for transfering some data

// messages get enqueued into a Agent queue, for this there are two types of 
// EnumQueueAction
//  ENQUEUE    request for the enquing of a message
//  ENQUEUED   response which gives information if the message got enqueued or not


// if the hub is in tracing mode for the context the agent must send a decorated decoded payload besides the usual payload.
// the decoded payload then can be sent to an eventstore by the hub



import misc.FiniteStateMachine;

import std.stdint;

// The state of the service used by an agent
class AgentServiceContext {
	enum EnumAgentServiceContextState {
		NOTCREATED = 0, // only for FSM, must be zero!
		USABLE,
		RELOCATIONPENDING,
	}

	enum EnumAgentServiceContextFsmSignal {
		NOTCREATED_TO_USABLE // just for init
	}

	public final this(uint32_t agentServiceContextId) {
		this.agentServiceContextId = agentServiceContextId;
		initFsm();
	}

	protected final void initFsm() {
		ServiceStateFsmType.StateTransitions[EnumAgentServiceContextState] transitionTable;
		transitionTable[EnumAgentServiceContextState.NOTCREATED].next[EnumAgentServiceContextFsmSignal.NOTCREATED_TO_USABLE] = EnumAgentServiceContextState.USABLE;

		serviceStateFsm = ServiceStateFsmType.make(transitionTable);
		serviceStateFsm.signal(EnumAgentServiceContextFsmSignal.NOTCREATED_TO_USABLE); // init to USABLE
	}

	ServiceStateFsmType serviceStateFsm;

	uint32_t agentServiceContextId; // unique id of the context-service-agent connection

	//EnumAgentServiceContextState serviceState = EnumAgentServiceContextState.NOTCREATED;

	protected alias FiniteStateMachine!(EnumAgentServiceContextState, EnumAgentServiceContextFsmSignal) ServiceStateFsmType;
}

// context of a client/Agent which uses a service
class ServiceAgentContextRelation {
	protected final this() {
	}

	public final this(NetworkClient networkClient) {
		this.networkClient = networkClient;
	}

	NetworkClient networkClient;

	AgentServiceContext[] openContexts;
}


// holds information about the Agent client which provides/owns the service and its user Agent clients
class ServiceAgentRelation {
	static class LookupException : Exception {
    	public final this () {
        	super("");
    	}
	}



	protected final this() {
	}

	public final this(NetworkClient owningClientOfAgent, ServiceCapabilities capabilities, ContractInformation contract) {
		this.owningClientOfAgent = owningClientOfAgent;
		this.capabilities = capabilities;
		this.contract = contract;
	}

	public final ServiceAgentContextRelation findServiceAgentContextRelationByClient(NetworkClient networkClient) {
		foreach( iterationServiceAgentContextRelation; serviceAgentContextRelations ) {
			if( iterationServiceAgentContextRelation.networkClient is networkClient ) {
				return iterationServiceAgentContextRelation;
			}
		}

		throw new LookupException();
	}

	NetworkClient owningClientOfAgent; // which client does hold the service

	ServiceCapabilities capabilities;
	ContractInformation contract;

	ServiceAgentContextRelation[] serviceAgentContextRelations; // which contexts are open for the Agent and in which state are they?
}


class Service {
	public final uint[] calcSortedProvidedVersions() {
		import std.algorithm.sorting : sort;
		import std.array : array;

		return array(sort(serviceAgentRelationsByVersion.keys));
	}

	public ServiceAgentRelation[][uint] serviceAgentRelationsByVersion;
}

// ServiceRegister
class ServiceRegister {
	public final void registerService(string servicenName, uint serviceVersion, NetworkClient owningClientOfAgent, ServiceCapabilities capabilities, ContractInformation contract) {
		Service *servicePtr = servicenName in registeredServices;
		if( servicePtr !is null ) {
			addServiceProviderToService(*servicePtr, serviceVersion, owningClientOfAgent, capabilities, contract);
		}
		else {
			addNewServiceProvider(servicenName, serviceVersion, owningClientOfAgent, capabilities, contract);
		}
	}

	protected final void addNewServiceProvider(string servicenName, uint serviceVersion, NetworkClient owningClientOfAgent, ServiceCapabilities capabilities, ContractInformation contract) {
		Service createdService = new Service();
		createdService.serviceAgentRelationsByVersion[serviceVersion] = [new ServiceAgentRelation(owningClientOfAgent, capabilities, contract)];
		registeredServices[servicenName] = createdService;
	}

	protected static void addServiceProviderToService(Service destinationService, uint serviceVersion, NetworkClient owningClientOfAgent, ServiceCapabilities capabilities, ContractInformation contract) {
		ServiceAgentRelation[] *ptr = serviceVersion in destinationService.serviceAgentRelationsByVersion;
		if( ptr !is null ) {
			*ptr ~= new ServiceAgentRelation(owningClientOfAgent, capabilities, contract);
		}
		else {
			destinationService.serviceAgentRelationsByVersion[serviceVersion] = [new ServiceAgentRelation(owningClientOfAgent, capabilities, contract)];
		}
	}

	public final bool isServiceProvided(string serviceName) {
		return (serviceName in registeredServices) !is null;
	}

	public final Service lookup(string serviceName) {
		Service *servicePtr = serviceName in registeredServices;
		if( servicePtr !is null ) {
			return *servicePtr;
		}
		else {
			// TODO< throw error >
			return null;
		}
	}

	protected Service[string] registeredServices;
}



import misc.TracingLogging;

class Hub : INetworkCallback, IReport {
	public final this() {
		tracer = new Tracer(this);
		networkHost = new NetworkHost(this, AbstractNetworkHost!NetworkClient.EnumRole.SERVER, tracer);
	}
	


	protected final void reportHub(string message) {
		
		report("hub", message);
	}

	public final void startServer(ushort port) {
		networkHost.startServer(port);
		reportHub("server started, waiting");
	}

	public final void networkMainloopIteration() {
		networkHost.iteration();
	}

	//////////////////////////////
	// implement interface INetworkCallback
	
	final override protected void networkCallbackRegisterServices(NetworkClient networkClient, ref RegisterServices structure) {
		tracer.internalEvent("called with...", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		foreach( iterationServiceDescriptor; structure.service ) {
			tracer.internalEvent(format("... locator.name=%s, locator.version=%s", iterationServiceDescriptor.locator.name, iterationServiceDescriptor.locator.version_), structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
		}

		scope(exit) tracer.internalEvent("exit", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		// TODO< checks for blocking >
		// TODO< check for flooding >


		foreach( iterationServiceDescriptor; structure.service ) {
			serviceRegister.registerService(
				iterationServiceDescriptor.locator.name,
				iterationServiceDescriptor.locator.version_,
				networkClient,
				iterationServiceDescriptor.capabilities,
				iterationServiceDescriptor.contract
			);
		}
	}

	final override protected void networkCallbackAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
		tracer.internalEvent(format("called with servicename=%s, acceptedVersions=%s, serviceVersionsAndUp=%s", structure.serviceName, structure.acceptedVersions, structure.serviceVersionsAndUp), structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		// TODO< checks for blocking >
		// TODO< check for flooding >

		bool connectSuccess = false;
		uint[] providedVersions;

		bool serviceProvided = serviceRegister.isServiceProvided(structure.serviceName);
		if( serviceProvided ) {
			Service service = serviceRegister.lookup(structure.serviceName);

			providedVersions = service.calcSortedProvidedVersions();

			// check version
			// look for service, check version and connect if it all matches and is found
			// we reverse to try to connect to the highest provided version
			foreach( uint iterationVersion; providedVersions.reverse ) {
				bool isVersionAccepted(uint version_) {
					if( structure.serviceVersionsAndUp ) {
						import std.algorithm.searching : any;

						bool isVersionGreater(uint iterationVersion) {
							return iterationVersion >= version_;
						}

						return any!isVersionGreater(structure.acceptedVersions);
					}
					else {
						import std.algorithm: canFind;
						return structure.acceptedVersions.canFind(version_);
					}
				}

				bool versionMatches = isVersionAccepted(iterationVersion);
				if( versionMatches ) {
					void connect(uint version_) {
						// TODO< selection strategy / load balancing >
						service.serviceAgentRelationsByVersion[version_][$].serviceAgentContextRelations ~= new ServiceAgentContextRelation(client);
						connectSuccess = true;
					}

					connect(iterationVersion);

					break;
				}
			}
		}

		tracer.internalEvent(format("connectSuccess=%s, providedVersions=%s", connectSuccess, providedVersions), structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);


		// send response back
		{
			BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
			BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

			bool successChained = true;


			bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTCONNECTTOSERVICERESPONSE, 16, successChained); // type of message
			
			{
				AgentConnectToServiceResponse agentConnectToServiceResponse;
				agentConnectToServiceResponse.serviceName = structure.serviceName;
				agentConnectToServiceResponse.providedVersions = providedVersions;
				agentConnectToServiceResponse.connectSuccess = connectSuccess;

				serialize(agentConnectToServiceResponse, successChained, bitstreamWriterForPayload);

				if( !successChained ) {
					tracer.internalEvent("serialisation failed!", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
				}
			}


			networkHost.sendMessageToClient(client, bitstreamDestinationForPayload);
		}
	}

	final override protected void networkCallbackAgentCreateContext(NetworkClient client, ref AgentCreateContext structure) {
		tracer.internalEvent(format("called with locator.name=%s, locator.version=%s, requestId=%s", structure.locator.name, structure.locator.version_, structure.requestId), structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		AgentCreateContextResponse agentCreateContextResponse;

		void sendMessageAgentCreatedContextToOwningAgent(uint32_t agentServiceContextId, ServiceAgentRelation serviceAgentRelation) {
			AgentCreatedContext message;
			message.agentServiceContextId = agentServiceContextId;

			BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
			BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

			bool successChained = true;

			bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTCREATEDCONTEXT, 16, successChained); // type of message
			
			serialize(message, successChained, bitstreamWriterForPayload);

			if( !successChained ) {
				tracer.internalEvent("serialisation failed!", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
			}

			networkHost.sendMessageToClient(serviceAgentRelation.owningClientOfAgent, bitstreamDestinationForPayload);
		}

		void sendResponseToClient() {
			BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
			BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

			bool successChained = true;

			bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTCREATECONTEXTRESPONSE, 16, successChained); // type of message
			
			serialize(agentCreateContextResponse, successChained, bitstreamWriterForPayload);

			if( !successChained ) {
				tracer.internalEvent("serialisation failed!", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
			}

			networkHost.sendMessageToClient(client, bitstreamDestinationForPayload);
		}


		
		agentCreateContextResponse.responseType = EnumAgentCreateContextResponseType.SERVICENOTFOUND;
		agentCreateContextResponse.humanReadableError = "Service was not found";

		agentCreateContextResponse.requestId = structure.requestId;

		// TODO< checks for blocking >
		// TODO< check for flooding >

		bool serviceProvided = serviceRegister.isServiceProvided(structure.locator.name);
		if( serviceProvided ) {
			Service service = serviceRegister.lookup(structure.locator.name);

			uint[] providedVersions = service.calcSortedProvidedVersions();

			import std.algorithm: canFind;
			bool versionFound = providedVersions.canFind(structure.locator.version_);
			if( versionFound ) {
				// TODO< selection strategy / load balancing >
				ServiceAgentRelation serviceAgentRelation = service.serviceAgentRelationsByVersion[structure.locator.version_][$];

				void locateServiceAgentContextRelationAndAddNewContextToIt(out bool success) {
					success = false;

					ServiceAgentContextRelation serviceAgentContextRelation;
					try {
						serviceAgentContextRelation = serviceAgentRelation.findServiceAgentContextRelationByClient(client);
					}
					catch( ServiceAgentRelation.LookupException exception ) {
						tracer.internalEvent("Service agent context creation failed, because client wasn't found", structure, __PRETTY_FUNCTION__, __LINE__);
						return;
					}

					import std.random : uniform, Random, unpredictableSeed;
					Random gen = Random(unpredictableSeed);
					agentCreateContextResponse.agentServiceContextId = uniform!"[)"(0, uint32_t.max, gen);

					serviceAgentContextRelation.openContexts ~= new AgentServiceContext(agentCreateContextResponse.agentServiceContextId);

					success = true;

					tracer.internalEvent("Service agent context was successfully created", structure, __PRETTY_FUNCTION__, __LINE__);
				}

				bool calleeSuccess;
				locateServiceAgentContextRelationAndAddNewContextToIt(calleeSuccess);
				if( calleeSuccess ) {
					agentCreateContextResponse.responseType = EnumAgentCreateContextResponseType.SUCCESS;
					agentCreateContextResponse.humanReadableError = "";
				}

				if( calleeSuccess ) {
					sendMessageAgentCreatedContextToOwningAgent(agentCreateContextResponse.agentServiceContextId, serviceAgentRelation);
				}
			}
			else {
				agentCreateContextResponse.responseType = EnumAgentCreateContextResponseType.SERVICEFOUNDBUTWRONGVERSION;
				agentCreateContextResponse.humanReadableError = "Service was found but version didn't match!";

				tracer.internalEvent("Service was found but version didn't match!", structure, __PRETTY_FUNCTION__, __LINE__);
			}
		}



		sendResponseToClient();
	}

	final override protected void networkCallbackAgentConnectToServiceResponse(NetworkClient client, ref AgentConnectToServiceResponse structure) {
		tracer.internalEvent("called, ignored because in role \"Hub\"", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
	}

	final override protected void networkClientDisconnected(NetworkClient client) {
		tracer.internalEvent("called, updating...", null, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

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

	

	protected ServiceRegister serviceRegister = new ServiceRegister();
	protected Tracer tracer;

	protected NetworkHost networkHost;
}








void main() {
	ushort port = 1555;

	Hub hub = new Hub();
	hub.startServer(port);

	for(;;) {
		hub.networkMainloopIteration();
	}

	
	/*


			client.shutdown(SocketShutdown.BOTH);

	*/
}
