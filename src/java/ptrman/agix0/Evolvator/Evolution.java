package ptrman.agix0.Evolvator;

import org.uncommons.maths.random.MersenneTwisterRNG;
import org.uncommons.watchmaker.framework.*;
import org.uncommons.watchmaker.framework.selection.RouletteWheelSelection;
import org.uncommons.watchmaker.framework.termination.GenerationCount;
import ptrman.agix0.Common.SimulationContext;
import ptrman.agix0.Neuroids.Datastructures.NeuronDescriptor;

/**
 *
 */
public class Evolution {
    public static void main(String[] arguments) {
        final String pathToScript = "/home/r0b3/neuralExp/startup.js";

        SimulationContext simulationContext = new SimulationContext();

        simulationContext.loadAndExecuteSetupProcedures(pathToScript, false);



        NetworkGlobalState networkGlobalState = new NetworkGlobalState();
        // initialize networkGlobalState
        //  initialize templateNeuron with data
        NetworkGlobalState.templateNeuronDescriptor = new NeuronDescriptor();
        NetworkGlobalState.templateNeuronDescriptor.firingLatency = 2;

        // TODO< initialize networkGlobalState >

        /* old code pre 26.07.2015

        result.templateNeuronDescriptor.numberOfInputNeurons = 2;
        result.templateNeuronDescriptor.numberOfOutputNeurons = 2;

        result.templateNeuronDescriptor.neuronLatencyMin = 2;
        result.templateNeuronDescriptor.neuronLatencyMax = 20;

        result.templateNeuronDescriptor.neuronThresholdMin = 0.1f;
        result.templateNeuronDescriptor.neuronThresholdMax = 1.0f;

        // NeuronDescriptor/template neuron
        result.templateNeuronDescriptor.randomFiringPropability = 0.0f;
        result.templateNeuronDescriptor.firingThreshold = 0.5f;
         */


        NetworkCandidateFactory candidateFactory = new NetworkCandidateFactory(networkGlobalState);
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

                // 26.07.2015 uncommented because the indirect descriptor has to be stored
                //Serialisation.saveNetworkToFilepath(bestCandidate.networkDescriptor, "/tmp/neuroidGen" + Integer.toString(data.getGenerationNumber()) + "Candidate" + "0");

            }
        });


        NetworkGeneticExpression result = engine.evolve(300, 5, new GenerationCount(50000));
    }
}
