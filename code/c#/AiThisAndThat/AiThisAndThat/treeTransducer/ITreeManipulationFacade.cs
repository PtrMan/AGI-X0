using System;
using System.Collections.Generic;

namespace treeTransducer {
    /**
     * Adds an layer of abstraction over the representation of the tree (which depends on the domain) and the manipulation of it (which is unique to tree transducers)
     * 
     */
    abstract class ITreeManipulationFacade<ElementType> {
        public enum EnumTreeElementType {
            VARIABLE,
            VALUE
        }

        abstract public bool isLeaf(ElementType element);
        abstract public int getNumberOfChildren(ElementType element);
        abstract public ElementType getChildren(ElementType element, int index);
        abstract public void setChildren(ElementType element, int index, ElementType newChildrenValue);
        abstract public EnumTreeElementType getType(ElementType element);
        abstract public void setType(ElementType element, EnumTreeElementType type);

        // gets the variable name of an rule element which is later rewritten
        abstract public string getVariableName(ElementType element);

        abstract public bool isEqual(ElementType a, ElementType b);
    }
}
