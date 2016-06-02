module network.AbstractNetworking;

import std.socket;

abstract class AbstractNetworkClient {
	public final this(Socket socket) {
		this.socket = socket;
	}

	public Socket socket;

	public ubyte[] receivedQueue;
}

// TODO< functionality to close down TCP connection
// with membmer Socket.close()

abstract class AbstractNetworkHost(ClientType : AbstractNetworkClient) {
	protected final this() {
	}

	public final this(EnumRole role) {
		this.role = role;
	}

	public final void iteration() {
		SocketSet checkReadSet = new SocketSet();
		SocketSet checkWriteSet = new SocketSet();
		SocketSet checkErrorSet = new SocketSet();

		if( role == EnumRole.SERVER ) {
			checkReadSet.add(serverSocket);
		}
		
		foreach( iterationClient; clients ) {
			checkReadSet.add(iterationClient.socket);
		}

		// select crashes if nothing is in the sets
		if( role == EnumRole.CLIENT && clients.length == 0 ) {
			return;
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

			import std.stdio : writeln;
			writeln("AbstractNetworking interation(),  numberOfSocketsWithStatusChanges=", numberOfSocketsWithStatusChanges);

			foreach( iStatusChange; 0..numberOfSocketsWithStatusChanges ) {
				if( role == EnumRole.SERVER && checkReadSet.isSet(serverSocket) ) {
					Socket clientSocket = serverSocket.accept();

					assert(clientSocket.isAlive);
					ClientType createdClient = new ClientType(clientSocket);
					createdClient.socket.blocking = true;
					clients ~= createdClient;

					checkReadSet.remove(serverSocket);
				}

				for( uint clientI = 0; clientI < clients.length; clientI++ ) {
					ClientType iterationClient = clients[clientI];

					if( checkReadSet.isSet(iterationClient.socket) ) {
						bool socketClosed;
						receiveDataOfClient(iterationClient, socketClosed);

						checkReadSet.remove(iterationClient.socket);

						if( socketClosed ) {
							clientDisconnected(iterationClient);

							import std.algorithm.mutation : remove;
							clients = remove(clients, clientI);
							
							clientI--;
							continue;
						}
					}
				}

				foreach( iterationClient; clients ) {
					
				}
			}
		}
	}

	public final void startServer(ushort port) {
		if( role != EnumRole.SERVER ) {
			// TODO< throw something >
			return;
		}

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

	public final void connectAsClient(string host, ushort port) {
		if( role != EnumRole.CLIENT ) {
			// TODO< throw something >
			return;
		}

		Socket socket = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
		socket.connect(getAddress(host, port)[0]);

		clients ~= new ClientType(socket);
	}

	public final ClientType getClientForRoleClient() {
		if( role != EnumRole.CLIENT ) {
			// TODO< throw something >
			return null;
		}

		return clients[0];
	}

	protected final void receiveDataOfClient(ClientType client, out bool socketClosed) {
		socketClosed = false;

		ubyte[4096] buffer;

		ptrdiff_t receiveResult = client.socket.receive(buffer);
		if( receiveResult == -1 ) {
			if( Socket.ERROR == -1 /* on windows */ ) {
				// socket was closed
				socketClosed = true;
				return;
			}

			// client hasn't received new data
			return;
		}
		else if( receiveResult == 0 ) {
			// if a read returns 0 bytes the socket closed
			socketClosed = true;
			return;
		}



		client.receivedQueue ~= buffer[0..receiveResult];

		clientReceivedNewData(client);

		import std.stdio;
		writeln("AbstractNetworkServer.receiveDataOfClient(), received data from client=", receiveResult);
	}

	enum EnumRole {
		SERVER,
		CLIENT
	}

	protected EnumRole role;

	// callback if a client has received new data
	protected abstract void clientReceivedNewData(ClientType client);

	protected abstract void clientDisconnected(ClientType client);

	protected Socket serverSocket;
	protected ClientType[] clients;
}