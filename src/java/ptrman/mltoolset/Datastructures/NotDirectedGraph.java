package ptrman.mltoolset.Datastructures;

import java.util.ArrayList;
import java.util.List;

public class NotDirectedGraph<Type> {
    public static class Element<Type>  {
        public Element(Type content) {
            this.content = content;
        }

        public List<Integer> parentIndices = new ArrayList<>();
        public List<Integer> childIndices = new ArrayList<>();
        public Type content;
    }

    public void addElement(Element<Type> element) {
        elements.add(element);
    }

    public List<NotDirectedGraph.Element<Type>> elements = new ArrayList<>();
}
