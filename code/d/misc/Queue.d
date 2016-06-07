module misc.Queue;

// a small discussion about using the condition variable for queue is at https://www.justsoftwaresolutions.co.uk/threading/implementing-a-thread-safe-queue-using-condition-variables.html

mixin template QueueConcurrencyStrategy_Dummy() {
	protected final void concurrency_ctor() {
	}

	protected final void concurrency_lock() {
	}

	protected final void concurrency_unlock() {
	}
}

import core.sync.mutex;

mixin template QueueConcurrencyStrategy_Mutex() {
	protected final void concurrency_ctor() {
		protectedMutex = new Mutex();
	}

	protected final void concurrency_lock() {
		protectedMutex.lock();
	}

	protected final void concurrency_unlock() {
		protectedMutex.unlock();
	}

	protected Mutex protectedMutex;
}

mixin template QueueStorageStrategy_GcArray(Type) {
	protected final void storage_insert(Type data) {
		protectedArray.length = protectedArray.length + 1;
		protectedArray[protectedArray.length-1] = data;
	}

	protected final void storage_peek(out bool success, out Type element) {
		import std.algorithm.mutation : remove;

		if( protectedArray.length == 0 ) {
			success = false;
			return;
		}

		element = protectedArray[0];
		protectedArray = remove(protectedArray, 0);

		success = true;
	}

	protected final size_t storage_size() {
		return protectedArray.length;
	}

	protected final bool storage_isEmpty() {
		return protectedArray.length == 0;
	}

	protected Type[] protectedArray;
}

mixin template QueueBlockingStrategy_Dummy() {
	final protected void blocking_ctor() {
		
	}

	final protected bool blocking_wasEmpty() {
		return storage_isEmpty();
	}

	final protected void blocking_notify(bool wasEmpty) {
		// do nothing
	}

	final protected void blocking_waitForData() {
		// do nothing
	}
}

import core.sync.condition;

mixin template QueueBlockingStrategy_Blocking() {
	final protected void blocking_ctor() {
		protectedCondition = new Condition(new Mutex());
	}

	// helper method for optimisation
	final protected bool blocking_wasEmpty() {
		return storage_isEmpty();
	}

	final protected void blocking_notify(bool wasEmpty) {
		if( wasEmpty ) {
            protectedCondition.notify();
        }
	}

	final protected void blocking_waitForData() {
		concurrency_lock();

		while(storage_isEmpty()) {
            protectedCondition.wait();
        }

		concurrency_unlock();
	}

	protected Condition protectedCondition;
}


mixin template QueueBehavior(Type) {
	public final this() {
		concurrency_ctor();
		blocking_ctor();
	}

	public final insert(Type data) {
		concurrency_lock();
		bool wasEmpty = blocking_wasEmpty();
		storage_insert(data);
		concurrency_unlock();

		blocking_notify(wasEmpty);
	}

	public final peek(out bool success, out Type element) {
		blocking_waitForData();

		concurrency_lock();
		storage_peek(success, element);
		concurrency_unlock();
	}

	public final @property size_t length() {
		concurrency_lock();
		size_t result = storage_size();
		concurrency_unlock();
		return result;
	}
}

class QueueNonConcurrentGcArray(Type) {
	mixin QueueConcurrencyStrategy_Dummy;
	mixin QueueStorageStrategy_GcArray!Type;
	mixin QueueBlockingStrategy_Dummy;

	mixin QueueBehavior!Type;
}

class QueueConcurrentBlockingGcArray(Type) {
	mixin QueueConcurrencyStrategy_Mutex;
	mixin QueueStorageStrategy_GcArray!Type;
	mixin QueueBlockingStrategy_Blocking;

	mixin QueueBehavior!Type;
}