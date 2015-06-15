package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import com.mxgraph.layout.mxFastOrganicLayout;
import com.mxgraph.layout.mxIGraphLayout;
import com.mxgraph.model.mxCell;
import com.mxgraph.view.mxGraph;
import ptrman.agix0.src.java.Datastructures.NeuroidNetworkDescriptor;
import ptrman.mltoolset.Neuroid.Neuroid;

// TODO< integrate/use this from/into seh's library for the display of topologies and modules >
/**
 *
 */
public class NeuronGraphLayout {
    public mxGraph graph = new mxGraph();

    public mxCell[] graphVerticesForHiddenNeurons;
    public mxCell[] graphVerticesForInputNeurons;

    public NeuronGraphLayout() {
    }

    public void repopulateAfterDescriptor(NeuroidNetworkDescriptor descriptor) {
        clear();
        populateAfterDescriptor(descriptor);
    }

    public void clear() {
        graph.removeCells(graph.getChildVertices(graph.getDefaultParent()));
    }

    private void populateAfterDescriptor(NeuroidNetworkDescriptor descriptor) {
        graph.getModel().beginUpdate();

        Object graphParent = graph.getDefaultParent();

        {
            graphVerticesForInputNeurons = new mxCell[descriptor.numberOfInputNeurons];
            graphVerticesForHiddenNeurons = new mxCell[descriptor.getNumberOfHiddenNeurons()];

            for( int i = 0; i < descriptor.numberOfInputNeurons; i++ ) {
                graphVerticesForInputNeurons[i] = (mxCell)graph.insertVertex(graphParent, null, "I" + "[" + Integer.toString(i) + "]", 20, 20, 20, 20);
            }

            for( int i = 0; i < descriptor.getNumberOfHiddenNeurons(); i++ ) {
                graphVerticesForHiddenNeurons[i] = (mxCell)graph.insertVertex(graphParent, null, "H" + "[" + Integer.toString(i) + "]", 20, 20, 20, 20);
            }
        }

        {
            for( final Neuroid.Helper.EdgeWeightTuple<Float> iterationEdge : descriptor.connections ) {
                Object sourceVertex, destinationVertex;

                if( iterationEdge.sourceIndex.type == Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT ) {
                    sourceVertex = graphVerticesForInputNeurons[iterationEdge.sourceIndex.index];
                }
                else {
                    sourceVertex = graphVerticesForHiddenNeurons[iterationEdge.sourceIndex.index];
                }

                if( iterationEdge.destinationIndex.type == Neuroid.Helper.EdgeWeightTuple.NeuronAdress.EnumType.INPUT ) {
                    destinationVertex = graphVerticesForInputNeurons[iterationEdge.destinationIndex.index];
                }
                else {
                    destinationVertex = graphVerticesForHiddenNeurons[iterationEdge.destinationIndex.index];
                }

                graph.insertEdge(graphParent, null, "->", sourceVertex, destinationVertex);
                // back edge just for better clustering
                graph.insertEdge(graphParent, null, "->", destinationVertex, sourceVertex);

            }
        }

        graph.getModel().endUpdate();


        mxIGraphLayout layout = new mxFastOrganicLayout(graph);

        layout.execute(graph.getDefaultParent());
    }
}
