package ptrman.grailExperiment.func;

import com.syncleus.ferma.DelegatingFramedGraph;
import com.syncleus.ferma.FramedGraph;
import com.syncleus.grail.graph.GrailGraph;
import com.syncleus.grail.graph.TinkerGrailGraphFactory;

import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/**
 * Created by r0b3 on 16.06.15.
 */
public class Test {
    public static void main(String[] args) {
        final GrailGraph graph = new TinkerGrailGraphFactory().subgraph("0");

        Set<Class<?>> types = new HashSet<Class<?>>(Arrays.asList(new Class<?>[]{FuncVertex.class, FuncLogicalOperationVertex.class, FuncConstantVertex.class, FuncValueBindingEdge.class, FuncFunctionCallVertex.class}));
        FramedGraph framedGraph = new DelegatingFramedGraph(graph, true, types);

        // construct graph
        final FuncLogicalOperationVertex logicalOperationVertex = framedGraph.addFramedVertex(FuncLogicalOperationVertex.class);
        final FuncConstantVertex constant1Vertex = framedGraph.addFramedVertex(FuncConstantVertex.class);
        final FuncConstantVertex constant2Vertex = framedGraph.addFramedVertex(FuncConstantVertex.class);

        logicalOperationVertex.setOperation("||");

        constant1Vertex.setValue("5");
        constant2Vertex.setValue("6");



        framedGraph.addFramedEdge(logicalOperationVertex, constant1Vertex, "0", FuncValueBindingEdge.class);
        framedGraph.addFramedEdge(logicalOperationVertex, constant2Vertex, "1", FuncValueBindingEdge.class);


        final FuncFunctionCallVertex callVertex = framedGraph.addFramedVertex(FuncFunctionCallVertex.class);
        callVertex.setName("test");

        FuncValueBindingEdge edgeCallParameter0 = framedGraph.addFramedEdge(callVertex, logicalOperationVertex, "binding", FuncValueBindingEdge.class);
        edgeCallParameter0.setSite("0");

        String codeAsString = FuncOpenClCodeGenerator.generateCode(callVertex);

        int x = 0;
    }
}
