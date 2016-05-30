module distributed.DistributedHelpers;

import misc.BitstreamDestination;
import serialisation.BitstreamWriter;

ubyte[] composeMessageWithLengthPrefix(BitstreamDestination payloadBitstream, out bool successChained) {
	BitstreamDestination bitstreamDestinationForMessage = new BitstreamDestination();
	BitstreamWriter!BitstreamDestination bitstreamWriterForMessage = new BitstreamWriter!BitstreamDestination(bitstreamDestinationForMessage);
	successChained = true;
	bitstreamWriterForMessage.addArray__16(payloadBitstream.dataAsUbyte, successChained);

	ubyte[] message = bitstreamDestinationForMessage.dataAsUbyte;
	return message;
}
