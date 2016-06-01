import std.stdio; // TODO< just >
import std.format : format;

import std.socket;

import serialisation.BitstreamReader;
import misc.BitstreamSource;

import misc.ConvertBitstream;

abstract class AbstractNetworkClient {
	public final this(Socket socket) {
		this.socket = socket;
	}

	public Socket socket;

	public ubyte[] receivedQueue;
}

abstract class AbstractNetworkServer(ClientType : AbstractNetworkClient) {
	
	public final void iteration() {
		SocketSet checkReadSet = new SocketSet();
		SocketSet checkWriteSet = new SocketSet();
		SocketSet checkErrorSet = new SocketSet();

		checkReadSet.add(serverSocket);

		foreach( iterationClient; clients ) {
			checkReadSet.add(iterationClient.socket);
		}

		
		import core.time : dur, Duration;
		Duration timeout = dur!"msecs"(50);
		int selectResult = Socket.select(checkReadSet, checkWriteSet, checkErrorSet, timeout); 
		if( selectResult == 0 ) {
			// timeout, do nothing
		}
		else if( selectResult == -1 ) {
			// interruption, do nothing
		}
		else {
			int numberOfSocketsWithStatusChanges = selectResult;

			foreach( iStatusChange; 0..numberOfSocketsWithStatusChanges ) {
				if( checkReadSet.isSet(serverSocket) ) {
					Socket clientSocket = serverSocket.accept();

					assert(clientSocket.isAlive);
					ClientType createdClient = new ClientType(clientSocket);
					createdClient.socket.blocking = true;
					clients ~= createdClient;

					checkReadSet.remove(serverSocket);
				}

				foreach( iterationClient; clients ) {
					if( checkReadSet.isSet(iterationClient.socket) ) {
						receiveDataOfClient(iterationClient);

						checkReadSet.remove(iterationClient.socket);
					}
				}
			}
		}


		//acceptNewClients();
		//pollData();
	}

	public final void start(ushort port) {
		uint backlog = 2;

		sockaddr_in sa;
		sa.sin_family = AF_INET;
		sa.sin_addr.s_addr = INADDR_ANY;
		sa.sin_port = htons(port);

		serverSocket = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
		serverSocket.blocking = true;
		serverSocket.bind(new UnknownAddressReference(cast(sockaddr*)&sa, sa.sizeof));
		serverSocket.listen(backlog);
	}

	/*
	private final void acceptNewClients() {
		Socket clientSocket = serverSocket.accept();

		if( clientSocket.isAlive ) {
			ClientType createdClient = new ClientType(clientSocket);
			createdClient.socket.blocking = true;
			clients ~= createdClient;
		}
	}

	private final void pollData() {
		ubyte[4096] buffer;

		for( uint clientI = 0; clientI < clients.length; clientI++ ) {
			ClientType iterationClient = clients[clientI];

			ptrdiff_t receiveResult = iterationClient.socket.receive(buffer);
			if( receiveResult == -1 ) {
				// client hasn't received new data
				continue;
			}

			iterationClient.receivedQueue ~= buffer[0..receiveResult];

			clientReceivedNewData(iterationClient);

			import std.stdio;
			writeln("AbstractNetworkServer.pollData(), ", receiveResult);
		}
	}*/

	protected final void receiveDataOfClient(ClientType client) {
		ubyte[4096] buffer;

		ptrdiff_t receiveResult = client.socket.receive(buffer);
		if( receiveResult == -1 ) {
			// client hasn't received new data
			return;
		}

		client.receivedQueue ~= buffer[0..receiveResult];

		clientReceivedNewData(client);

		import std.stdio;
		writeln("AbstractNetworkServer.receiveDataOfClient(), received data from client=", receiveResult);
	}

	// callback if a client has received new data
	protected abstract void clientReceivedNewData(ClientType client);

	protected Socket serverSocket;
	protected ClientType[] clients;
}

class NetworkClient : AbstractNetworkClient {
	public final this(Socket socket) {
		super(socket);
	}

