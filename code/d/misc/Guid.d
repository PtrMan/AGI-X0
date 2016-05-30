module misc.Guid;

import std.algorithm : min;

ubyte[16] generateGuid(string agentName, uint agentVersion) {
	ubyte[16] result;
	result[0] = 1;
	foreach(i ; 0.. min(agentName.length, 8)) {
		result[1+i] = agentName[i];
	}

	result[8+0] = cast(byte)agentVersion;
	result[8+1] = cast(byte)(agentVersion >> 8);
	result[8+2] = cast(byte)(agentVersion >> 16);
	result[8+3] = cast(byte)(agentVersion >> 24);

	// the remaining are 0

	return result;
}

void decodeGuid(ubyte[16] guid, out string agentName, out uint agentVersion) {
	agentName = "";
	foreach( i ; 0..8 ) {
		if( guid[1+i] == 0 ) {
			break;
		}

		agentName ~= cast(char)guid[1+i];
	}

	agentVersion = 0;
	agentVersion |= cast(uint)guid[8+0];
	agentVersion |= (cast(uint)guid[8+1] << 8);
	agentVersion |= (cast(uint)guid[8+1] << 16);
	agentVersion |= (cast(uint)guid[8+1] << 24);
}
