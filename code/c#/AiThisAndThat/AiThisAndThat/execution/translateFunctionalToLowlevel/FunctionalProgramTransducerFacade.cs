using System.Diagnostics;

using treeTransducer;
using execution.functional;

namespace execution.translateFunctionalToLowlevel {
    /**
     * used to translate the functional instructions to VLIW instructions
     * 
     */
    class FunctionalProgramTransducerFacade : ITreeManipulationFacade<FunctionalProgramElement> {
        override public bool isLeaf(FunctionalProgramElement element) {
            return element.children == null;
        }
        override public int getNumberOfChildren(FunctionalProgramElement element) {
            if( element.children == null ) {
                return 0;
            }

            return element.children.Count;
        }
        override public FunctionalProgramElement getChildren(FunctionalProgramElement element, int index) {
            return element.children[index];
        }
        override public void setChildren(FunctionalProgramElement element, int index, FunctionalProgramElement newChildrenValue) {
            element.children[index] = newChildrenValue;
        }
        override public ITreeManipulationFacade<FunctionalProgramElement>.EnumTreeElementType getType(FunctionalProgramElement element) {
            return
                element.type.value == FunctionalProgramElementType.EnumType.REWRITE_VARIABLE ?
                ITreeManipulationFacade<FunctionalProgramElement>.EnumTreeElementType.VARIABLE : // true
                ITreeManipulationFacade<FunctionalProgramElement>.EnumTreeElementType.VALUE;     // false
        }
        override public void setType(FunctionalProgramElement element, ITreeManipulationFacade<FunctionalProgramElement>.EnumTreeElementType type) {
            element.type.value = 
                (type == ITreeManipulationFacade<FunctionalProgramElement>.EnumTreeElementType.VARIABLE) ?
                FunctionalProgramElementType.EnumType.REWRITE_VARIABLE :
                FunctionalProgramElementType.EnumType.VARIABLE;
        }
        override public string getVariableName(FunctionalProgramElement element) {
            Debug.Assert(element.type.value == FunctionalProgramElementType.EnumType.REWRITE_VARIABLE);

            // the variablename is just the expression
            return element.expression;
        }
        override public bool isEqual(FunctionalProgramElement a, FunctionalProgramElement b) {
            return a == b;
        }
    }
}