	// network
	public uint remainingBytesTillEndOfMessage = 0;
	public ubyte[] currentMessageWithBuffer; // gets filled with the complete message before it gets dispatched
}


import misc.BitstreamSource;
import misc.BitstreamDestination;
import distributed.GlobalStructs;
import serialisation.BitstreamReader;
import serialisation.BitstreamWriter;
import misc.GenericSerializer;


import std.algorithm : min;

class NetworkServer : AbstractNetworkServer!NetworkClient {
	public final this(Hub hub) {
		this.hub = hub;
	}

	// used by the hub
	public final void sendMessageToClient(NetworkClient client, BitstreamDestination payloadBitstream) {
		import distributed.DistributedHelpers : composeMessageWithLengthPrefix;

		bool successChained;
		ubyte[] message = composeMessageWithLengthPrefix(payloadBitstream, successChained);

		if( !successChained ) {
			reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.sendMessageToClient() " ~ "serialisation failed!");
		}

		writeln("bitstreamDestinationForMessage.dataAsUbyte length=", message.length);

		long sentNumber = client.socket.send(cast(const void[])message, cast(SocketFlags)0);

		// TODO< check sentNumber >
	}

	protected final override void clientReceivedNewData(NetworkClient client) {
		while( client.receivedQueue.length > 0 ) {
			// if this is zero it means we wait for at least the length of the next message, which gets stored in the first two byte in the receivedQueue
			if( client.remainingBytesTillEndOfMessage == 0 ) {
				if( client.receivedQueue.length >= 2 ) {
					ubyte[] newRemainingBytesTillEndOfMessageBuffer = client.receivedQueue[0..2];

					client.receivedQueue = client.receivedQueue[2..$];

					// TODO< convert little/big endian by machine type >
					uint newRemainingBytesTillEndOfMessage = (cast(uint)newRemainingBytesTillEndOfMessageBuffer[1] << 8) | cast(uint)newRemainingBytesTillEndOfMessageBuffer[0];
					
					writeln("newRemainingBytesTillEndOfMessage=", newRemainingBytesTillEndOfMessage);

					client.remainingBytesTillEndOfMessage = newRemainingBytesTillEndOfMessage;
				}
			}


			if( client.remainingBytesTillEndOfMessage > 0 ) {
				uint numberOfTransferedBytes = min(client.remainingBytesTillEndOfMessage, client.receivedQueue.length);

				client.currentMessageWithBuffer ~= client.receivedQueue[0..numberOfTransferedBytes];
				client.receivedQueue = client.receivedQueue[numberOfTransferedBytes..$];

				client.remainingBytesTillEndOfMessage -= numberOfTransferedBytes;

				if( client.remainingBytesTillEndOfMessage == 0 ) {
					// message completed, dispatch
					dispatchMessageFromClient(client);
				}
			}
		}

		writeln("NetworkServer.clientReceivedNewData() exit");
	}

	// gets called if a message is completed and is ready to get parsed
	protected final void dispatchMessageFromClient(NetworkClient client) {

		void clientMessageAgentIdentificationHandshake(NetworkClient client, ref AgentIdentificationHandshake agentIdentificationHandshake) {
			reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.clientMessageAgentIdentificationHandshake() called");

			// TODO< call into hub >
		}

	 	void clientMessageRegisterServices(NetworkClient client, ref RegisterServices registerServices) {
			hub.networkCallbackRegisterServices(client, registerServices);
		}

	 	void clientMessageAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
			hub.networkCallbackAgentConnectToService(client, structure);
		}

		void clientMessageAgentCreateContext(NetworkClient client, ref AgentCreateContext structure) {
			hub.networkCallbackAgentCreateContext(client, structure);
		}


