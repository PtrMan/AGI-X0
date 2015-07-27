package ptrman.FargGeneral;

import ptrman.bpsolver.SolverCodelet;
import java.util.Random;

/**
 * Propabilistic queue for all codelets
 */
public class Coderack
{
    public void enqueue(Codelet codelet, float priority)
    {
        queue.add(codelet, priority);
    }
    
    public void flush()
    {
        queue.flush();
    }
    
    public void cycle(int count)
    {
        int counter;
        
        for( counter = 0; counter < count; counter++ )
        {
            Codelet currentCodelet;
            SolverCodelet.RunResult runResult;
            PriorityQueue.QueueElement queueElement;
            
            if( queue.getSize() == 0 )
            {
                break;
            }
            
            queueElement = queue.dequeueQueueElement();
            currentCodelet = (Codelet)queueElement.value;
            
            // execute codelet and examine result, enqueue it to the queue if it needs to be execute again
            runResult = currentCodelet.run();
            if( runResult.putback )
            {
                queue.add(currentCodelet, queueElement.priority);
            }
        }
    }
    
    private PriorityQueue queue = new PriorityQueue(new Random());
}
