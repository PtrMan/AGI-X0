package ptrman.agix0.src.java.SpinglassExplorer;


import ptrman.agix0.src.java.Common.SimulationContext;
import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.src.java.Serialisation;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.NeuronGraphLayout;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.SingleNetworkCanvas;
import ptrman.agix0.src.java.SpinglassExplorer.Gui.WindowContent;

import javax.swing.*;
import java.awt.*;

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


    // called from GUI code when the user wants to load a neural network (and let it simulate)
    private void loadSetupScript(String filepath) {
        simulationContext.loadAndExecuteSetupProcedures(filepath, true);


        neuralNetworkDescriptor = Serialisation.loadNetworkFromFilepath(filepath);
        recalculateLayoutOfNeurons();

        neuralNetworkDisplayState.allocateAfterDescriptor(neuralNetworkDescriptor);
        networkCanvas.networkState = neuralNetworkDisplayState;

        // TODO< call the setupEnvironment from the context and update the gui relevant state, then simulate each step >
    }

    public Entry() {



        // just for testing
        //writeTestNetwork();

        WindowContent content = new WindowContent(simulationContext.environment, new LoadSetupScriptHandler(this));
        networkCanvas = content.networkCanvas;

        JFrame windowFrame = new JFrame("Spinglass explorer");

        Container panel = windowFrame.getContentPane();

        panel.setLayout(new GridLayout(1, 1));
        panel.add(content);

        windowFrame.setSize(500, 500);
        windowFrame.setVisible(true);
    }

    private void recalculateLayoutOfNeurons() {
        NeuronGraphLayout neuronGraphLayout = new NeuronGraphLayout();

        neuronGraphLayout.repopulateAfterDescriptor(neuralNetworkDescriptor);

        networkCanvas.graphLayout = neuronGraphLayout;
    }

    // just for testing
    /*
    private static void writeTestNetwork() {
        NeuroidNetworkDescriptor testNetwork = new NeuroidNetworkDescriptor();
        testNetwork.connections.add(new Neuroid.Helper.EdgeWeightTuple<>(new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(0, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT), new Neuroid.Helper.EdgeWeightTuple.NeuronAdress(1, Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.HIDDEN), 0.5f));
        testNetwork.hiddenNeurons = new NeuronDescriptor[50];

        testNetwork.numberOfInputNeurons = 1;

        Serialisation.saveNetworkToFilepath(testNetwork, "/tmp/network.json");
    }
    */

    private NeuroidNetworkDescriptor neuralNetworkDescriptor;

    private SingleNetworkCanvas networkCanvas;
    private NeuralNetworkDisplayState neuralNetworkDisplayState = new NeuralNetworkDisplayState();

    private SimulationContext simulationContext = new SimulationContext();
}
