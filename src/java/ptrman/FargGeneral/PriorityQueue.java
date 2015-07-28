package ptrman.FargGeneral;

import ptrman.misc.Assert;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class PriorityQueue<Type> {
    public static class QueueElement<Type> {
        public Type value;
        public float priority = 0.0f;
    }
    
    public PriorityQueue(Random random) {
        this.random = random;
    }
    
    public QueueElement getReference() {
        int chosenIndex;
        
        chosenIndex = getNextQueueIndex();

        return queue.get(chosenIndex);
    }
    
    /**
    * 
    * only used for (re)filling
    */
    public void add(Type value, float priority) {
        QueueElement element;
        
        element = new QueueElement();
        element.priority = priority;
        element.value = value;
        
        prioritySum += priority;
        
        queue.add(element);
    }
    
    public void flush() {
        queue.clear();
        prioritySum = 0.0f;
    }
    
    public int getSize() {
        return queue.size();
    }
    
    public Type dequeue() {
        return dequeueQueueElement().value;
    }
    
    public QueueElement<Type> dequeueQueueElement() {
        int chosenIndex;
        QueueElement<Type> result;
        
        chosenIndex = getNextQueueIndex();
        result = queue.get(chosenIndex);
        queue.remove(chosenIndex);
        
        return result;
    }
    
    private int getNextQueueIndex() {
        int chosenIndex;
        int priorityDiscreteRange;
        int chosenDiscretePriority;
        float remainingPriority;

        // NOTE< a priority 10 times as high means the concept gets selected 10 times as much >
        // algorithm is (very) inefficient

        //System.Diagnostics.Debug.Assert(queue.Count > 0);

        priorityDiscreteRange = (int)(prioritySum / PRIORITYGRANULARITY);
        chosenDiscretePriority = random.nextInt(priorityDiscreteRange);
        remainingPriority = (float)chosenDiscretePriority * PRIORITYGRANULARITY;

        chosenIndex = 0;
        for (; ; ) {
            Assert.Assert(chosenIndex <= queue.size(), "");
            
            if( chosenIndex == queue.size() ) {
                Assert.Assert(queue.size() != 0, "");
                chosenIndex = queue.size() - 1;
                
                break;
            }
            
            if( remainingPriority < queue.get(chosenIndex).priority ) {
                break;
            }
            // else

            remainingPriority -= queue.get(chosenIndex).priority;
            
            chosenIndex++;
        }
        
        return chosenIndex;
    }

    // the queue is sorted by priority
    private List<QueueElement> queue = new ArrayList<>();

    
    private Random random;
    private float prioritySum = 0.0f;

    private final float PRIORITYGRANULARITY = 0.02f;
}
