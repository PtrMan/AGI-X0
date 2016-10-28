/**
 * processor based AGI framework based on the ideas presented in the paper "The complex cognitive systems manifesto"
 */

/**
 * The processors lay in a (prewired) grid.
 * Processors process programs.
 * These programs can connect to other processors, spawn new processes in these, send information to other processors.
 * 
 * processes do have a value "urgency", which indicates how important the process is for other processes.
 * urgency decays with time and processes get deleted if the urgency falls below a certain threshold.
 * each processor can execute just one process.
 *
 * operations
 * ===
 * 
 * tryspawn("spawnUrgency")    [to other processor]
 *    tries to spawn a new process with the (initial) "spawnUrgency" in a processor
 *    if the processor is empty the process gets spawned immediatly
 *    if a process already runs in the processor, there are two cases
 *           if the spawnUrgency is bigger than the urgency of running process the old process gets replaced
 *           else the new process doesn't get started
 * send                        [to other processor]
 *    tries to send a message to an processor
 * 
 * modifyUrgency                  [self]
 *    increments or decrements the urgency
 * 
 * move("moveUrgency")            [to self or other processor]
 *    tries to move to another processor, urgency rules are the same as for the tryspawn operation
 * kill("killUrgency")            [to self or other processor]
 *    kills an process, urgency rules same as for tryspawn
 * 
 * not existing:
 *  * link                           [to other processor]
 *    tries to accomplish a symbolic link to another process.
 *    the link stays until the other process gets removed or the link gets unlinked
 * * unlink                         [to other processor]
 *
 *
 * messages which get sent to the processes
 * ===
 * //linkEstablished
 * //linkHangup
 * received          is a message sent from another process
 */

/** links
 * links are a way to hold symbolic information. They act like semantic pointers, which means that they represent something
 * mentally.
 * For example a "hasA" relationship in a more symbolic system can be represented as a link in this framework.
 * Links can have attributes attached, which can be read or modified by both sides (which are processes).
 */

import std.variant : Variant;

struct Message {
	union {
		//Link!LinkDecorationType linkHangup;
		Variant receivedMessage;
	}

	enum EnumType {
		RECEIVED,

		//LINKHANGUP,
		// LINKESTABLISHED,
	}

	EnumType type;
}

struct MessageBox {
	// double buffered
	private Message[] queue;
	private Message[] queueAdd;

	final void doublebufferFlip() {
		queue ~= queueAdd;
		queueAdd.length = 0;
	}

	final void addReceived(Variant message) {
		Message newMessage;
		newMessage.type = Message.EnumType.RECEIVED;
		newMessage.receivedMessage = message;
		queueAdd.enqueue(newMessage);
	}

	/*
	final void addLinkHangup(Link!LinkDecorationType link) {
		Message!LinkDecorationType newMessage;
		newMessage.linkHangup = link;
		newMessage.type = Message!LinkDecorationType.EnumType.LINKHANGUP;
		queue.enqueue(newMessage);
	}
	*/

	// iterates with an delegate over all messages and deletes messages
	final void iterateAndRemove(void delegate(Message message, out bool removeMessage) delegate_) {
		foreach( i; 0..queue.length ) {
			bool removeMessage;
			delegate_(queue[i], /*out*/removeMessage);
			if( removeMessage ) {
				queue = queue.remove(i);
				i--;
				continue;
			}
		}
	} 

	final @property bool isEmpty() pure const {
		return queue.isEmpty;
	}

	final Message dequeue() {
		return queue.dequeue();
	}

	final void reset() {
		queue.length = 0;
	}
}

alias void function(ProcessorIndex processorIndex, Processor *self, OperationContext *operationContext, void *contextParameter) ProcessDelegateType;

struct Process {
	void *contextParameter; // is passed to the process program/function/delegate

	MessageBox messageBox;

	ProcessDelegateType processDelegate; // can be null
	float urgency;

	final void decayUrgency(float decayFactor) {
		urgency *= decayFactor;
	}

	final bool isUrgencyBelowValue(float urgency) const {
		return this.urgency < urgency;
	}

	final bool isNullProcess() const {
		return processDelegate is null;
	}

	final void reset() {
		messageBox.reset();
		processDelegate = null;
	}
}

struct Processor {
	Process process;

	Processor*[] neightborhood;

	final void reset() {
		process.reset();
	}

	final void execute(ProcessorIndex processorIndex, OperationContext *operationContext) {
		if( process.isNullProcess ) {
			return;
		}

		process.processDelegate(processorIndex, &this, operationContext, process.contextParameter);
	}
}

