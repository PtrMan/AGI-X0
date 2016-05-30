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
		acceptNewClients();
		pollData();
	}

	public final void start(ushort port) {
		uint backlog = 2;

		sockaddr_in sa;
		sa.sin_family = AF_INET;
		sa.sin_addr.s_addr = INADDR_ANY;
		sa.sin_port = htons(port);

		serverSocket = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
		serverSocket.blocking = false;
		serverSocket.bind(new UnknownAddressReference(cast(sockaddr*)&sa, sa.sizeof));
		serverSocket.listen(backlog);
	}

	private final void acceptNewClients() {
		Socket clientSocket = serverSocket.accept();

		if( clientSocket.isAlive ) {
			ClientType createdClient = new ClientType(clientSocket);
			createdClient.socket.blocking = false;
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
		void deserializeMessage(BitstreamReader!BitstreamSource bitstreamReaderOfMessage) {
			bool successChained = true;
			uint messageType = bitstreamReaderOfMessage.getUint__n(16, successChained);

			if( messageType == cast(uint)EnumMessageType.AGENTIDENTIFICATIONHANDSHAKE ) {
				AgentIdentificationHandshake structure;
	      		GenericDeserializer!(AgentIdentificationHandshake, BitstreamSource).deserialize(structure, successChained, bitstreamReaderOfMessage);
	      		
	      		if( !successChained ) {
	      			return;
	      		}

	      		clientMessageAgentIdentificationHandshake(client, structure);
			}
			else if( messageType == cast(uint)EnumMessageType.REGISTERSERVICE ) {
				RegisterService structure;
				GenericDeserializer!(RegisterService, BitstreamSource).deserialize(structure, successChained, bitstreamReaderOfMessage);
	      		
	      		if( !successChained ) {
	      			return;
	      		}

	      		clientMessageRegisterService(client, structure);

			}
			else if( messageType == cast(uint)EnumMessageType.AGENTCONNECTTOSERVICE ) {
				AgentConnectToService structure;
				GenericDeserializer!(AgentConnectToService, BitstreamSource).deserialize(structure, successChained, bitstreamReaderOfMessage);

				if( !successChained ) {
	      			return;
	      		}

	      		clientMessageAgentConnectToService(client, structure);
			}
			else {
				reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.dispatchMessageFromClient() received packet with unknown message type -> throw away");
			}

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

	protected final void clientMessageAgentIdentificationHandshake(NetworkClient client, ref AgentIdentificationHandshake agentIdentificationHandshake) {
		reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.clientMessageAgentIdentificationHandshake() called");

		// TODO< call into hub >
	}

	protected final void clientMessageRegisterService(NetworkClient client, ref RegisterService registerService) {
		hub.networkCallbackRegisterService(client, registerService.serviceName, registerService.serviceVersion);
	}

	protected final void clientMessageAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
		hub.networkCallbackAgentConnectToService(client, structure);
	}


	protected Hub hub;
}










// a Agent can have any number of services, the same services can be provided by different agents, versions can overlap

// Agents can connect to Services provided by the same agent or other agents.
// The hub can force a disconnection (for some reason).

// Used services of agents can be relocated by the hub.

// state diagram of an service and the agent relationship


//    +--------------------------------------------------------------------------------------------------------+
//    |                                               disconnected                                             |
//    +--------------------------------------------------------------------------------------------------------+
//         |                               /\                                        /\                                           /\
//         |                               |                                         |                                            |
//   AGENT CONNECT TO SERVICE ----> AGENT CONNECT TO SERVICE RESPONSE        AGENT DISCONNECT FROM SERVICE                  AGENT FORCE DISCONNECT FROM SERVICE
//   agent->hub                      hub->agent                                  agent->hub                                      hub->agent
//                                         |                                          |                                            |
//                                         V                                          |                                            |
//    +--------------------------------------------------------------------------------------------------------+
//    |                                                connected                                               |
//    +--------------------------------------------------------------------------------------------------------+
//         |                                              /\
//         |                                              |
//    AGENT SERVICE RELOCATE PENDING                 AGENT SERVICE RELOCATED
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


// holds information about the Agent client which provides/owns the service and its user Agent clients
class ServiceAgentRelation {
	protected final this() {
	}

	public final this(NetworkClient owningClientOfAgent) {
		this.owningClientOfAgent = owningClientOfAgent;
	}

	NetworkClient owningClientOfAgent; // which client does hold the service

	NetworkClient[] connectedAgents; // which agent clients are connected to the service?
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
	public final void registerService(string servicenName, uint serviceVersion, NetworkClient owningClientOfAgent) {
		Service *servicePtr = servicenName in registeredServices;
		if( servicePtr !is null ) {
			addServiceProviderToService(*servicePtr, serviceVersion, owningClientOfAgent);
		}
		else {
			addNewServiceProvider(servicenName, serviceVersion, owningClientOfAgent);
		}
	}

	protected final void addNewServiceProvider(string servicenName, uint serviceVersion, NetworkClient owningClientOfAgent) {
		Service createdService = new Service();
		createdService.serviceAgentRelationsByVersion[serviceVersion] = [new ServiceAgentRelation(owningClientOfAgent)];
		registeredServices[servicenName] = createdService;
	}

	protected static void addServiceProviderToService(Service destinationService, uint serviceVersion, NetworkClient owningClientOfAgent) {
		ServiceAgentRelation[] *ptr = serviceVersion in destinationService.serviceAgentRelationsByVersion;
		if( ptr !is null ) {
			*ptr ~= new ServiceAgentRelation(owningClientOfAgent);
		}
		else {
			destinationService.serviceAgentRelationsByVersion[serviceVersion] = [new ServiceAgentRelation(owningClientOfAgent)];
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
	public final void networkCallbackRegisterService(NetworkClient networkClient, string serviceName, uint serviceVersion) {
		reportError(EnumErrorType.NONCRITICAL, "-verbose Hub.networkCallbackRegisterService() called with " ~ format("servicename=%s, serviceVersion=%d", serviceName, serviceVersion));

		serviceRegister.registerService(serviceName, serviceVersion, networkClient);
	}

	public final void networkCallbackAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
		reportError(EnumErrorType.NONCRITICAL, "-verbose Hub.networkCallbackAgentConnectToService() called with " ~ format("servicename=%s, acceptedVersions=%d, serviceVersionsAndUp=%s", structure.serviceName, structure.acceptedVersions, structure.serviceVersionsAndUp));


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
						service.serviceAgentRelationsByVersion[version_][$].connectedAgents ~= client;
						connectSuccess = true;
					}

					connect(iterationVersion);

					break;
				}
			}
		}


		reportError(EnumErrorType.NONCRITICAL, "-verbose Hub.networkCallbackAgentConnectToService() " ~ format("connectSuccess=%s, providedVersions=%s", connectSuccess, providedVersions));



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

				GenericSerializer!(AgentConnectToServiceResponse, BitstreamDestination).serialize(agentConnectToServiceResponse, successChained, bitstreamWriterForPayload);

				if( !successChained ) {
					reportError(EnumErrorType.NONCRITICAL, "-verbose Hub.networkCallbackAgentConnectToService() " ~ "serialisation failed!");
				}
			}


			networkServer.sendMessageToClient(client, bitstreamDestinationForPayload);
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

		// TODO< sleep >
	}

	
	/*


			client.shutdown(SocketShutdown.BOTH);

	*/
}
