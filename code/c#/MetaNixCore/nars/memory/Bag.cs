using System.Collections;
using System.Collections.Generic;

using MetaNix.misc;
using MetaNix.nars.config;
using MetaNix.nars.entity;
using MetaNix.nars.inference;

namespace MetaNix.nars.memory {
    // E must provide the method or UFCS function ".name()" to get the name/key
    public abstract class Bag<E, K> : IEnumerable<E> where E : Item<K> {
        
        public abstract void setMaxSize(ulong size);

        /**
         * Put an item back into the itemTable
         * <p>
         * The only place where the forgetting rate is applied
         *
         * \param oldItem The Item to put back
         * \return the item which was removed, or null if none removed
         */
        public E putBack(E oldItem, float forgetCycles, Memory m) {
            float relativeThreshold = Parameters.FORGET_QUALITY_RELATIVE;
            BudgetFunctions.applyForgetting(oldItem.budget, getForgetCycles(forgetCycles, oldItem), relativeThreshold);
            return putIn(oldItem);
        }

        /** allows adjusting forgetting rate in subclasses */
        public float getForgetCycles(float baseForgetCycles, E item) {
            return baseForgetCycles;
        }

        /**
         * Add a new Item into the Bag
         * if the same item already exists it gets merged
         *
         * \param newItem The new Item
         * \return the item which was removed, which may be the input item if it could not be inserted; or null if nothing needed removed
         */
        public E putIn(E newItem) {
            K newKey = newItem.name;

            E existingItemWithSameKey = take(newKey);

            if (existingItemWithSameKey != null ) {
                newItem = (E)existingItemWithSameKey.merge(newItem);
            }

            // put the (new or merged) item into itemTable        
            E overflowItem = addItem(newItem);


            if (overflowItem != null ) {
                return overflowItem;
            }
            else {
                return default(E);
            }
        }

        // returns an element without taking it out
        // is not allowed to be called on an empty bag
        public abstract E reference();


        // like reference but keeps it in the bag
        // returns null if item doesn't exit, is legal
        public abstract E referenceByKey(K key);

        // returns null if item doesn't exit, is legal
        public abstract E take(K key);

        public E takeElement(E value) {
            return take(value.name);
        }

        /**
         * Choose an Item according to distribution policy and take it out of the Bag
         * \return The selected Item, or null if this bag is empty
         */
        public abstract E takeNext();



        // value is [0, 1]
        //BagEntity reference(PriorityType value);

        // the number of items in the bag
        public abstract ulong size {
            get;
        }

        public abstract void clear();

        /**
         * Insert an item into the bag, and return the overflow
         *
         * \param newItem The Item to put in
         * \return The overflow Item, or null if nothing displaced
         */
        protected abstract E addItem(E newItem);

        public abstract IEnumerator<E> GetEnumerator();

        // Explicit interface implementation for nongeneric interface
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
