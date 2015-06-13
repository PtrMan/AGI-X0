package ptrman.agix0.src.java.Common.Scripting;

import org.apache.commons.math3.linear.ArrayRealVector;
import ptrman.agix0.src.java.Common.SimulationContext;

/**
 * used to restrict/simplyfy the functions used on the setup initialted from the setup script such as
 * * setting the Environment setup function which prepares the environment for the test/evolution of a module
 * * loading of the correspoding neural network/components and configuring them
 * * etc
 */
public class EntryScriptingAccessor {
    private final SimulationContext context;

    public EntryScriptingAccessor(SimulationContext context) {
        this.context = context;
    }

    /**
     * called from the startup script to the set functionname of the function in the script which is used to initialize the environement (with a possibly random configuration)
     *
     * @param functionname
     */
    public void setEnvironmentSetupFunction(String functionname) {
        context.environmentSetupJavascriptFunctionname = functionname;
    }


    // helpers
    public ArrayRealVector create2dArrayRealVector(float x, float y) {
        return new ArrayRealVector(new double[]{x, y});
    }
}
