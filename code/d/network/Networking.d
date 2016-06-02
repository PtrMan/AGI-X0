module network.Networking;

import std.socket;
import std.algorithm : min;
import std.stdio : writeln; // for debugging

import misc.BitstreamSource;
import misc.BitstreamDestination;
import distributed.GlobalStructs;
import serialisation.BitstreamReader;
import misc.GenericSerializer;
import network.AbstractNetworking;
import misc.TracingLogging;
import distributed.DistributedHelpers : composeMessageWithLengthPrefix;

// abstract the Hub/Agent details of the networking handling away
interface INetworkCallback {
	protected void networkCallbackRegisterServices(NetworkClient networkClient, ref RegisterServices structure);
	protected void networkCallbackAgentConnectToService(NetworkClient client, ref AgentConnectToService structure);
	protected void networkCallbackAgentCreateContext(NetworkClient client, ref AgentCreateContext structure);
	protected void networkCallbackAgentConnectToServiceResponse(NetworkClient client, ref AgentConnectToServiceResponse structure);

	protected void networkClientDisconnected(NetworkClient client);
}

class NetworkClient : AbstractNetworkClient {
	public final this(Socket socket) {
		super(socket);
	}

	// network
	public uint remainingBytesTillEndOfMessage = 0;
	public ubyte[] currentMessageWithBuffer; // gets filled with the complete message before it gets dispatched
}

import misc.TracingLogging;



class NetworkHost : AbstractNetworkHost!NetworkClient {
	public final this(INetworkCallback networkCallback, AbstractNetworkHost!NetworkClient.EnumRole role, Tracer tracer) {
		super(role);
		this.networkCallback = networkCallback;
		this.tracer = tracer;
	}

	public final void sendMessageToClient(NetworkClient client, BitstreamDestination payloadBitstream) {
		bool successChained;
		ubyte[] message = composeMessageWithLengthPrefix(payloadBitstream, successChained);

		if( !successChained ) {
			tracer.internalEvent("packet deserialisation failed", null, "UNKNOWS", 0, Tracer.EnumVerbose.YES);
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

	protected final override void clientDisconnected(NetworkClient client) {
		networkCallback.networkClientDisconnected(client);
	}

	// gets called if a message is completed and is ready to get parsed
	protected final void dispatchMessageFromClient(NetworkClient client) {

		void clientMessageAgentIdentificationHandshake(NetworkClient client, ref AgentIdentificationHandshake agentIdentificationHandshake) {
			//reportError(EnumErrorType.NONCRITICAL, "-verbose NetworkServer.clientMessageAgentIdentificationHandshake() called");

			// TODO< call into networkCallback >
		}

	 	void clientMessageRegisterServices(NetworkClient client, ref RegisterServices registerServices) {
			networkCallback.networkCallbackRegisterServices(client, registerServices);
		}

	 	void clientMessageAgentConnectToService(NetworkClient client, ref AgentConnectToService structure) {
			networkCallback.networkCallbackAgentConnectToService(client, structure);
		}

		void clientMessageAgentCreateContext(NetworkClient client, ref AgentCreateContext structure) {
			networkCallback.networkCallbackAgentCreateContext(client, structure);
		}

		void clientMessageAgentConnectToServiceResponse(NetworkClient client, ref AgentConnectToServiceResponse structure) {
			networkCallback.networkCallbackAgentConnectToServiceResponse(client, structure);
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
	      			tracer.internalEvent(\"received packet with unknown message type -> throw away\", null, \"UNKNOWN\", 0,  Tracer.EnumVerbose.YES);
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
				DispatchEntry("AGENTCONNECTTOSERVICERESPONSE", "AgentConnectToServiceResponse"),
				DispatchEntry("AGENTCREATECONTEXT", "AgentCreateContext"),
				]));


			if( !successChained ) {
				tracer.internalEvent("packet deserialisation failed", null, "UNKNOWS", 0, Tracer.EnumVerbose.YES);
			}
		}



		BitstreamSource bitstreamSourceOfMessage = new BitstreamSource();
		bitstreamSourceOfMessage.resetToArray(client.currentMessageWithBuffer);

		BitstreamReader!BitstreamSource bitstreamReaderOfMessage = new BitstreamReader!BitstreamSource(bitstreamSourceOfMessage);

		deserializeMessage(bitstreamReaderOfMessage);

		client.currentMessageWithBuffer.length = 0;
	}



	protected INetworkCallback networkCallback;
	protected Tracer tracer;
}
