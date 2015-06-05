package mltoolset.Usage.NetworkEvolvator;

import org.uncommons.maths.random.MersenneTwisterRNG;
import org.uncommons.watchmaker.framework.*;
import org.uncommons.watchmaker.framework.selection.RouletteWheelSelection;
import org.uncommons.watchmaker.framework.termination.GenerationCount;

/**
 * Created by r0b3 on 02.06.15.
 */
public class NetworkEvolvator {
    public static void main(String[] args) {
        NetworkCandidateFactory candidateFactory = new NetworkCandidateFactory(100);
        NetworkFitnessEvaluator fitnessEvaluator = new NetworkFitnessEvaluator();
        NetworkMutationOperator networkMutationOperator = new NetworkMutationOperator();
        SelectionStrategy<Object> selectionStrategy = new RouletteWheelSelection();

        EvolutionEngine<NetworkGeneticExpression> engine = new GenerationalEvolutionEngine<>(
                candidateFactory,
                networkMutationOperator,
                fitnessEvaluator,
                selectionStrategy,
                new MersenneTwisterRNG());

        engine.addEvolutionObserver(new EvolutionObserver<NetworkGeneticExpression>()
        {
            public void populationUpdate(PopulationData<? extends NetworkGeneticExpression> data)
            {
                System.out.printf("Generation %d: best candidate fitness: %s\n",
                        data.getGenerationNumber(),
                        data.getBestCandidateFitness());
            }
        });

        NetworkGeneticExpression result = engine.evolve(500, 3, new GenerationCount(50000));
        //System.out.println(result);
    }
}
