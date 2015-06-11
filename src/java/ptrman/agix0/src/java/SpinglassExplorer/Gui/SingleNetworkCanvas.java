package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import com.mxgraph.model.mxGeometry;
import org.apache.commons.math3.linear.ArrayRealVector;
import ptrman.agix0.src.java.SpinglassExplorer.NeuralNetworkDisplayState;

import java.awt.*;

/**
 * Canvas which displays the state and connections for just one network
 *
 * this will fly out of the window with seh's library
 */
public class SingleNetworkCanvas extends Canvas {
    public void paint(Graphics graphics) {
        final float NEURON_RADIUS = 4.0f;

        if( networkState == null ) {
            return;
        }

        for( int neuronI = 0; neuronI < networkState.integratedActiviationOfInputNeurons.length; neuronI++ ) {
            final float integratedActivation = networkState.integratedActiviationOfInputNeurons[neuronI];

            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForInputNeurons[neuronI].getGeometry();
            final ArrayRealVector neuronPosition = new ArrayRealVector(new double[]{geometryOfNeuron.getX(), geometryOfNeuron.getX()});

            drawNeuron(neuronPosition, integratedActivation, NEURON_RADIUS, graphics);
        }

        for( int neuronI = 0; neuronI < networkState.integratedActiviationOfHiddenNeurons.length; neuronI++ ) {
            final float integratedActivation = networkState.integratedActiviationOfHiddenNeurons[neuronI];

            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForHiddenNeurons[neuronI].getGeometry();
            final ArrayRealVector neuronPosition = new ArrayRealVector(new double[]{geometryOfNeuron.getX(), geometryOfNeuron.getX()});

            drawNeuron(neuronPosition, integratedActivation, NEURON_RADIUS, graphics);
        }


        // TODO< draw connections >


        graphics.dispose();
    }

    private static void drawNeuron(ArrayRealVector position, final float integratedActivation, final float radius, Graphics graphics) {
        final Color color = new Color(integratedActivation, 0.0f, 0.0f);

        graphics.setColor(color);
        graphics.fillOval((int) (position.getDataRef()[0] - radius), (int) (position.getDataRef()[1] - radius), (int) (radius * 2), (int) (radius * 2));
    }

    public NeuralNetworkDisplayState networkState;
    public NeuronGraphLayout graphLayout; // TODO< replace with a abstraction for the positions/topology >
}
