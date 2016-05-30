module distributed.GlobalStructs;


enum EnumMessageType {
	AGENTIDENTIFICATIONHANDSHAKE = 1,
	REGISTERSERVICE,
	AGENTCONNECTTOSERVICE,
	AGENTCONNECTTOSERVICERESPONSE
}

// Agent -> hub
struct AgentIdentificationHandshake {
	ubyte[16] guid;
}

// Agent -> hub
struct RegisterService {
	string serviceName;
	uint serviceVersion;
}

// Agent-> hub
struct AgentConnectToService {
	string serviceName;
	uint[] acceptedVersions;
	bool serviceVersionsAndUp; // if it is true then the accept the lowester version of acceptedVersions
}

// hub-> agent
struct AgentConnectToServiceResponse {
	string serviceName;
	uint[] providedVersions;
	bool connectSuccess;
}
