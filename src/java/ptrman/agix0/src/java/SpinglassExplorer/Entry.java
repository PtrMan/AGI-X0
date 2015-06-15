package ptrman.agix0.src.java.SpinglassExplorer;


import ptrman.agix0.src.java.Common.SimulationContext;
import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.NeuronGraphLayout;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.SingleNetworkCanvas;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.WindowContent;
import ptrman.agix0.src.java.UsageCases.CritterSimpleUsageCase;
import ptrman.agix0.src.java.UsageCases.IUsageCase;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

/**
 *
 */
public class Entry {
    public static void main(String[] args) {
        Entry entry = new Entry();
    }

    private class LoadSetupScriptHandler implements WindowContent.ILoadSetupScriptHandler {
        private final Entry entry;

        public LoadSetupScriptHandler(Entry entry) {
            this.entry = entry;
        }

        @Override
        public void load(String filepath) {
            entry.loadSetupScript(filepath);
        }
    }

    private class TimerActionListener implements ActionListener {
        private final Entry entry;

        public TimerActionListener(Entry entry) {
            this.entry = entry;
        }

        @Override
        public void actionPerformed(ActionEvent e) {
            entry.timestep();
        }
    }

    // called from timer or gui to advance one timestep
    private void timestep() {
        boolean[] stimulus = usageCase.beforeNeuroidSimationStepGetNeuroidInputForNextStep(simulationContext.environment, stepCounter);

        simulationContext.getComponent().setStimulus(stimulus);
        simulationContext.modelTimestep();

        // read out result and rate

        final boolean[] neuronActivation = simulationContext.getComponent().getActiviationOfNeurons();

        usageCase.afterNeuroidSimulationStep(simulationContext.environment, neuronActivation);

        simulationContext.environment.timestep();

        stepCounter++;

        neuralNetworkDisplayState.resetIntegratedActivation();
        neuralNetworkDisplayState.integrate(1, stimulus, neuronActivation);

        windowContent.actualizeAllDisplays();
    }


    // called from GUI code when the user wants to load a neural network (and let it simulate)
    private void loadSetupScript(String filepath) {
        simulationContext.loadAndExecuteSetupProcedures(filepath, true);

        neuralNetworkDescriptor = simulationContext.getComponent().getNeuralNetworkDescriptor();
        recalculateLayoutOfNeurons();

        neuralNetworkDisplayState.allocateAfterDescriptor(neuralNetworkDescriptor);
        networkCanvas.networkState = neuralNetworkDisplayState;

        // TODO< call the setupEnvironment from the context and update the gui relevant state, then simulate each step >
        networkAndComponentsLoaded = true;

        usageCase = new CritterSimpleUsageCase(simulationContext.environmentScriptingAccessor);

        simulationContext.setupEnvironment();

        timer = new Timer(50, new TimerActionListener(this));
        timer.start();
    }

    public Entry() {
        windowContent = new WindowContent(simulationContext.environment, new LoadSetupScriptHandler(this));
        networkCanvas = windowContent.networkCanvas;

        JFrame windowFrame = new JFrame("Spinglass explorer");

        Container panel = windowFrame.getContentPane();

        panel.setLayout(new GridLayout(1, 1));
        panel.add(windowContent);

        windowFrame.setSize(500, 500);
        windowFrame.setVisible(true);
    }

    private void recalculateLayoutOfNeurons() {
        NeuronGraphLayout neuronGraphLayout = new NeuronGraphLayout();

        neuronGraphLayout.repopulateAfterDescriptor(neuralNetworkDescriptor);

        networkCanvas.graphLayout = neuronGraphLayout;
    }

    private NeuroidNetworkDescriptor neuralNetworkDescriptor;

    private SingleNetworkCanvas networkCanvas;
    private NeuralNetworkDisplayState neuralNetworkDisplayState = new NeuralNetworkDisplayState();

    private SimulationContext simulationContext = new SimulationContext();

    WindowContent windowContent;

    private boolean networkAndComponentsLoaded = false;
    private IUsageCase usageCase;
    private int stepCounter = 0;

    private Timer timer;
}
