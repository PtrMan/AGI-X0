using System;
using System.Collections.Generic;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    class OperatorQueue
    {
        public class QueueElement
        {
            public OperatorBlueprint blueprint;
            public float priority = 0.0f;
        }

        public OperatorQueue(int size, Random random)
        {
            this.size = size;
            this.random = random;
        }

        public QueueElement getReference()
        {
            int chosenIndex;
            int priorityDiscreteRange;
            int chosenDiscretePriority;
            float remainingPriority;

            // NOTE< a priority 10 times as high means the concept gets selected 10 times as much >
            // algorithm is (very) inefficient

            System.Diagnostics.Debug.Assert(queue.Count > 0);

            priorityDiscreteRange = (int)(prioritySum / PRIORITYGRANULARITY);
            chosenDiscretePriority = random.Next(priorityDiscreteRange);
            remainingPriority = (float)chosenDiscretePriority * PRIORITYGRANULARITY;

            chosenIndex = 0;
            for (; ; )
            {
                if( remainingPriority < queue[chosenIndex].priority )
                {
                    break;
                }
                // else

                remainingPriority -= queue[chosenIndex].priority;
            }
            
            return queue[chosenIndex];
        }

        /**
         * 
         * only used for (re)filling
         */
        public void add(OperatorBlueprint value, float priority)
        {
            QueueElement element;

            System.Diagnostics.Debug.Assert(queue.Count <= size);

            element = new QueueElement();
            element.priority = priority;
            element.blueprint = value;

            if( queue.Count == size )
            {
                // replace the last element with the new element
                queue[queue.Count - 1] = element;
                return;
            }
            // else

            queue.Add(element);

        }

        // the queue is sorted by priority
        private List<QueueElement> queue = new List<QueueElement>();
        
        private int size;
        private Random random;
        private float prioritySum = 0.0f;

        private const float PRIORITYGRANULARITY = 0.02f;
    }
}
