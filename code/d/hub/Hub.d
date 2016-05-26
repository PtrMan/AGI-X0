import std.stdio; // TODO< just >
import std.format : format;

import std.socket;

import serialisation.BitstreamReader;

import misc.ConvertBitstream;

void main() {
	ushort port = 1555;

	uint backlog = 1;

	sockaddr_in sa;
	sa.sin_family = AF_INET;
	sa.sin_addr.s_addr = INADDR_ANY;
	sa.sin_port = htons(port);

	Socket server = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
	server.blocking = true;
	server.bind(new UnknownAddressReference(cast(sockaddr*)&sa, sa.sizeof));
	server.listen(backlog);
	writeln("[server] waiting");

	while(true) {
		Socket client = server.accept();
		client.blocking = true;

		ubyte[4096] buffer;
		client.receive(buffer);

		// TODO< read n bits, decode hown many bits to receive, or refactor the bit reading class to read ondemand >
		ubyte[] byteVector = buffer[0..2+4];
		bool[] bitVector = toBool(byteVector);

		writeln("bitVector.length ", bitVector.length);
		writeln("bitVector ", bitVector);


		bool calleeSuccess;
		BitstreamReader bitstreamReader = new BitstreamReader();
		bitstreamReader.fillFrom(bitVector, 0, (2+4)*8-1, calleeSuccess);

		// TODO< proper handling >
		if( !calleeSuccess ) {
			writeln("[error] Couldn't read data into BitstreamReader");
			return;
		}

		bool successChained = true;

		uint numberOfByteToReadForString = bitstreamReader.getUint__n(16, successChained);
		writeln("number of bytes of string=", numberOfByteToReadForString);


		client.shutdown(SocketShutdown.BOTH);
	}
	return;
}
