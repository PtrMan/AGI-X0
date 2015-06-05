package mltoolset.Datastructures;

import java.util.ArrayList;
import java.util.List;

public class DirectedGraph<Type> {
    public static class Element<Type> {
        public Element(Type content) {
            this.content = content;
        }

        public List<Integer> childIndices = new ArrayList<>();
        public Type content;
    }

    public void addElement(Element<Type> element) {
        elements.add(element);
    }

    public List<DirectedGraph.Element> elements = new ArrayList<>();
}
