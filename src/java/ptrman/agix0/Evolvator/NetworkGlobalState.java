package ptrman.agix0.Evolvator;

import org.apache.commons.math3.distribution.EnumeratedDistribution;
import org.apache.commons.math3.util.Pair;
import ptrman.agix0.Evolution.InheritageDag;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.Datastructures.NeuronDescriptor;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

public class NetworkGlobalState {
    public static class GenerativeNeuronNetworkSettings {
        public GenerativeNeuronNetworkSettings(int neuronClusterNeuronsMin, int neuronClusterNeuronsMax, int neuronClusterCountMin, int neuronClusterCountMax) {
            this.neuronClusterNeuronsMin = neuronClusterNeuronsMin;
            this.neuronClusterNeuronsMax = neuronClusterNeuronsMax;
            this.neuronClusterCountMin = neuronClusterNeuronsMin;
            this.neuronClusterCountMax = neuronClusterNeuronsMax;
        }

        public List<Integer> getRandomNeuronFamily(Random random) {
            List<Integer> resultList = new ArrayList<>();

            final int countOfNeuronClusters = neuronClusterCountMin + random.nextInt(neuronClusterCountMax - neuronClusterCountMin);

            for( int neuronCluster = 0; neuronCluster < countOfNeuronClusters; neuronCluster++ ) {
                resultList.add(neuronClusterNeuronsMin + random.nextInt(neuronClusterNeuronsMax - neuronClusterNeuronsMin));
            }

            Collections.sort(resultList);

            return resultList;
        }

        public final int neuronClusterNeuronsMin;
        public final int neuronClusterNeuronsMax;

        public final int neuronClusterCountMin;
        public final int neuronClusterCountMax;
    }

    final static public EnumeratedDistribution<Integer> celularAutomataRules = getCelularAutomataRules();

    final static public InheritageDag inheritageDag = new InheritageDag();

    static public NeuronDescriptor templateNeuronDescriptor;
    static public NeuroidNetworkDescriptor templateNeuroidNetworkDescriptor;

    // global table with all rules for the network, is global to constrain the GA to "common" CA's
    public int[] celularAutomataRuleTable;

    public GenerativeNeuronNetworkSettings generativeNeuronNetworkSettings;

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
