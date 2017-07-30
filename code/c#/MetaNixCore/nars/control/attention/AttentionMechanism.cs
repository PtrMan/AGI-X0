using System;

using MetaNix.nars.entity;
using MetaNix.nars.memory;

namespace MetaNix.nars.control.attention {
    // abstraction for the attention mechanism for novel tasks
    public interface IAttentionMechanism<ElementType /*KeyType */> {
        void addNovelTask(ElementType element);

        ElementType getNextAttentionElement();

        void clear();

        void setMaxSize(uint size);
    }

    /* commented out because it is for now to experimental
    // attention allocation mechanism inspired by how markets decide the best price between consumer and producer.
    // 
    // The currency here is "attention", elements which require a lot of attention have a high "price".
    public class EcconomyAttentionMechanism<ElementType, KeyType> : IAttentionMechanism<ElementType, KeyType> {
        public ElementType getNextAttentionElement() {
            // TODO
            throw new NotImplementedException();
        }

        public void putIn(ElementType element, KeyType key) {
            throw new NotImplementedException();
        }
    }*/

    // bag based attention mechanism like OpenNARS 1.6.5 is doing
    public class BagBasedAttentionMechanism : IAttentionMechanism<ClassicalTask> {
        public ClassicalTask getNextAttentionElement() {
            return bag.takeNext();
        }

        public void addNovelTask(ClassicalTask novelTask) {
            bag.putIn(novelTask);
        }

        public void clear() {
            bag.clear();
        }

        public void setMaxSize(uint size) {
            bag.setMaxSize(size);
        }

        Bag<ClassicalTask, ClassicalSentence> bag = new ArrayBag<ClassicalTask, ClassicalSentence>();
    }

}
