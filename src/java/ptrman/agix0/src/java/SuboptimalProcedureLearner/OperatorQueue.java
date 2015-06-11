package ptrman.agix0.src.java.SuboptimalProcedureLearner;

import ptrman.misc.Assert;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class OperatorQueue {
    public static class QueueElement {
        public OperatorBlueprint blueprint;
        public float priority = 0.0f;
    }

    public OperatorQueue(final int size, Random random) {
        this.size = size;
        this.random = random;
    }

    public QueueElement getReference() {
        // NOTE< a priority 10 times as high means the concept gets selected 10 times as much >
        // algorithm is (very) inefficient
        Assert.Assert(queue.size() > 0, "");
        int priorityDiscreteRange = (int)(prioritySum / PRIORITYGRANULARITY);
        int chosenDiscretePriority = random.nextInt(priorityDiscreteRange);
        float remainingPriority = (float)chosenDiscretePriority * PRIORITYGRANULARITY;
        int chosenIndex = 0;
        for (;;) {
            if( remainingPriority < queue.get(chosenIndex).priority ) {
                break;
            }
             
            // else
            remainingPriority -= queue.get(chosenIndex).priority;
        }
        return queue.get(chosenIndex);
    }

    /**
     *
     * only used for (re)filling
     */
    public void add(OperatorBlueprint value, final float priority) {
        Assert.Assert(queue.size() <= size, "");
        QueueElement element = new QueueElement();
        element.priority = priority;
        element.blueprint = value;
        if (queue.size() == size) {
            // replace the last element with the new element
            queue.set(queue.size() - 1, element);
            return;
        }
         
        // else
        queue.add(element);
    }

    // the queue is sorted by priority
    private List<QueueElement> queue = new ArrayList<QueueElement>();
    private final int size;
    private Random random;
    private float prioritySum = 0.0f;
    private static final float PRIORITYGRANULARITY = 0.02f;
}
