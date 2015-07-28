package ptrman.FargGeneral;

import java.util.Random;

/**
 * Propabilistic queue for all codelets
 */
public class Coderack {
    public void enqueue(Codelet codelet, float priority) {
        queue.add(codelet, priority);
    }
    
    public void flush() {
        queue.flush();
    }
    
    public void cycle(int count) {
        int counter;
        
        for( counter = 0; counter < count; counter++ ) {
            if( queue.getSize() == 0 ) {
                break;
            }

            PriorityQueue.QueueElement queueElement = queue.dequeueQueueElement();
            Codelet currentCodelet = (Codelet)queueElement.value;
            
            // execute codelet and examine result, enqueue it to the queue if it needs to be execute again
            Codelet.RunResult runResult = currentCodelet.run();
            if( runResult.putback ) {
                queue.add(currentCodelet, queueElement.priority);
            }
        }
    }
    
    private PriorityQueue queue = new PriorityQueue(new Random());
}
