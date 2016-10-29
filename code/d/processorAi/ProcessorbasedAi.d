/**
 * processor based AGI framework based on the ideas presented in the paper "The complex cognitive systems manifesto"
 */

// TODO< spawn high primitive-link count processor >
// TODO< feed high primitive link count processors if the condition of highprimitivelinkcount is fullfilled >


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
 *    if the processor is empty the process gets spawned in the next cycle
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
 *
 *
 * messages which get sent to the processes
 * ===
 * received          is a message sent from another process
 */


import std.variant : Variant;

struct Message {
	union {
		Variant receivedMessage;
	}

	enum EnumType {
		RECEIVED,
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
	bool killed; // indicates if the process got killed

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
		killed = false;
	}
}

struct Processor {
	// olds information about the next to be spawned process
	static struct NextSpawn {
		float spawnUrgency;
		ProcessDelegateType function_;
		void function(ProcessorIndex processorIndex, float spawnUrgency, ProcessDelegateType function_, bool wasSpawned) confirmation;

		final void reset() {
			spawnUrgency = 0.0f;
			function_ = null;
			confirmation = null;
		}

		final @property bool isNull() pure {
			return confirmation is null;
		}
	}

	NextSpawn nextSpawn;

	Process process;

	Processor*[] neightborhood;

	final void reset() {
		process.reset();
		resetTrySpawn();
	}

	final void resetTrySpawn() {
		nextSpawn.reset();
	}

	final void execute(ProcessorIndex processorIndex, OperationContext *operationContext) {
		if( process.isNullProcess ) {
			return;
		}

		process.processDelegate(processorIndex, &this, operationContext, process.contextParameter);
	}
}

alias size_t ProcessorIndex;

// context used by processes for the operations as described in the *.md file or comments
struct OperationContext {
	Hub hub;

	final void send(ProcessorIndex receiverProcessorIndex, Variant message) {
		hub.operationContextSend(receiverProcessorIndex, message);
	}

	final void modifyUrgency(ProcessorIndex receiverProcessorIndex, float urgencyDelta) {
		hub.operationContextModifyUrgency(receiverProcessorIndex, urgencyDelta);
	}

	final void kill(ProcessorIndex receiverProcessorIndex, float urgency) {
		hub.operationKill(receiverProcessorIndex, urgency);
	}

	// confirmation is called defered either immediatly or defered to confirm or deny the spawn
	final void trySpawn(ProcessorIndex processorIndex, float spawnUrgency, ProcessDelegateType function_, void function(ProcessorIndex processorIndex, float spawnUrgency, ProcessDelegateType function_, bool wasSpawned) confirmation) {
		hub.operationTrySpawn(processorIndex, spawnUrgency, function_, confirmation);
	}
}