		void deserializeMessage(BitstreamReader!BitstreamSource bitstreamReaderOfMessage) {
			// compiletime
			struct DispatchEntry {
				public final this(string enumName, string structureName) {
					this.enumName = enumName;
					this.structureName = structureName;
				}

				string enumName;
				string structureName;
			}

			// compiletime
			string generateDOfMessageDispatch(DispatchEntry[] dispatchTable) {
				string result;

				import std.format : format;


				result ~= format("""
						if( messageType == cast(uint)EnumMessageType.%s ) {
							%s structure;
							deserialize(structure, successChained, bitstreamReaderOfMessage);

							if( !successChained ) {
				      			return;
				      		}

				      		clientMessage%s(client, structure);
				      	}
						""", dispatchTable[0].enumName, dispatchTable[0].structureName, dispatchTable[0].structureName);

				foreach( dispatchEntry; dispatchTable[1..$] ) {
					result ~= format("""
						else if( messageType == cast(uint)EnumMessageType.%s ) {
							%s structure;
							deserialize(structure, successChained, bitstreamReaderOfMessage);

							if( !successChained ) {
				      			return;
				      		}

				      		clientMessage%s(client, structure);
				      	}
						""", dispatchEntry.enumName, dispatchEntry.structureName, dispatchEntry.structureName);
	      		}

	      		result ~= """
	      		else {
					reportError(EnumErrorType.NONCRITICAL, \"-verbose NetworkServer.dispatchMessageFromClient() received packet with unknown message type -> throw away\");
				}
	      		""";

	      		return result;
			}

			// compiletime
			template generateDOfMessageDispatchTemplate(DispatchEntry[] dispatchTable) {
				const char[] generateDOfMessageDispatch = generateDOfMessageDispatch(dispatchTable);
			}



			bool successChained = true;
			uint messageType = bitstreamReaderOfMessage.getUint__n(16, successChained);

			mixin(generateDOfMessageDispatch(
				[
				DispatchEntry("AGENTIDENTIFICATIONHANDSHAKE", "AgentIdentificationHandshake"),
				DispatchEntry("REGISTERSERVICES", "RegisterServices"),
				DispatchEntry("AGENTCONNECTTOSERVICE", "AgentConnectToService"),
				DispatchEntry("AGENTCREATECONTEXT", "AgentCreateContext"),
				]));


			if( !successChained ) {
				reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.dispatchMessageFromClient() packet deserialisation failed");
			}
		}



		BitstreamSource bitstreamSourceOfMessage = new BitstreamSource();
		bitstreamSourceOfMessage.resetToArray(client.currentMessageWithBuffer);

		BitstreamReader!BitstreamSource bitstreamReaderOfMessage = new BitstreamReader!BitstreamSource(bitstreamSourceOfMessage);

		deserializeMessage(bitstreamReaderOfMessage);

		writeln("NetworkServer.dispatchMessageFromClient()");

		client.currentMessageWithBuffer.length = 0;
	}



	protected Hub hub;
}










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



/*
class RegisteredService {
	public final this(NetworkClient owningClientOfAgent, string serviceName, uint serviceVersion) {
		this.owningClientOfAgent = owningClientOfAgent;
		this.serviceName = serviceName;
		this.serviceVersion = serviceVersion;
	}

	public string serviceName; // used for identification
	public uint serviceVersion;

	
}*/




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

	alias FiniteStateMachine!(EnumAgentServiceContextState, EnumAgentServiceContextFsmSignal) ServiceStateFsmType;
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


enum EnumErrorType {
	NONCRITICAL
}

void reportError(EnumErrorType errorType, string message) {
	writeln("[ERROR] noncritical: ", message);
}

void report(string prefix, string message) {
	writeln("[", prefix, "] ", message);
}

class Hub {
	public final this() {
		networkServer = new NetworkServer(this);
	}
	

	

	protected final void reportHub(string message) {
		
		report("hub", message);
	}

	public final void startServer(ushort port) {
		networkServer.start(port);
		reportHub("server started, waiting");
	}

	public final void networkMainloopIteration() {
		networkServer.iteration();
	}

