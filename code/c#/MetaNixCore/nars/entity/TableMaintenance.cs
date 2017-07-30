using System.Collections.Generic;

using MetaNix.nars.inference;

namespace MetaNix.nars.entity {
    public static class TableMaintenance {
        // from https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/Concept.java#L345
        public static void addToTable(ClassicalTask task, bool rankTruthExpectation, IList<ClassicalTask> table, uint max, /*final Class eventAdd*/object eventAdd/*final Class eventRemove, */, object eventRemove/*final Object... extraEventArguments*/) {
            int preSize = table.Count;
            ClassicalTask removedT;
            ClassicalSentence removed = null;
            removedT = addToTable(task, table, max, rankTruthExpectation);
            if (removedT != null) {
                removed = removedT.sentence;
            }

            /*
            if (removed != null) {
                memory.event.emit(eventRemove, this, removed, task, extraEventArguments);
            }
            if ((preSize != table.size()) || (removed != null)) {
                memory.event.emit(eventAdd, this, task, extraEventArguments);
            }
             */
        }

        // from https://github.com/opennars/opennars/blob/62c814fb0f3e474a176515103394049b2887ec29/nars_core/nars/entity/Concept.java#L740
        /**
         * Add a new belief (or goal) into the table Sort the beliefs/desires by
         * rank, and remove redundant or low rank one
         *
         * \param newSentence The judgment to be processed
         * \param table The table to be revised
         * \param capacity The capacity of the table
         * \return whether table was modified
         */
        public static ClassicalTask addToTable(ClassicalTask newTask, IList<ClassicalTask> table, uint capacity, bool rankTruthExpectation) {
            ClassicalSentence newSentence = newTask.sentence;
            float rank1 = BudgetFunctions.rankBelief(newSentence, rankTruthExpectation);    // for the new isBelief
            float rank2;
            int i;
            for (i = 0; i < table.Count; i++) {
                ClassicalSentence judgment2 = table[i].sentence;
                rank2 = BudgetFunctions.rankBelief(judgment2, rankTruthExpectation);
                if (rank1 >= rank2) {
                    if (newSentence.checkEquivalentTo(judgment2)) {
                        //System.out.println(" ---------- Equivalent Belief: " + newSentence + " == " + judgment2);
                        return null;
                    }
                    table.Insert(i, newTask);
                    break;
                }
            }

            if (table.Count == capacity) {
                // nothing
            }
            else if (table.Count > capacity) {
                ClassicalTask removed = table[table.Count - 1];
                table.RemoveAt(table.Count - 1);
                return removed;
            }
            else if (i == table.Count) { // branch implies implicit table.size() < capacity
                table.Add(newTask);
            }

            return null;
        }


    }
}