import std.algorithm.comparison : max;

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

	// if an processor got killed we have to reset it to remove the process which got killed
	private final void resetKilledProcesses() {
		foreach( ref iterationProcessor; processors ) {
			if( 
				!iterationProcessor.process.isNullProcess() && /*small optimization because we don't need to reset already reseted processes */
			    iterationProcessor.process.killed
			) {
				iterationProcessor.reset();
			}
		}
	}

	private final void resetTrySpawnOfProcessors() {
		foreach( ref iterationProcessor; processors ) {
			iterationProcessor.resetTrySpawn();
		}
	}

	private final void spawnNewProcesses() {
		foreach( processorIndex, ref iterationProcessor; processors ) {
			if( !iterationProcessor.nextSpawn.isNull ) {
				// check and send negative confirmation if the urgency was not high enough
				bool spawned = iterationProcessor.nextSpawn.spawnUrgency >= iterationProcessor.process.urgency;
				if( spawned ) {
					iterationProcessor.process.reset();
					iterationProcessor.process.processDelegate = iterationProcessor.nextSpawn.function_;
					iterationProcessor.process.urgency = iterationProcessor.nextSpawn.spawnUrgency;

					// TODO< call function to reset the context for the process >
				}

				iterationProcessor.nextSpawn.confirmation(processorIndex, iterationProcessor.nextSpawn.spawnUrgency, iterationProcessor.nextSpawn.function_, spawned);
			}
		}
	}

	final void cycle() {
		decayProcessUrgency();
		removeDecayedProcesses();//AndCorespondingLinks();
		resetTrySpawnOfProcessors();
		processProcessors();
		messageboxFlip();
		resetKilledProcesses();
		spawnNewProcesses();
	}

	////////////
	// called by operation-context
	////////////

	final void operationContextSend(ProcessorIndex receiverProcessorIndex, Variant message) {
		if( processors[receiverProcessorIndex].process.isNullProcess ) {
			return;
		}

		processors[receiverProcessorIndex].process.messageBox.addReceived(message);
	}

	final void operationContextModifyUrgency(ProcessorIndex receiverProcessorIndex, float urgencyDelta) {
		assert(!processors[receiverProcessorIndex].process.isNullProcess); // we do have an internal problem if we try to modify the urgency of a nonexisting process		

		processors[receiverProcessorIndex].process.urgency = max(urgencyDelta + processors[receiverProcessorIndex].process.urgency, 0.0f);
	}

	final void operationKill(ProcessorIndex receiverProcessorIndex, float urgency) {
		if( processors[receiverProcessorIndex].process.isNullProcess ) { // if no process runs on the processor, we ignore it
			return;
		}

		if( urgency > processors[receiverProcessorIndex].process.urgency ) { // we can just kill the process if the kill urgency is high enough
			processors[receiverProcessorIndex].process.killed = true;
		}
	}

	// confirmation is called defered either immediatly or defered to confirm or deny the spawn
	final void operationTrySpawn(ProcessorIndex processorIndex, float spawnUrgency, ProcessDelegateType function_, void function(ProcessorIndex processorIndex, float spawnUrgency, ProcessDelegateType function_, bool wasSpawned) confirmation) {
		assert(function_ !is null);

		if( spawnUrgency < processors[processorIndex].nextSpawn.spawnUrgency ) {
			confirmation(processorIndex, spawnUrgency, function_, false); // send nagative confirmation directly
			return;
		}

		// we have to send a negative confirmation to the spawn request we may override now
		if( processors[processorIndex].nextSpawn.confirmation !is null ) {
			processors[processorIndex].nextSpawn.confirmation(processorIndex, processors[processorIndex].nextSpawn.spawnUrgency, processors[processorIndex].nextSpawn.function_, false);
		}

		processors[processorIndex].nextSpawn.spawnUrgency = spawnUrgency;
		processors[processorIndex].nextSpawn.function_ = function_;
		processors[processorIndex].nextSpawn.confirmation = confirmation;
	}

}















class PerceptionMessage {
	ProcessorIndex senderProcessorIndex;

	uint linkId;

	enum EnumType {
		KEEPLINKALIVE, // sent to an processor/process to keep an link alive
		// TODO< ESTABLISHLINK >

		KEEPALIVE, // sent to an process to increase the lifetime of the process to where the message is sent to
	}

	EnumType type;

	static PerceptionMessage makeKeepLinkAlive(ProcessorIndex senderProcessorIndex, uint linkId) {
		PerceptionMessage result = new PerceptionMessage(EnumType.KEEPLINKALIVE);
		result.senderProcessorIndex = senderProcessorIndex;
		result.linkId = linkId;
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
	uint linkId;
}

struct VisualProcessorContext {
	AliveLink*[] aliveLinks;

	uint maxLinkAge; // maximal age of an link which didn't got updated

	uint noKeepAliveMessageSinceCycles; // number of cycles since when no keep alive message was sent

	// resets the state of the context to the startstate
	final void reset() {
		aliveLinks.length = 0;
		noKeepAliveMessageSinceCycles = 0;
	}
}


// TODO< implement this description >
/*
   Each processor coresponds to an location in the image.
   Each pixel in the image can corespond to less than one, one or more than one processors.
   Processors are connected to the immediate neightborhood and processors further away. These long connections are required to keep track of
   the soroundings.
   The neightborhood is divided into the following neightbor-slot's:
      * processors of this pixel, which are a number of processors for keeping track of contextual information for this pixel
      * immediate neightbor processors for the pixels
      * neightbor nonpixel processors

   similar processes reinforce each other by sending messages, establishing links and pushing their urgency if links exist.

   example:


   
   frame 1
   .   .   .   .
   a   b   c   .
   .   .   .   .
   .   .   .   .
   
   Here the pixel 'a' and 'b' can establish a primitive-link, 'b' and 'c' can establish a primitive-link too.
   
   'b' could spawn another process which indicates a high primitive-link count. The new process is spawned in the neightbor-slot for the processors of this pixel.
   This could be interpreted as 'lineness' on a higher level which monitors the low level interactions of the processes.
   the new created process increases its own urgency by receiving messages from b, because b broadcasts these special messages to the neightborhood.
   If b doesn't have a high primitive-link count anymore it stops sending these special messages,
   which mean that the process which indices a high primitive-link count dies off.

   Note that there are no hardcoded concepts such as points, lines, shapes, etc. these high level descriptions can be derived by a system which monitors the
   processes and messages between them.

   frame 2
   .   .   .   .
    ,  b   c   d,
   .   .   .   .
   .   .   .   .
   
   The link between 'a' and 'b' immediatly dies of, a new link between 'c' and 'd' gets established.
   Over the next time period the high-primitive-link-count process feed by 'b' dies of because it doesn't get messages from b anymore.
   A new high-primitive-link-count process is spawned and fed from 'c'


   frame 3
   .   .   .   .
   .   .,  .,  .,
   .   x,  y,  .
   .   .   .   .
   .   .   .   .

   One new thing beyond the above explained mechanisms could be the spawn of change processes. 
   If there are enough change processes with an gradient special "gradient change processes" could get tried to be spawned from the outside.
   These can try to spawn "movement tracking" processes which increment their urgency with each successive motion.
   These processes have to move themself to the positions where the motion ended in the last frame.

   This functionality can be used to track contours or blinking edges or any such "high level features".
   Here again the basic processes are not task dependent and there is not a 1:1 mapping to the high level phenomeon.
   "Movement tracking" processes could corespond to moving edges, blinking areas or anything like this.



*/



// structure which abstracts common variables and methods of a visual Process
struct VisualProcessCommon {
	Processor *self; // the processor on which the process is running on
	VisualProcessorContext *context;
	OperationContext *operationContext;
	ProcessorIndex processorIndex;