alias size_t ProcessorIndex;

/*
struct Link(DecorationType) {
	ProcessorIndex processors[2];

	DecorationType decoration;
}
*/

// context used by processes for the operations as described in the *.md file or comments
struct OperationContext {
	Hub hub;

	final void send(ProcessorIndex receiver, Variant message) {
		hub.operationContextSend(receiver, message);
	}
}

class Hub {
	Processor[] processors;

	float processDecayFactor;
	float processMinimalUrgency;

	private OperationContext *operationContext;

	final this() {
		operationContext = new OperationContext;
		operationContext.hub = this;
	}

	/**
	 * we have a global list of links to siplify the linking, unlinking and moving logic
	 *
	 */
	//Link!LinkDecorationType*[] links;

	private final void decayProcessUrgency() {
		foreach( ref iterationProcessor; processors ) {
			iterationProcessor.process.decayUrgency(processDecayFactor);
		}
	}

	private final ProcessorIndex[] findDecayedProcessors() {
		ProcessorIndex[] decayedProcessorIndices;
		foreach( iterationProcessorIndex, ref iterationProcessor; processors ) {
			if( !iterationProcessor.process.isNullProcess() && iterationProcessor.process.isUrgencyBelowValue(processMinimalUrgency) ) {
				decayedProcessorIndices ~= iterationProcessorIndex;
			}
		}
		return decayedProcessorIndices;
	}

	/*

	private final Link!LinkDecorationType*[] findAndRemoveLinksToProcessor(ProcessorIndex processorIndex) {
		Link!LinkDecorationType*[] removed;

		foreach( i; 0..links.length ) {
			if( links[i].processors[0] == processorIndex || links[i].processors[1] == processorIndex ) {
				links = links.remove(i);
				i--;

				removed += links[i];

				continue;
			}
		}

		return removed;
	}

	// adds a message that the link got broken down to the process
	private final void enqueueLinkHangupToCorespondingProcesses(Link!LinkDecorationType*[] hangupLinks) {
		foreach( iterationHangupLink; hangupLinks ) {
			if( !processors[iterationHangupLink.processors[0]].process.isNullProcess ) {
				processors[iterationHangupLink.processors[0]].process.messageBox.addLinkHangup(iterationHangupLink);
			}

			if( !processors[iterationHangupLink.processors[1]].process.isNullProcess ) {
				processors[iterationHangupLink.processors[1]].process.messageBox.addLinkHangup(iterationHangupLink);
			}
		}
	}

	private final void removeDecayedProcessesAndCorespondingLinks() {
		ProcessorIndex[] decayedProcessors = findDecayedProcessors();
		foreach( iterationDecayedProcessor; decayedProcessors ) {
			enqueueLinkHangupToCorespondingProcesses(findAndRemoveLinksToProcessor(iterationDecayedProcessor));
		}

		// remove processes
		foreach( iterationProcessorIndex; decayedProcessors ) {
			processors[iterationProcessorIndex].reset();
		}
	}*/

	private final void removeDecayedProcesses() {
		ProcessorIndex[] decayedProcessors = findDecayedProcessors();
		// remove processes
		foreach( iterationProcessorIndex; decayedProcessors ) {
			processors[iterationProcessorIndex].reset();
		}
	}


	private final void processProcessors() {
		foreach( processorIndex, ref iterationProcessor; processors ) {
			iterationProcessor.execute(processorIndex, operationContext);
		}
	}

	private final void messageboxFlip() {
		foreach( ref iterationProcessor; processors ) {
			iterationProcessor.process.messageBox.doublebufferFlip();
		}
	}

	final void cycle() {
		decayProcessUrgency();
		removeDecayedProcesses();//AndCorespondingLinks();
		processProcessors();
		messageboxFlip();
	}

	// called by operation-context
	final void operationContextSend(ProcessorIndex receiverProcessIndex, Variant message) {
		if( processors[receiverProcessIndex].process.isNullProcess ) {
			return;
		}

		processors[receiverProcessIndex].process.messageBox.addReceived(message);
	}
}















class PerceptionMessage {
	ProcessorIndex senderProcessorIndex;

	enum EnumType {
		KEEPLINKALIVE, // sent to an processor/process to keep an link alive
	}

	EnumType type;

	static PerceptionMessage makeKeepLinkAlive(ProcessorIndex senderProcessorIndex) {
		PerceptionMessage result = new PerceptionMessage(EnumType.KEEPLINKALIVE);
		result.senderProcessorIndex = senderProcessorIndex;
		return result;
	}

