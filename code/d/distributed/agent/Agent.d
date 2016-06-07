module distributed.agent.Agent;

import network.AbstractNetworking;
import network.Networking;
import distributed.GlobalStructs;
import misc.TracingLogging;
import misc.BitstreamDestination;
import misc.GenericSerializer;
import serialisation.BitstreamWriter;


interface IAgentCallbacks {
	protected void agentConnectToServiceResponse(AgentConnectToServiceResponse structure);
	protected void agentCreatedContext(ref AgentCreatedContext structure);

	protected void queueMessageWithFlowControl(ref QueueMessageWithFlowControl structure);
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

	public final void sendRegisterServices(RegisterServices structure) {
		BitstreamDestination bitstreamDestinationForPayload = new BitstreamDestination();
		BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForPayload);

		bool successChained = true;


		bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.REGISTERSERVICES, 16, successChained); // type of message
		
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

	protected final override void networkCallbackAgentCreatedContext(NetworkClient client, ref AgentCreatedContext structure) {
		tracer.internalEvent("called", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
		scope(exit) tracer.internalEvent("exit", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		agentCallbacks.agentCreatedContext(structure);
	}

	protected final override void networkCallbackAgentConnectToServiceResponse(NetworkClient client, ref AgentConnectToServiceResponse structure) {
		tracer.internalEvent("called", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
		scope(exit) tracer.internalEvent("exit", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		agentCallbacks.agentConnectToServiceResponse(structure);
	}

	final override protected void networkCallbackQueueMessageWithFlowControl(NetworkClient client, ref QueueMessageWithFlowControl structure) {
		tracer.internalEvent("called", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);
		scope(exit) tracer.internalEvent("exit", structure, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		agentCallbacks.queueMessageWithFlowControl(structure);
	}

	final override protected void networkClientDisconnected(NetworkClient client) {
		tracer.internalEvent("called, finishing...", null, __PRETTY_FUNCTION__, __LINE__, Tracer.EnumVerbose.YES);

		// TODO
	}

	protected NetworkHost networkHost;
	protected IAgentCallbacks agentCallbacks;
	protected Tracer tracer;
}
