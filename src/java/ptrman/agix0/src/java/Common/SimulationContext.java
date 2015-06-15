package ptrman.agix0.src.java.Common;

import ptrman.agix0.src.java.Common.Evironment.Environment;
import ptrman.agix0.src.java.Common.Scripting.EntryScriptingAccessor;
import ptrman.agix0.src.java.Common.Scripting.EnvironmentScriptingAccessor;
import ptrman.agix0.src.java.Common.Scripting.JavascriptEngine;
import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.src.java.Serialisation;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;

/**
 *
 */
public class SimulationContext {
    public String environmentSetupJavascriptFunctionname;

    private JavascriptEngine javascript = new JavascriptEngine();

    public Environment environment = new Environment();

    private EntryScriptingAccessor entryScriptingAccessor = new EntryScriptingAccessor(this);
    public EnvironmentScriptingAccessor environmentScriptingAccessor = new EnvironmentScriptingAccessor(environment);

    // for now just one Component
    // later is is a dag / graph(?) which describes the dataflow between the components
    private Component onlyComponentOfModel;

    public SimulationContext() {
        javascript.addBinding("entryScriptingAccessor", entryScriptingAccessor);
        javascript.addBinding("environmentSetupScriptingAccessor", environmentScriptingAccessor);

        environment.environmentScriptingAccessor = environmentScriptingAccessor;
    }

    public void setupEnvironment() {
        environment.reset();

        javascript.invokeFunction(environmentSetupJavascriptFunctionname, Arrays.asList(new Object[]{environment}));
    }

    public void loadAndExecuteSetupProcedures(String scriptFilepath, boolean loadNeuralNetwork) {
        javascript.loadFile(scriptFilepath);
        javascript.invokeFunction("loaded", new ArrayList<>());

        if( !loadNeuralNetwork ) {
            return;
        }

        Path completePath = Paths.get(scriptFilepath);
        String pathToNeualNetworks = completePath.getParent().toString();

        // TODO< search the last network generation and load it >

        String nameOfLatestNeuralNetwork = "neuroidGen260Candidate0";

        String pathToNeuralNetworkToLoad = pathToNeualNetworks + "/" + nameOfLatestNeuralNetwork;

        NeuroidNetworkDescriptor neuralNetworkDescriptor = Serialisation.loadNetworkFromFilepath(pathToNeuralNetworkToLoad);

        setComponent(new Component());
        getComponent().setupNeuroidNetwork(neuralNetworkDescriptor);
    }

    public void modelTimestep() {
        onlyComponentOfModel.timestep();
    }

    // implemented just for one component
    // TODO< for many components a location of a component must be given >
    public Component getComponent() {
        return onlyComponentOfModel;
    }

    // implemented just for one component
    // TODO< for many components a location of a component must be given >
    public void setComponent(Component component) {
        this.onlyComponentOfModel = component;
    }
}
