package ptrman.agix0.SpinglassExplorer.Gui;

import com.mxgraph.model.mxGeometry;
import org.apache.commons.math3.linear.ArrayRealVector;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;
import ptrman.agix0.SpinglassExplorer.NeuralNetworkDisplayState;
import ptrman.mltoolset.Neuroid.Neuroid;

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

        Graphics offscreenGraphics;
        Image offscreen = null;
        Dimension d = size();

        offscreen = createImage(d.width, d.height);
        offscreenGraphics = offscreen.getGraphics();
        // clear the exposed area
        offscreenGraphics.setColor(getBackground());
        offscreenGraphics.fillRect(0, 0, d.width, d.height);



        for( int neuronI = 0; neuronI < networkState.integratedActiviationOfInputNeurons.length; neuronI++ ) {
            final float integratedActivation = networkState.integratedActiviationOfInputNeurons[neuronI];

            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForInputNeurons[neuronI].getGeometry();
            final ArrayRealVector neuronPosition = new ArrayRealVector(new double[]{geometryOfNeuron.getX(), geometryOfNeuron.getX()});

            drawNeuron(neuronPosition, integratedActivation, NEURON_RADIUS, offscreenGraphics);
        }

        for( int neuronI = 0; neuronI < networkState.integratedActiviationOfHiddenNeurons.length; neuronI++ ) {
            final float integratedActivation = networkState.integratedActiviationOfHiddenNeurons[neuronI];

            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForHiddenNeurons[neuronI].getGeometry();
            final ArrayRealVector neuronPosition = new ArrayRealVector(new double[]{geometryOfNeuron.getX(), geometryOfNeuron.getX()});

            drawNeuron(neuronPosition, integratedActivation, NEURON_RADIUS, offscreenGraphics);
        }

        // draw connections
        graphics.setColor(Color.BLACK);

        for( Neuroid.Helper.EdgeWeightTuple<Float> iterationConnection : neuroidNetworkDescriptor.connections ) {
            int[] destinationPositon = getPositionOfNeuron(iterationConnection.destinationAdress);
            int[] sourcePosition = getPositionOfNeuron(iterationConnection.sourceAdress);

            graphics.drawLine(sourcePosition[0], sourcePosition[1], destinationPositon[0], destinationPositon[1]);
        }

        graphics.drawImage(offscreen, 0, 0, this);

        graphics.dispose();
    }

    private static void drawNeuron(ArrayRealVector position, final float integratedActivation, final float radius, Graphics graphics) {
        final Color color = new Color(integratedActivation, 0.0f, 0.0f);

        graphics.setColor(color);
        graphics.fillOval((int) (position.getDataRef()[0] - radius), (int) (position.getDataRef()[1] - radius), (int) (radius * 2), (int) (radius * 2));
        graphics.setColor(Color.BLACK);
        graphics.drawOval((int) (position.getDataRef()[0] - radius), (int) (position.getDataRef()[1] - radius), (int) (radius * 2), (int) (radius * 2));
    }

    private int[] getPositionOfNeuron(Neuroid.Helper.EdgeWeightTuple.NeuronAdress neuronAdress) {
        if( neuronAdress.type == Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT ) {
            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForInputNeurons[neuronAdress.index].getGeometry();
            return new int[]{(int)geometryOfNeuron.getX(), (int)geometryOfNeuron.getX()};
        }
        else {
            final mxGeometry geometryOfNeuron = graphLayout.graphVerticesForHiddenNeurons[neuronAdress.index].getGeometry();
            return new int[]{(int)geometryOfNeuron.getX(), (int)geometryOfNeuron.getX()};
        }
    }

    public NeuralNetworkDisplayState networkState;
    public NeuronGraphLayout graphLayout; // TODO< replace with a abstraction for the positions/topology >
    public NeuroidNetworkDescriptor neuroidNetworkDescriptor;
}
