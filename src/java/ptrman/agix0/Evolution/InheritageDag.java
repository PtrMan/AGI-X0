package ptrman.agix0.Evolution;

import com.syncleus.dann.graph.AbstractDirectedEdge;
import com.syncleus.dann.graph.MutableDirectedAdjacencyGraph;

import java.util.ArrayList;
import java.util.List;

/**
 * Used to store the whole inheritage history of all individuals
 */
public class InheritageDag {
    public List<Inheritage> rootInheritages = new ArrayList<>();

    public static class InheritageEdge extends AbstractDirectedEdge<Inheritage> {
        public InheritageEdge(Inheritage source, Inheritage destination) {
            super(source, destination);
        }
    }

    public MutableDirectedAdjacencyGraph<Inheritage, InheritageEdge> graph = new MutableDirectedAdjacencyGraph<>();

}
