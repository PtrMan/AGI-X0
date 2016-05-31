module distributed.GlobalStructs;

import std.stdint;

enum EnumMessageType : uint16_t {
	AGENTIDENTIFICATIONHANDSHAKE = 1,
	REGISTERSERVICES,
	AGENTCONNECTTOSERVICE,
	AGENTCONNECTTOSERVICERESPONSE
}

// Agent -> hub
struct AgentIdentificationHandshake {
	ubyte[16] guid;
}


enum EnumContractServiceRequestFrequency : uint8_t {
	NONE,
	SPORADIC,
	BULK
}

enum EnumStateful : uint8_t {
	STATELESS,
	PARTIAL,
	STATEFUL
}

enum EnumLifecycle : uint8_t {
	TESTING, // service is unstable in "alpha" under test
	// TODO< other ones >
}

// helper
struct ServiceCapabilities {
	bool isSoftComputing; // does the service fall into the range of soft computing (fuzzy logic, Genetic Programming, swarm evolution, etc.)
	EnumStateful stateful; // how much state does the service carry
}

// helper
struct ContractInformation {
	EnumContractServiceRequestFrequency serviceRequestFrequency; // how often and in which patterns does the service request other services?
	EnumContractServiceRequestFrequency serviceUsageFrequency;
	EnumLifecycle lifecylce;
}

// helper
struct ServiceDescriptor {
	string name;
	uint32_t version_;

	ServiceCapabilities capabilities;
	ContractInformation contract;
}

// Agent -> hub
struct RegisterServices {
	ServiceDescriptor[] service;
}

// Agent-> hub
struct AgentConnectToService {
	string serviceName;
	uint32_t[] acceptedVersions;
	bool serviceVersionsAndUp; // if it is true then the accept the lowester version of acceptedVersions
}

// hub-> agent
struct AgentConnectToServiceResponse {
	string serviceName;
	uint32_t[] providedVersions;
	bool connectSuccess;
}
