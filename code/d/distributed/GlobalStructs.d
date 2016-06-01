module distributed.GlobalStructs;

import std.stdint;

enum EnumMessageType : uint16_t {
	AGENTIDENTIFICATIONHANDSHAKE = 1,
	REGISTERSERVICES,

	AGENTCONNECTTOSERVICE,
	AGENTCONNECTTOSERVICERESPONSE,

	AGENTCREATECONTEXT,
	AGENTCREATECONTEXTRESPONSE,

	AGENTCREATEDCONTEXT,
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
struct ServiceLocator {
	string name;
	uint32_t version_;
}

// helper
struct ServiceDescriptor {
	ServiceLocator locator;

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

// agent->hub
struct AgentCreateContext {
	ServiceLocator locator;
	uint32_t requestId; // should be random
}

// hub->agent
struct AgentCreateContextResponse {
	uint32_t requestId; // same as request to couple the response to the request
	uint32_t agentServiceContextId; // unique id of the context-service-agent connection in case of success
	EnumAgentCreateContextResponseType responseType;
	string humanReadableError; // only filled if it didn't succeed
}

enum EnumAgentCreateContextResponseType {
	SUCCESS = 0,
	SERVICENOTFOUND,
	SERVICEFOUNDBUTWRONGVERSION,
	IGNORED_TOOMANYREQUESTS, // only if the Agent makes/made too many requests
	IGNORED_BLOCKED, // client is blocked for some reason and the request is completly ignored
}

// hub->agent
struct AgentCreatedContext {
	uint32_t agentServiceContextId;
}