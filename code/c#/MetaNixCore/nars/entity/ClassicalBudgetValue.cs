using MetaNix.nars.config;
using MetaNix.nars.inference;
using System;

namespace MetaNix.nars.entity {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/entity/BudgetValue.java
    /**
     * A triple of priority (current), durability (decay), and quality (long-term average).
     */
    public class ClassicalBudgetValue {
        /** 
         * Constructor with initialization
         * @param p Initial priority
         * @param d Initial durability
         * @param q Initial quality
         */
        public ClassicalBudgetValue(float p, float d, float q) {
            priority = p;
            durability = d;
            quality = q;

            if (d >= 1.0) {
                //durability = (float)(1.0 - TRUTH_EPSILON);
                throw new Exception("durability value above or equal 1");
            }
            if (p > 1.0) {
                //priority = 1.0f;
                throw new Exception("priority value above 1");
            }
        }

        public float priority {
            get {
                return privatePriority;
            }
            set {
                if (value > 1.0f) {
                    throw new Exception(String.Format("Priority > 1.0: {0}", value));
                    //v=1.0f;
                }
                privatePriority = value;
            }
        }
        
        public float durability {
            get {
                return privateDurability;
            }

            /**
             * Change durability value
             * @param v The new durability
             */
            set {
                if (value >= 1.0f) {
                    value = 1.0f - Parameters.TRUTH_EPSILON;
                }
                privateDurability = value;
            }
        }
        

        public float quality {
            set {
                privateQuality = value;
            }

            /**
             * Get quality value
             * @return The current quality
             */
            get {
                return privateQuality;
            }
        }

        /**
         * Whether the budget should get any processing at all
         * <p>
         * to be revised to depend on how busy the system is
         * \return The decision on whether to process the Item
         */
        public bool isAboveThreshold { get {
            return summary >= Parameters.BUDGET_THRESHOLD;
        }}

        /**
         * To summarize a BudgetValue into a single number in [0, 1]
         * @return The summary value
         */
        public float summary { get {
            return UtilityFunctions.aveGeo(priority, durability, quality);
        }}


        /**
         * Merge one BudgetValue into another
         * @param that The other Budget
         */
        public void merge(ClassicalBudgetValue that) {
            BudgetFunctions.merge(this, that);
        }


        /**
         * Increase priority value by a percentage of the remaining range
         * \param v The increasing percent
         */
        public void incPriority(float v) {
            priority = ((float)Math.Min(1.0, UtilityFunctions.or(priority, v)));
        }

        /**
         * Decrease priority value by a percentage of the remaining range
         * \param v The decreasing percent
         */
        public void decPriority(float v) {
            priority = UtilityFunctions.and(priority, v);
        }

        /**
         * Decrease durability value by a percentage of the remaining range
         * \param v The decreasing percent
         */
        public void decDurability(float v) {
            durability = UtilityFunctions.and(durability, v);
        }

        public ClassicalBudgetValue clone() {
            return new ClassicalBudgetValue(priority, durability, quality);
        }



        /** The relative share of time resource to be allocated */
        private float privatePriority;
        

        /**
         * The percent of priority to be kept in a constant period; All priority
         * values "decay" over time, though at different rates. Each item is given a
         * "durability" factor in (0, 1) to specify the percentage of priority level
         * left after each reevaluation
         */
        private float privateDurability;

        /** The overall (context-independent) evaluation */
        private float privateQuality;
    }

}