	// retrives the processors of specified categories

	private static const size_t NUMBEROFIMMEDIATEPIXELPROCESSORS = 1;
	private static const size_t NUMBEROFCONTEXTUALIXELPROCESSORS = 3;

	private static const size_t NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXEL = NUMBEROFIMMEDIATEPIXELPROCESSORS+NUMBEROFCONTEXTUALIXELPROCESSORS;
	private static const size_t NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXELIMMEDIATENEIGHTBORHOOD = 4;

	private enum EnumNeightborhoodIndices {
		_0 = 0,
		_1 = _0 + NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXEL,
		_2 = _1 + NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXELIMMEDIATENEIGHTBORHOOD,
		_3 = _2 + (NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXEL-NUMBEROFIMMEDIATEPIXELPROCESSORS) * NEIGHTBORHOOD_NUMBEROFPROCESSORSOFPIXELIMMEDIATENEIGHTBORHOOD, // because one processor is receiving the immediate pixel activity
	}

	// bitmask
	enum EnumNeightborhoodProcessorCategories {
		PROCESSORSOFTHISPIXEL = 1,           // * processors of this pixel, which are a number of processors for keeping track of contextual information for this pixel
		IMMEDIATENEIGHTBORHOODFORPIXELS = 2, // * immediate neightbor processors for the pixels
		NEIGHTBORHOODNONPIXEL = 4,           // * neightbor nonpixel processors
	}

	final Processor*[] getNeightborhoodProcessors(uint categories) pure {
		Processor*[] result;
		if( categories & EnumNeightborhoodProcessorCategories.PROCESSORSOFTHISPIXEL ) {
			result ~= self.neightborhood[EnumNeightborhoodIndices._0..EnumNeightborhoodIndices._1];
		}
		if( categories & EnumNeightborhoodProcessorCategories.IMMEDIATENEIGHTBORHOODFORPIXELS ) {
			result ~= self.neightborhood[EnumNeightborhoodIndices._1..EnumNeightborhoodIndices._2];
		}
		if( categories & EnumNeightborhoodProcessorCategories.NEIGHTBORHOODNONPIXEL ) {
			result ~= self.neightborhood[EnumNeightborhoodIndices._2..EnumNeightborhoodIndices._3];
		}
		return result;
	}




	//////////////////
	// abstraction for all operations
	//////////////////

	private final void operationIncreaseSelfUrgencyBy(float urgencyDelta) {
		assert(urgencyDelta >= 0.0f);
		operationContext.modifyUrgency(/* for the process of this processor*/processorIndex, urgencyDelta);
	}

	final private void operationKillSelfWithInfiniteUrgency() {
		operationContext.kill(/* for the process of this processor*/processorIndex, 1e10 /* TODO< maximal float number > */);
	}
}


import std.array : array;
import std.algorithm.iteration : filter;

struct VisualProcessHelper {
	VisualProcessCommon common;

	private static const URGENCYINCREASEFORLINK = 0.3f;

	final void run() {
		sendAliveMessageToAliveLinks();
		incrementNoResponseTimeOfLinks();
		receiveKeepAliveMessagesAndResetCounterForLinks();
		removeTooOldLinks();

		incrementUrgencyForExistingLinks();
	}

