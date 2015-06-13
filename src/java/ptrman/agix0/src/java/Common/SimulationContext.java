package ptrman.agix0.src.java.Common;

import ptrman.agix0.src.java.Common.Evironment.Environment;
import ptrman.agix0.src.java.Common.Scripting.EntryScriptingAccessor;
import ptrman.agix0.src.java.Common.Scripting.EnvironmentScriptingAccessor;
import ptrman.agix0.src.java.Common.Scripting.JavascriptEngine;

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
    private EnvironmentScriptingAccessor environmentScriptingAccessor = new EnvironmentScriptingAccessor(environment);

    // for now just one Component
    // later is is a dag / graph(?) which describes the dataflow between the components
    private Component onlyComponentOfModel;

    public SimulationContext() {
        javascript.addBinding("entryScriptingAccessor", entryScriptingAccessor);
        javascript.addBinding("environmentSetupScriptingAccessor", environmentScriptingAccessor);
    }

    public void setupEnvironment() {
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

        int debug = 0;
        // TODO

        // TODO< search the last network generation and load it >
        // TODO TODO TODO TODO TODO
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
