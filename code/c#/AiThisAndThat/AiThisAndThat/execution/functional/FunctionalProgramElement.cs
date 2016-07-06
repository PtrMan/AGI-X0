using System;
using System.Collections.Generic;

using helper;

namespace execution.functional {
    class FunctionalProgramElement : TreeElement<FunctionalProgramElementType>, ICloneable {
        public List<FunctionalProgramElement> children;
        public string expression;

        public string variableDatatype; // decoration, doesn't have to be set for all elements, depending on interpretation

        public FunctionalProgramElement(FunctionalProgramElementType type) : base(type) { }

        public Object Clone() {
            FunctionalProgramElement cloned = new FunctionalProgramElement(type);
            if(children != null) {
                cloned.children = new List<FunctionalProgramElement>();

                foreach (FunctionalProgramElement iterationChildren in children) {
                    cloned.children.Add((FunctionalProgramElement)iterationChildren.Clone());
                }
            }
            
            cloned.expression = (string)expression.Clone();
            if( variableDatatype != null ) {
                cloned.variableDatatype = (string)variableDatatype.Clone();
            }
            
            return cloned;
        }

        public static bool operator ==(FunctionalProgramElement a, FunctionalProgramElement b) {
            if (System.Object.ReferenceEquals(a, b)) {
                return true;
            }

            if( a.children != b.children ) {
                return false;
            }

            if (a.children != null && a.children.Count != b.children.Count) {
                return false;
            }

            if ( a.children == null || a.children.Count == 0 ) {
                return a.variableDatatype == b.variableDatatype && a.expression == b.expression;
            }

            if (a.children.Count != b.children.Count) {
                return false;
            }


            int chilrenCount = a.children.Count;
            for (int i = 0; i < chilrenCount; i++) {
                if (a.children[i] != b.children[i]) {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(FunctionalProgramElement a, FunctionalProgramElement b) {
            return !(a == b);
        }
    }

    class FunctionalProgramElementType {
        public FunctionalProgramElementType(EnumType value) {
            this.value = value;
        }

        public enum EnumType {
            VARIABLE,
            REWRITE_VARIABLE, // is a variable for an rewrite rule
            BRACE
        }
        
        public EnumType value;
    }
}
