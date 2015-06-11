package ptrman.agix0.src.java.Evolvator;


import org.apache.commons.math3.distribution.EnumeratedDistribution;
import org.apache.commons.math3.util.Pair;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;


public class NetworkGlobalSettings {
    final static public EnumeratedDistribution<Integer> celularAutomataRules = getCelularAutomataRules();

    // global table with all rules for the network, is global to constrain the GA to "common" CA's
    public int[] celularAutomataRuleTable;

    public void replaceRandomCelularAutomataRuleTableEntryWithRandomRule(Random random) {
        final int newRule = celularAutomataRules.sample();

        final int ruleTableIndex = random.nextInt(celularAutomataRuleTable.length);
        celularAutomataRuleTable[ruleTableIndex] = newRule;
    }

    private static EnumeratedDistribution<Integer> getCelularAutomataRules() {
        List<Pair<Integer, Double>> rules = new ArrayList<>();

        final int numberOfSpecialRules = 1;
        int numberOfRules = 0;

        for( int ruleNumber = 0; ruleNumber < 256; ruleNumber++ ) {
            final int langtonsDelta = ptrman.misc.Bits.countBitsSlow(ruleNumber);

            if( langtonsDelta > 1 && langtonsDelta < 7 ) {
                numberOfRules++;
            }
        }

        for( int ruleNumber = 0; ruleNumber < 256; ruleNumber++ ) {
            if( ruleNumber == 110 ) {// class IV
                rules.add(new Pair<>(ruleNumber, 0.3));
            }

            final int langtonsDelta = ptrman.misc.Bits.countBitsSlow(ruleNumber);


            // info about Regimes of the rules
            // https://www.youtube.com/watch?v=XBB_lOfsqQA
            // 3 5 Cellular Automata-Model Thinking-Scott E. Page

            // we wan't only the interesting rules
            // pick rules from the chaotic and systematic regimes
            if( langtonsDelta > 1 && langtonsDelta < 7 ) {
                rules.add(new Pair<>(ruleNumber, 1.0/(double)numberOfRules));
            }
        }

        return new EnumeratedDistribution(rules);
    }
}