	// called by NetworkServer
	public final void networkCallbackRegisterServices(NetworkClient networkClient, ref RegisterServices registerServices) {
		internalEvent("called with...", registerServices, "UNKNOWN", 0, EnumVerbose.YES);

		foreach( iterationServiceDescriptor; registerServices.service ) {
			internalEvent(format("... locator.name=%s, locator.version=%s", iterationServiceDescriptor.locator.name, iterationServiceDescriptor.locator.version_), registerServices, "UNKNOWN", 0, EnumVerbose.YES);
		}

		// TODO< checks for blocking >
		// TODO< check for flooding >


		foreach( iterationServiceDescriptor; registerServices.service ) {
			serviceRegister.registerService(
				iterationServiceDescriptor.locator.name,
				iterationServiceDescriptor.locator.version_,
				networkClient,
				iterationServiceDescriptor.capabilities,
				iterationServiceDescriptor.contract
			);
		}
	}

	public final void networkCallbackAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
		internalEvent(format("called with servicename=%s, acceptedVersions=%d, serviceVersionsAndUp=%s", structure.serviceName, structure.acceptedVersions, structure.serviceVersionsAndUp), structure, "UNKNOWN", 0, EnumVerbose.YES);

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

		internalEvent(format("connectSuccess=%s, providedVersions=%s", connectSuccess, providedVersions), structure, "UNKNOWN", 0, EnumVerbose.YES);


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
					internalEvent("serialisation failed!", structure, "UNKNOWN", 0, EnumVerbose.YES);
				}
			}


			networkServer.sendMessageToClient(client, bitstreamDestinationForPayload);
		}
	}

	public final void networkCallbackAgentCreateContext(NetworkClient client, ref AgentCreateContext structure) {
		internalEvent(format("called with locator.name=%s, locator.version=%s, requestId=%s", structure.locator.name, structure.locator.version_, structure.requestId), structure, "UNKNOWN", 0, EnumVerbose.YES);

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
				internalEvent("serialisation failed!", structure, "UNKNOWN", 0, EnumVerbose.YES);
			}

			networkServer.sendMessageToClient(serviceAgentRelation.owningClientOfAgent, bitstreamDestinationForPayload);
		}

		void sendResponseToClient() {
			BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
			BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

			bool successChained = true;

			bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.AGENTCREATECONTEXTRESPONSE, 16, successChained); // type of message
			
			serialize(agentCreateContextResponse, successChained, bitstreamWriterForPayload);

			if( !successChained ) {
				internalEvent("serialisation failed!", structure, "UNKNOWN", 0, EnumVerbose.YES);
			}

			networkServer.sendMessageToClient(client, bitstreamDestinationForPayload);
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
						internalEvent("Service agent context creation failed, because client wasn't found", structure, "UNKNOWN", 0);
						return;
					}

					import std.random : uniform, Random, unpredictableSeed;
					Random gen = Random(unpredictableSeed);
					agentCreateContextResponse.agentServiceContextId = uniform!"[)"(0, uint32_t.max, gen);

					serviceAgentContextRelation.openContexts ~= new AgentServiceContext(agentCreateContextResponse.agentServiceContextId);

					success = true;

					internalEvent("Service agent context was successfully created", structure, "UNKNOWN", 0);
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

				internalEvent("Service was found but version didn't match!", structure, "UNKNOWN", 0);
			}
		}



		sendResponseToClient();
	}

	enum EnumVerbose : bool {
		NO,
		YES,
	}

	// sends an internal event to an eventstore or logs it or whatever
	protected final void internalEvent(PayloadType)(string humanreadableDescription, PayloadType payload, string sourceFunction, uint sourceLine, EnumVerbose verbose = EnumVerbose.NO) {
		// TODO< send to eventstore if the configuration is set this way >

		if( verbose == EnumVerbose.YES ) {
			import std.format : format;
			reportError(EnumErrorType.NONCRITICAL, format("-verbose %s line %s : %s", sourceFunction, sourceLine, humanreadableDescription));
		}
	}

	protected ServiceRegister serviceRegister = new ServiceRegister();

	protected NetworkServer networkServer;

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
