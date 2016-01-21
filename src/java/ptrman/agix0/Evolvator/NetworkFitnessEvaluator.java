package ptrman.agix0.Evolvator;

import org.uncommons.watchmaker.framework.FitnessEvaluator;
import ptrman.agix0.Common.Component;
import ptrman.agix0.Common.SimulationContext;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.Neuroids.NeuroidNetworkManipulation.GenerativeNeuroidNetworkTransformator;
import ptrman.agix0.UsageCases.CritterSimpleUsageCase;
import ptrman.agix0.UsageCases.IUsageCase;
import ptrman.agix0.UsageCases.NullUsageCase;

import java.util.List;

import static java.lang.Float.max;
import static java.lang.Math.abs;


public class NetworkFitnessEvaluator implements FitnessEvaluator<NetworkGeneticExpression> {
    public NetworkFitnessEvaluator(SimulationContext simulationContext) {
        this.simulationContext = simulationContext;
    }

    @Override
    public double getFitness(NetworkGeneticExpression networkGeneticExpression, List<? extends NetworkGeneticExpression> list) {




        final float CONNECTION_PENELIZE = 0.08f; // how much does a connection cost?
        final float NEURON_PENELIZE = 0.2f; // how much does a neuron cost?

        //final int numberOfInputNeurons = 1;





        float fitness = 5000.0f;

        // evaluate how many times the output neuron (neuron 0) got stimulated

        /*
        Neuroid<Float, Integer> neuroid = new Neuroid<>(new Neuroid.FloatWeighttypeHelper());
        neuroid.update = new Update(latencyAfterActivation, randomFiringPropability);

        neuroid.allocateNeurons(networkGeneticExpression.networkDescriptor.getNumberOfHiddenNeurons(), numberOfInputNeurons);
        neuroid.input = new boolean[numberOfInputNeurons];

        for( int neuronI = 0; neuronI < networkGeneticExpression.networkDescriptor.getNumberOfHiddenNeurons(); neuronI++ ) {
            neuroid.getGraph().neuronNodes[neuronI].graphElement.threshold = networkGeneticExpression.networkDescriptor.hiddenNeurons[neuronI].firingThreshold;
        }

        neuroid.addEdgeWeightTuples(networkGeneticExpression.networkDescriptor.connections);

        neuroid.initialize();
        */

        final NeuroidNetworkDescriptor neuroidNetworkFromIndirectEncoding = GenerativeNeuroidNetworkTransformator.generateNetwork(NetworkGlobalState.templateNeuroidNetworkDescriptor, networkGeneticExpression.generativeNeuroidNetworkDescriptor);

        simulationContext.setComponent(new Component());

        simulationContext.getComponent().setupNeuroidNetwork(neuroidNetworkFromIndirectEncoding);

        IUsageCase usageCase = new NullUsageCase(); //new CritterSimpleUsageCase(simulationContext.environmentScriptingAccessor);

        final int numberOfNeuralSimulationSteps = usageCase.getNumberOfNeuralSimulationSteps();

        /*
        Environment environment = new Environment();
        environment.entities.add(new Entity());
        environment.entities.get(0).position = new ArrayRealVector(new double[]{0.0, 0.0});
        environment.entities.get(0).direction = new ArrayRealVector(new double[]{1.0, 0.0});
         */
        simulationContext.setupEnvironment();

        // simulate network
        // together with the environment
        for( int timestep = 0; timestep < numberOfNeuralSimulationSteps; timestep++ ) {
            // stimulate

            simulationContext.getComponent().setStimulus(usageCase.beforeNeuroidSimationStepGetNeuroidInputForNextStep(simulationContext.environment, timestep));
            simulationContext.modelTimestep();

            // read out result and rate

            final boolean[] neuronActivation = simulationContext.getComponent().getActiviationOfNeurons();

            usageCase.afterNeuroidSimulationStep(simulationContext.environment, neuronActivation);

            simulationContext.environment.timestep();
        }

        // just for testing, reward for rotation
        ///fitness += (abs(simulationContext.environment.entities.get(0).angle2d) * 10.0f);

        // reward for traveled distance
        ///fitness += simulationContext.environment.entities.get(0).body.body.getPosition().length();

        //System.out.println(networkGeneticExpression.connectionsWithWeights.size());

        fitness -= ((float)neuroidNetworkFromIndirectEncoding.connections.size() * CONNECTION_PENELIZE);

        fitness -= ((float)neuroidNetworkFromIndirectEncoding.hiddenNeurons.length * NEURON_PENELIZE);

        //System.out.println(networkGeneticExpression.getEnabledNeurons());

        fitness = max(fitness, 0.0f);

        return fitness;
    }

    @Override
    public boolean isNatural() {
        return true;
    }

    private SimulationContext simulationContext;
}