	final private this(EnumType type) {
		this.senderProcessorIndex = senderProcessorIndex;
	}
}

// process for visual perception task

struct AliveLink {
	uint noResponseSinceTicks;

	ProcessorIndex linkTo;
}

struct VisualProcessorContext {
	AliveLink*[] aliveLinks;

	uint maxLinkAge; // maximal age of an link which didn't got updated
}

import std.array : array;
import std.algorithm.iteration : filter;

void visualProcess(ProcessorIndex processorIndex, Processor *self, OperationContext *operationContext, void *contextParameter) {
	struct __ {
		VisualProcessorContext* context;

		final void run() {
			sendAliveMessageToAliveLinks();
			incrementNoResponseTimeOfLinks();
			receiveKeepAliveMessagesAndResetCounterForLinks();
			removeTooOldLinks();
		}

		private final void removeTooOldLinks() {
			context.aliveLinks = context.aliveLinks.filter!(v => v.noResponseSinceTicks <= context.maxLinkAge).array;
		}

		private final void incrementNoResponseTimeOfLinks() {
			foreach( iterationAliveLink; context.aliveLinks ) {
				iterationAliveLink.noResponseSinceTicks++;
			}
		}

		private final void sendAliveMessageToAliveLinks() {
			foreach( iterationAliveLink; context.aliveLinks ) {
				import std.stdio; writeln("send keep alive to ", iterationAliveLink.linkTo);

				operationContext.send(iterationAliveLink.linkTo, Variant(PerceptionMessage.makeKeepLinkAlive(/*sender processor index */processorIndex)));
			}
		}

		private final void receiveKeepAliveMessagesAndResetCounterForLinks() {
			self.process.messageBox.iterateAndRemove((Message message, out bool removeMessage) {
				// searches for the link with the coresponding processor id and resets the response time
				void innerFnKeepLinkAlive(ProcessorIndex senderProcessorIndex) {
					foreach( iterationLink; context.aliveLinks ) {
						if( iterationLink.linkTo == senderProcessorIndex ) {
							import std.stdio; writeln("found keep alive for ", senderProcessorIndex);

							iterationLink.noResponseSinceTicks = 0;
							return; // we can return because of the invariant that the link appears just once for the senderProcessorIndex
						}
					}
				}

				removeMessage = false;

				if( message.type == Message.EnumType.RECEIVED ) {
					// convert the message to our message type and check if the type of the message is equal to the keep alive message,
					// if it is we read out the processorIndex

					// then we reset the link for the correpsonding processor 
					
					PerceptionMessage receivedMessagePayload = message.receivedMessage.get!PerceptionMessage;
					if( receivedMessagePayload.type == PerceptionMessage.EnumType.KEEPLINKALIVE ) {
						innerFnKeepLinkAlive(receivedMessagePayload.senderProcessorIndex);
						removeMessage = true;
					}
				}
			});
		}
	}

	__ _;
	_.context = cast(VisualProcessorContext*)contextParameter;
	_.run();
}



void main() {


	Hub hub = new Hub;

	hub.processors.length = 2;

	hub.processDecayFactor = 0.5f;
	hub.processMinimalUrgency = 0.0f; // disabled

	// reset processes
	foreach( ref iterationProcessor; hub.processors ) {
		iterationProcessor.process.contextParameter = new VisualProcessorContext;
		(cast(VisualProcessorContext*)(iterationProcessor.process.contextParameter)).maxLinkAge = 5;

		iterationProcessor.process.processDelegate = &visualProcess;
		iterationProcessor.process.urgency = 1.0f;
	}

	// link
	foreach( i, ref iterationProcessor; hub.processors ) {
		VisualProcessorContext *visualProcessorContext = cast(VisualProcessorContext*)(iterationProcessor.process.contextParameter);
		visualProcessorContext.aliveLinks ~= new AliveLink;
		visualProcessorContext.aliveLinks[0].linkTo = (i == 0 ? 1 : 0);
	}

	hub.cycle();
	hub.cycle();
}




// TODO< move to file in misced >

import std.algorithm.mutation : remove;

Type dequeue(Type)(ref Type[] arr) {
	Type result = arr[0];
	arr.remove(0);
	return result;
}

void enqueue(Type)(ref Type[] arr, Type element) {
	arr ~= element;
}


bool isEmpty(Type)(Type[] arr) {
	return arr.length == 0;
}
