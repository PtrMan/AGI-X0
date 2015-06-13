package ptrman.agix0.src.java.Evolvator;

import org.uncommons.maths.random.MersenneTwisterRNG;
import org.uncommons.watchmaker.framework.*;
import org.uncommons.watchmaker.framework.selection.RouletteWheelSelection;
import org.uncommons.watchmaker.framework.termination.GenerationCount;
import ptrman.agix0.src.java.Common.SimulationContext;
import ptrman.agix0.src.java.Serialisation;

/**
 *
 */
public class Evolution {
    public static void main(String[] arguments) {
        final String pathToScript = "/home/r0b3/neuralExp/startup.js";

        SimulationContext simulationContext = new SimulationContext();

        simulationContext.loadAndExecuteSetupProcedures(pathToScript, false);

        NetworkCandidateFactory candidateFactory = new NetworkCandidateFactory(100);
        NetworkFitnessEvaluator fitnessEvaluator = new NetworkFitnessEvaluator(simulationContext);
        NetworkMutationOperator networkMutationOperator = new NetworkMutationOperator();
        SelectionStrategy<Object> selectionStrategy = new RouletteWheelSelection();

        AbstractEvolutionEngine<NetworkGeneticExpression> engine = new GenerationalEvolutionEngine<>(
                candidateFactory,
                networkMutationOperator,
                fitnessEvaluator,
                selectionStrategy,
                new MersenneTwisterRNG());

        engine.setSingleThreaded(true);

        engine.addEvolutionObserver(new EvolutionObserver<NetworkGeneticExpression>()
        {
            public void populationUpdate(PopulationData<? extends NetworkGeneticExpression> data)
            {
                System.out.printf("Generation %d: best candidate fitness: %s\n",
                        data.getGenerationNumber(),
                        data.getBestCandidateFitness());

                NetworkGeneticExpression bestCandidate = data.getBestCandidate();

                Serialisation.saveNetworkToFilepath(bestCandidate.networkDescriptor, "/tmp/neuroidGen" + Integer.toString(data.getGenerationNumber()) + "Candidate" + "0");

            }
        });


        NetworkGeneticExpression result = engine.evolve(500, 3, new GenerationCount(50000));
    }
}
