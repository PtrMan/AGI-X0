package ptrman.mltoolset.pac;

import java.util.*;

/**
 * see http://pages.cs.wisc.edu/~shuchi/courses/787-F07/scribe-notes/lecture25.pdf
 */
public class PacDecisionLists {
    public static class ListEntry {
        public final boolean functionValue;
        public final List<Boolean> values;

        public ListEntry(final List<Boolean> values, final boolean functionValue) {
            this.values = values;
            this.functionValue = functionValue;
        }
    }

    public static class Rule {
        public static Rule createConditional(final int valueIndex, final boolean value) {
            return new Rule(true, valueIndex, value);
        }

        public static Rule createUnconditional(final boolean value) {
            return new Rule(false, -1, value);
        }

        private Rule(final boolean isCondition, final int valueIndex, final boolean value) {
            this.isCondition = isCondition;
            this.valueIndex = valueIndex;
            this.value = value;
        }

        final public boolean isCondition;
        final public int valueIndex;
        final public boolean value;
    }

    // see http://pages.cs.wisc.edu/~shuchi/courses/787-F07/scribe-notes/lecture25.pdf
    public static List<Rule> learn(final List<ListEntry> listEntries, Random random) {
        List<Rule> resultRules = new ArrayList<>();

        Set<ListEntry> remainingListEntries = new HashSet<>(listEntries);

        List<Integer> remainingAttributeIndices = enumerateValues(listEntries.get(0).values.size());

        for(;;) {
            if( remainingListEntries.size() == 1 ) {
                break;
            }

            // repeat until rule is output
            for(;;) {
                final int remainingAttributeIndicesIndex = random.nextInt(remainingAttributeIndices.size());
                final int remainingAttributeIndex = remainingAttributeIndices.get(remainingAttributeIndicesIndex);

                final boolean j = getRandomValue(remainingListEntries, remainingAttributeIndex, random);
                final Set<Boolean> bj = new HashSet<>(getFunctionValueWhereValueIsEqualTo(remainingListEntries, remainingAttributeIndex, j));

                if( bj.size() == 1 ) {
                    resultRules.add(Rule.createConditional(remainingAttributeIndex, j));

                    remainingAttributeIndices.remove(remainingAttributeIndicesIndex);
                    remainingListEntries.removeAll(getListEntriesWhereXiEquals(remainingListEntries, remainingAttributeIndex, j));

                    break;
                }
            }
        }

        // output last rule
        resultRules.add(Rule.createUnconditional((new ArrayList<>(remainingListEntries)).get(0).functionValue));

        return resultRules;
    }

    private static List<ListEntry> getListEntriesWhereXiEquals(final Set<ListEntry> entries, final int attributeIndex, final boolean value) {
        List<ListEntry> result = new ArrayList<>();

        for( final ListEntry iterationEntry : entries ) {
            if (iterationEntry.values.get(attributeIndex) == value) {
                result.add(iterationEntry);
            }
        }

        return result;
    }

    private static List<Boolean> getFunctionValueWhereValueIsEqualTo(final Set<ListEntry> entries, final int attributeIndex, final boolean value) {
        boolean trueSet = false;
        boolean falseSet = false;

        for( final ListEntry iterationEntry : entries ) {
            if( iterationEntry.values.get(attributeIndex) == value ) {
                if( iterationEntry.functionValue ) {
                    trueSet = true;
                }
                else {
                    falseSet = true;
                }
            }
        }

        List<Boolean> resultList = new ArrayList<>();
        if( trueSet ) {
            resultList.add(true);
        }
        if( falseSet ) {
            resultList.add(false);
        }

        return resultList;
    }

    private static boolean getRandomValue(final Set<ListEntry> entriesAsSet, final int attributeIndex, Random random) {
        final List<ListEntry> entries = new ArrayList<>(entriesAsSet);
        final int entrieIndex = random.nextInt(entries.size());
        return entries.get(entrieIndex).values.get(attributeIndex);
    }

    private static List<Integer> enumerateValues(final int size) {
        List<Integer> result = new ArrayList<>();

        for( int i = 0; i < size; i++ ) {
            result.add(i);
        }

        return result;
    }
}
