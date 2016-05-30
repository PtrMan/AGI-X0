// just for testing
import std.stdio : writeln;

import std.socket;



import serialisation.BitstreamWriter;
import misc.BitstreamDestination;

//import misc.ConvertBitstream;

import optimisation.cartesianGeneticProgramming.CartesianGeneticProgramming;
import optimisation.cartesianGeneticProgramming.TokenOperators;

import misc.GenericSerializer;
import distributed.GlobalStructs;



enum EnumErrorType {
	NONCRITICAL
}

// TODO< move into own file >
void reportError(EnumErrorType errorType, string message) {
	writeln("[ERROR] noncritical: ", message);
}

// TODO< move into own file >
void report(string prefix, string message) {
	writeln("[", prefix, "] ", message);
}


import distributed.DistributedHelpers : composeMessageWithLengthPrefix;

void main() {
	string host = "127.0.0.1";
	ushort port = 1555;

	auto socket = new Socket(AddressFamily.INET, SocketType.STREAM, ProtocolType.TCP);
	scope(exit) socket.close();
	
	socket.connect(getAddress(host, port)[0]);
	
	auto buffer = new ubyte[4096];
	ptrdiff_t amountRead;

	//amountRead = socket.receive(buffer));

	BitstreamDestination payloadBitstream = new BitstreamDestination();

	BitstreamWriter!BitstreamDestination bitstreamWriterForPayload = new BitstreamWriter!BitstreamDestination(payloadBitstream);

	bool successChained = true;

	payloadBitstream.flush();
	{
		bitstreamWriterForPayload.addUint__n(1, 16, successChained); // type of message

		import misc.Guid;

		AgentIdentificationHandshake agentIdentificationHandshake;
		agentIdentificationHandshake.guid = generateGuid("softComputing", 1);
		GenericSerializer!(AgentIdentificationHandshake, BitstreamDestination).serialize(agentIdentificationHandshake, successChained, bitstreamWriterForPayload);

		if( !successChained ) {
			reportError(EnumErrorType.NONCRITICAL, "Serialisation failed!");
		}
	}

	{
		ubyte[] message = composeMessageWithLengthPrefix(payloadBitstream, successChained);

		if( !successChained ) {
			reportError(EnumErrorType.NONCRITICAL, "-verbose main() " ~ "serialisation failed!");
		}

		writeln("bitstreamDestinationForMessage.dataAsUbyte length=", message.length);

		long sentNumber = socket.send(cast(const void[])message, cast(SocketFlags)0);

		writeln(sentNumber);
	}



	payloadBitstream.flush();
	successChained = true;

	{
		bitstreamWriterForPayload.addUint__n(cast(uint)EnumMessageType.REGISTERSERVICE, 16, successChained); // type of message
		RegisterService registerService;
		registerService.serviceName = "CartesianGeneticProgramming";
		registerService.serviceVersion = 1;
		GenericSerializer!(RegisterService, BitstreamDestination).serialize(registerService, successChained, bitstreamWriterForPayload);

		if( !successChained ) {
			reportError(EnumErrorType.NONCRITICAL, "Serialisation failed!");
		}
	}

	{
		ubyte[] message = composeMessageWithLengthPrefix(payloadBitstream, successChained);

		if( !successChained ) {
			reportError(EnumErrorType.NONCRITICAL, "-verbose main() " ~ "serialisation failed!");
		}

		writeln("bitstreamDestinationForMessage.dataAsUbyte length=", message.length);

		long sentNumber = socket.send(cast(const void[])message, cast(SocketFlags)0);

		writeln(sentNumber);
	}



	while((amountRead = socket.receive(buffer)) != 0) {
		//enforce(amountRead > 0, lastSocketError);
		
		// Do stuff with buffer
	}
}
