using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using MetaNix.nars.entity;
using MetaNix.misc;

namespace MetaNix.nars.memory {
    public class ArrayBag<E, K> : Bag<E, K> where E : Item<K> {
	    public void setPriorityQuantisation(float priorityQuantisation) {
            this.priorityQuantisation = priorityQuantisation;
        }

        public override E take(K key) {
            int i = 0;
            foreach (var iElement in elements ) {
                if (iElement.name.Equals(key)) {
                    elements.RemoveAt(i);
                    return iElement;
                }
            }

            return default(E);
        }

        public override E takeNext() {
            if( elements.Count == 0 ) {
                return null;
            }

            // TODO MEDIUM < select by priority >

            ulong index = (ulong)rand.Next(elements.Count);
            E result = elements[(int)index];
            elements.RemoveAt((int)index);

            return result;
        }


        public override E referenceByKey(K key) {
            int i = 0;
            foreach (var iElement in elements) {
                if (iElement.name.Equals(key)) {
                    return iElement;
                }
            }

            return default(E);
        }

        override public E reference() {
            Contract.Requires(elements.Count > 0, "reference() called on empty ArrayBag");

            // TODO MEDIUM < select by priority >

            ulong index = (ulong)rand.Next(elements.Count);
            return elements[(int)index];
        }

        // value is [0, 1]
        //BagEntity reference(PriorityType value);

        // the number of items in the bag
        public override ulong size => (ulong)elements.Count;

        public override void clear() {
            elements.Clear();

            prioritySumQuantisized = 0;
        }

        class Compararer : IComparer<E> {
            public int Compare(E x, E y) {
                if( x.budget.priority == y.budget.priority ) {
                    return 0;
                }

                return x.budget.priority > y.budget.priority ? 1 : -1;
            }
        }
        static Compararer comparer = new Compararer();

        /**
         * Insert an item into the bag, and return the overflow
         *
         * \param newItem The Item to put in
         * \return The overflow Item, or null if nothing displaced
         */
        protected override E addItem(E element) {
            ulong quantisizedPriorityInt = quantisizePriority(element.budget.priority);
            prioritySumQuantisized += quantisizedPriorityInt;

            elements.Add(element);

            if( (ulong)elements.Count > maxSize ) {
                elements.Sort((a, b) => -1 * comparer.Compare(a, b));
                E overflowElement = elements[(int)maxSize];
                elements.limitSize(maxSize);
                return overflowElement;
            }

            return default(E); // no overflow
        }

        // value is [0, 1]
        /* uncommented because abstract Bag lass has to be overhauled that it supports this fast method
	    final BagEntity reference(float value) {
		    size_t index = sample(value);
		    return elements[index];
	    }
	    */



        public override void setMaxSize(ulong size) {
            maxSize = size;
        }

        protected ulong quantisizePriority(float priority) {
            return (uint)(priority / priorityQuantisation);
        }

        public override IEnumerator<E> GetEnumerator() {
            return elements.GetEnumerator();
        }

        protected float priorityQuantisation = (float)(0.001);

        // superslow algorithm
        // value is [0, 1]
        /* commented because it's not used, code should work fine
	    protected final size_t sample(float value) {
		    int64_t absolutePriority = cast(int64_t)(value * cast(float)(prioritySumQuantisized));

		    int64_t accumulator = 0.0f;
		    for( size_t i = 0; i < elements.length; i++ ) {
			    // simulate what our bag does with the quantisation
			    unsigned quantisizedPriorityInt = quantisizePriority(elements[i].getPriority());
			    accumulator += quantisizedPriorityInt;
			
			    if (accumulator >= absolutePriority) {
				    return i;
			    }

			
		    }

		    return elements.length - 1;
	    }
	    */

        protected List<E> elements = new List<E>();

        protected ulong prioritySumQuantisized = 0;

        protected ulong maxSize;

        private Random rand = new Random();
    }
}
