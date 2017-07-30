using MetaNix.nars.memory;

namespace MetaNix.nars.entity {
    // see https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/entity/Item.java
    /**
     * An item is an object that can be put into a Bag,
     * to participate in the resource competition of the system.
     *
     * It has a key and a budget. Cannot be cloned
     */
    public abstract class Item<K> : IMergeable, IBudgeted, INamed<K>, IDiscardable {
        public Item() {}

        public Item(ClassicalBudgetValue budget) {
            this.budget = budget;
        }

	    public ClassicalBudgetValue budget; /** The budget of the Item, consisting of 3 numbers */

        /** called when the item has been discarded */
        public virtual void wasDiscarded() {
        }

        /**
         * Get the current key
         * \return Current key value
         */
        public abstract K name {
            get;
        }

        ClassicalBudgetValue IBudgeted.budget {
            get {
                return budget;
            }
        }

        /**
         * Merge with another Item with identical key
         * \param that The Item to be merged
         * \return the resulting Item: this or that
         */
        /*nonfinal*/
        public IMergeable merge(IMergeable that) {
            budget.merge(((Item<K>)that).budget);
            return this;
        }
    }
}
