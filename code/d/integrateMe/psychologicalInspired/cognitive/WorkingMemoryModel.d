// prototype of the working memory model
// after Baddeley's model
// https://en.wikipedia.org/wiki/Baddeley%27s_model_of_working_memory
// http://www2.psych.ubc.ca/~pgraf/Psy583Readings/Baddeley%201983.pdf [baddeley1983]

module cognitive.WorkingMemoryModel;

import std.random : uniform;
import std.algorithm.searching : canFind;
import std.algorithm.iteration : filter;
import std.array : array;

import cognitive.EpisodicBuffer : EpisodicBuffer, WIDTHOFHTMLIKECELLIN64BITWORDS;

//////////////////////
// loop
// see [baddeley1983] page 7
//////////////////////

// trace which is hold inside the phonological short term store
struct Trace {
	uint[] traceContent;

	uint id;

	static Trace make(uint[] traceContent, uint id) {
		Trace result;
		result.traceContent = traceContent;
		result.id = id;
		return result;
	}
}

struct TraceWithSalience {
	Trace trace;
	float salience;

	static TraceWithSalience make(Trace trace, float salience) {
		TraceWithSalience result;
		result.trace = trace;
		result.salience = salience;
		return result;
	}
}

float minSalienceTillRemoval = 0.1f;
float salienceDecay = 0.82f;
float salienceAdditionForHit = 0.5f; // which salience gets added to the elements which were hit by rehersal?

struct PhonologicalShortTermStore {
	TraceWithSalience[] traces;

	final void tick() {
		decay();
		removeBelowMinSalience();
	}

	private final void decay() {
		foreach( ref t; traces ) {
			t.salience *= salienceDecay;
		}
	}

	final void incrementSalienceForAllTracesWithWord(uint word) {
		foreach( ref t; traces ) {
			if( t.trace.traceContent.canFind(word) ) {
				t.salience += salienceAdditionForHit;
			}
		}
	} 

	private final void removeBelowMinSalience() {
		traces = traces.filter!(v => v.salience > minSalienceTillRemoval).array;
	}

	final Trace getTraceById(uint id, out bool found) {
		found = false;

		foreach( ref t; traces ) {
			if( t.trace.id == id ) {
				found = true;
				return t.trace;
			}
		}

		return Trace.init;
	}
}

// picks one Trace at random and replays it
struct ReplayElementForTrace {
	uint traceId;
	uint index;

	final void replay(PhonologicalShortTermStore *phonologicalShortTermStore, out uint replayedTraceContent, out bool replaySuccess) {
		replaySuccess = false;

		bool found;
		Trace trace = phonologicalShortTermStore.getTraceById(traceId, found);
		if( !found ) {
			return;
		}

		if( index >= trace.traceContent.length ) {
			return;
		}

		replayedTraceContent = trace.traceContent[index];
		index++;
		replaySuccess = true;
	}
}

interface IReplayListener {
	void replayed(uint word);
}

// picks an trace at random, replays it
struct Replay {
	ReplayElementForTrace *currentReplayElement;
	PhonologicalShortTermStore *phonologicalShortTermStore;
	IReplayListener[] replayListeners;

	final void tick() {
		if( currentReplayElement is null ) {
			pickRandomReplayElement();
		}

		if( currentReplayElement is null ) {
			return;
		}

		uint replayedTraceContent;
		bool replaySuccess;
		currentReplayElement.replay(phonologicalShortTermStore, /*out*/replayedTraceContent, /*out*/replaySuccess);
		if( !replaySuccess ) {
			currentReplayElement = null;
			return;
		}

		notifyAllReplayListeners(replayedTraceContent);
		phonologicalShortTermStore.incrementSalienceForAllTracesWithWord(replayedTraceContent);
	}

	private final void notifyAllReplayListeners(uint word) {
		foreach( iterationReplayListener; replayListeners ) {
			iterationReplayListener.replayed(word);
		}
	}

	private final void pickRandomReplayElement() {
		size_t index = uniform(0, phonologicalShortTermStore.traces.length);
		currentReplayElement = new ReplayElementForTrace;
		currentReplayElement.traceId = phonologicalShortTermStore.traces[index].trace.id;
	}
}

import linopterixed.types.Bigint;

// refreshes the element(s) in the Episodic buffer for the replayed word
// must be updated/syncronized by the "central executive" 
struct EpisodicBufferReplayContext {
	EpisodicBuffer *episodicBuffer;

	// which features get refreshed in the Episodic buffer for the replayed word
	Bigint!(WIDTHOFHTMLIKECELLIN64BITWORDS)[uint] featurebyWord;

	final void wordReplayed(uint word) {
		if( !(word in featurebyWord) ) {
			return;
		}

		episodicBuffer.refreshElementsWithFeatures(featurebyWord[word]);
	}
}

// listener which refreshes the corresponding elements in the episodic buffer by the replayed word
class EpisodicBufferRefreshReplayListener : IReplayListener {
	final this(EpisodicBufferReplayContext *context) {
		this.context = context;
	}

	void replayed(uint word) {
		context.wordReplayed(word);
	}

	private EpisodicBufferReplayContext *context;
}


import std.stdio;
class PrintReplayListener : IReplayListener {
	void replayed(uint word) {
		writeln("replayed: ", word);
	}
}


// small test to see if replay works
/*
void main() {
	PhonologicalShortTermStore *phonologicalShortTermStore = new PhonologicalShortTermStore;
	Replay *replay = new Replay;
	replay.phonologicalShortTermStore = phonologicalShortTermStore;
	replay.replayListeners ~= new PrintReplayListener;

	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([5], 0), 2.0f);
	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([9, 2, 5], 1), 2.0f);
	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([1, 3, 9, 2, 5], 2), 2.0f);
	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([1, 2, 3], 3), 2.0f);
	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([1, 2], 4), 2.0f);
	phonologicalShortTermStore.traces ~= TraceWithSalience.make(Trace.make([4], 5), 2.0f);

	for( uint i = 0; i < 500; i++ ) {
		replay.tick();
		phonologicalShortTermStore.tick();

		writeln("traces in PhonologicalShortTermStore #=", phonologicalShortTermStore.traces.length);
	}
}
*/