import std.stdio; // TODO< just >
import std.format : format;

import std.socket;

import serialisation.BitstreamReader;
import misc.BitstreamSource;

import misc.ConvertBitstream;

enum EnumErrorType {
	NONCRITICAL
}

void reportError(EnumErrorType errorType, string message) {
	writeln("[ERROR] noncritical: ", message);
}

void report(string prefix, string message) {
	writeln("[", prefix, "] ", message);
}

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
}

class NetworkServer : AbstractNetworkServer!NetworkClient {
	protected final override void clientReceivedNewData(NetworkClient client) {

	}
}

class Hub {
	NetworkServer networkServer = new NetworkServer();

	

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

}

void main() {
	ushort port = 1555;

	Hub hub = new Hub();
	hub.startServer(port);

	for(;;) {
		hub.networkMainloopIteration();
	}

	
	/*
	while(true) {
		Socket client = server.accept();

		writeln(client.isAlive());

		if(false) {


			// TODO< read n bits, decode hown many bits to receive, or refactor the bit reading class to read ondemand >
			ubyte[] byteVector = buffer[0..2+4];


			BitstreamSource bitstreamSource = new BitstreamSource();
			bitstreamSource.resetToArray(byteVector);


			BitstreamReader!BitstreamSource bitstreamReader = new BitstreamReader!BitstreamSource(bitstreamSource);


			bool successChained = true;

			uint numberOfByteToReadForString = bitstreamReader.getUint__n(16, successChained);
			writeln("number of bytes of string=", numberOfByteToReadForString);

			if( !successChained ) {
				reportError(EnumErrorType.NONCRITICAL, "Bitstream reading failed!");
			}


			client.shutdown(SocketShutdown.BOTH);

		}


	}
	return;
	*/
}
