// just for testing
import std.stdio : writeln;

import std.socket;



import serialisation.BitstreamWriter;
import misc.ConvertBitstream;

import optimisation.cartesianGeneticProgramming.CartesianGeneticProgramming;
import optimisation.cartesianGeneticProgramming.TokenOperators;


void main() {
	string host = "127.0.0.1";
	ushort port = 1555;

	auto socket = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
	scope(exit) socket.close();
	
	socket.connect(getAddress(host, port)[0]);
	
	auto buffer = new ubyte[4096];
	ptrdiff_t amountRead;

	//amountRead = socket.receive(buffer));

	BitstreamWriter sink = new BitstreamWriter();

	bool successChained = true;
	sink.addString("test", successChained);

	// TODO< alright serialisation and error handling >
	if( !successChained ) {
		writeln("Building BitstreamWriter message failed!");
		return;
	}

	ubyte[] toSend = toUbyte(sink.data);

	long sentNumber = socket.send(cast(const void[])toSend, cast(SocketFlags)0);

	writeln(sentNumber);


	while((amountRead = socket.receive(buffer)) != 0) {
		//enforce(amountRead > 0, lastSocketError);
		
		// Do stuff with buffer
	}
}