	// for each link we add a certain amount of urgency
	private final void incrementUrgencyForExistingLinks() {
		foreach( iterationAliveLink; common.context.aliveLinks ) {
			common.operationIncreaseSelfUrgencyBy(URGENCYINCREASEFORLINK);
		}
	}

	private final void removeTooOldLinks() {
		common.context.aliveLinks = common.context.aliveLinks.filter!(v => v.noResponseSinceTicks <= common.context.maxLinkAge).array;
	}

	private final void incrementNoResponseTimeOfLinks() {
		foreach( iterationAliveLink; common.context.aliveLinks ) {
			iterationAliveLink.noResponseSinceTicks++;
		}
	}

	private final void sendAliveMessageToAliveLinks() {
		foreach( iterationAliveLink; common.context.aliveLinks ) {
			import std.stdio;
			import std.format : format;
			writeln("send keep alive to linkTo=%s for linkId=%s".format(iterationAliveLink.linkTo, iterationAliveLink.linkId));

			common.operationContext.send(iterationAliveLink.linkTo, Variant(PerceptionMessage.makeKeepLinkAlive(/*sender processor index */common.processorIndex, iterationAliveLink.linkId)));
		}
	}

	private final void receiveKeepAliveMessagesAndResetCounterForLinks() {
		common.self.process.messageBox.iterateAndRemove((Message message, out bool removeMessage) {
			// searches for the link with the coresponding processor id and resets the response time
			void innerFnKeepLinkAlive(ProcessorIndex senderProcessorIndex, uint linkId) {
				import std.format : format;
				import std.stdio; writeln("called innerFnKeepLinkAlive() for senderProcessorIndex=%s linkId=%s".format(senderProcessorIndex, linkId));

				foreach( iterationLink; common.context.aliveLinks ) {
					import std.format : format;
					import std.stdio; writeln("iterate over link linkTo=%s linkId=%s".format(iterationLink.linkTo, iterationLink.linkId));

					if( iterationLink.linkTo == senderProcessorIndex && iterationLink.linkId == linkId) {
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
					innerFnKeepLinkAlive(receivedMessagePayload.senderProcessorIndex, receivedMessagePayload.linkId);
					removeMessage = true;
				}
			}
		});
	}
}



void visualProcess(ProcessorIndex processorIndex, Processor *self, OperationContext *operationContext, void *contextParameter) {
	VisualProcessHelper _;
	_.common.context = cast(VisualProcessorContext*)contextParameter;
	_.common.self = self;
	_.common.processorIndex = processorIndex;
	_.common.operationContext = operationContext;
	_.run();
}



struct VisualHighPrimitiveLinkCountProcessHelper {
	VisualProcessCommon common;

	private static uint MAXNOKEEPALIVEMESSAGESUNTILSELFKILL = 4; // hhow many cycles can be waited until the process kills itself because 
	                                                             // no other process sends keep alive messages to it?

	final void run() {
		incrementCylce();
		receiveKeepAliveMessages();
		killSelfIfTooOld();
	}

	final private void receiveKeepAliveMessages() {
		common.self.process.messageBox.iterateAndRemove((Message message, out bool removeMessage) {
			removeMessage = false;

			if( message.type == Message.EnumType.RECEIVED ) {
				// convert the message to our message type and check if the type of the message is equal to the keep alive message,
				// if it is we read out the processorIndex

				// then we reset the link for the correpsonding processor 
				
				PerceptionMessage receivedMessagePayload = message.receivedMessage.get!PerceptionMessage;
				if( receivedMessagePayload.type == PerceptionMessage.EnumType.KEEPALIVE ) {
					common.context.noKeepAliveMessageSinceCycles = 0;
					removeMessage = true;
				}
			}
		});
	}

	final private void incrementCylce() {
		common.context.noKeepAliveMessageSinceCycles++;
	}

	final private void killSelfIfTooOld() {
		if( common.context.noKeepAliveMessageSinceCycles > MAXNOKEEPALIVEMESSAGESUNTILSELFKILL ) {
			common.operationKillSelfWithInfiniteUrgency();
		}
	}

}

/*
 * process used to keep track of a high count of primitive links of an pixel
 * is kept alive by specialized messages from the initiator
 *
 */
void visualHighPrimitiveLinkCountProcess(ProcessorIndex processorIndex, Processor *self, OperationContext *operationContext, void *contextParameter) {
	VisualHighPrimitiveLinkCountProcessHelper _;
	_.common.context = cast(VisualProcessorContext*)contextParameter;
	_.common.self = self;
	_.common.processorIndex = processorIndex;
	_.common.operationContext = operationContext;
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
		visualProcessorContext.aliveLinks[0].linkId = 0;
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
